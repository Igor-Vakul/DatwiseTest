using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Web.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SafetyPortal.Web.Models;

namespace SafetyPortal.Web
{
    /// <summary>
    /// Typed HTTP client for the SafetyPortal Minimal API.
    /// Uses the shared Global.HttpClient but adds JWT per-request via HttpRequestMessage
    /// to avoid thread-safety issues with DefaultRequestHeaders.
    /// </summary>
    public class ApiClient
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            ContractResolver     = new CamelCasePropertyNamesContractResolver(),
            DateFormatHandling   = DateFormatHandling.IsoDateFormat,
            NullValueHandling    = NullValueHandling.Ignore
        };

        private static readonly JsonSerializerSettings DeserializeSettings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            },
            DateFormatHandling = DateFormatHandling.IsoDateFormat
        };

        private readonly string _base;
        private readonly string _token;

        public ApiClient(string jwtToken = null)
        {
            _base  = (WebConfigurationManager.AppSettings["ApiBaseUrl"] ?? "https://localhost:7182").TrimEnd('/');
            _token = jwtToken;
        }

        // ── Auth ──────────────────────────────────────────────────────────
        public LoginResponse Login(string email, string password)
        {
            var body = JsonConvert.SerializeObject(new { Email = email, Password = password });
            var resp = SendRaw(HttpMethod.Post, "/api/auth/login", body, withAuth: false);
            return resp == null ? null : JsonConvert.DeserializeObject<LoginResponse>(resp, DeserializeSettings);
        }

        // ── Dashboard ─────────────────────────────────────────────────────
        public DashboardStats GetDashboardStats()
            => Get<DashboardStats>("/api/dashboard/stats");

        // ── Incidents ─────────────────────────────────────────────────────
        public PagedResult<IncidentSummary> GetIncidents(
            int page = 1, int pageSize = AppConstants.Pagination.DefaultPageSize,
            string search = null, string status = null,
            string severityLevel = null, int? departmentId = null, int? categoryId = null,
            bool archived = false)
        {
            var qs = BuildQuery(
                ("page",          page.ToString()),
                ("pageSize",      pageSize.ToString()),
                ("search",        search),
                ("status",        status),
                ("severityLevel", severityLevel),
                ("departmentId",  departmentId?.ToString()),
                ("categoryId",    categoryId?.ToString()),
                ("archived",      archived.ToString().ToLower())
            );
            return Get<PagedResult<IncidentSummary>>($"/api/incidents{qs}");
        }

        public bool ToggleIncidentArchive(int id)
            => Put($"/api/incidents/{id}/archive", (object)null);

        public IncidentDetail GetIncident(int id)
            => Get<IncidentDetail>($"/api/incidents/{id}");

        public bool CreateIncident(CreateIncidentRequest req)
            => Post("/api/incidents", req);

        // Returns the new incident ID, or null on failure
        public int? CreateIncidentForId(CreateIncidentRequest req)
        {
            var json = SendRaw(HttpMethod.Post, "/api/incidents",
                               JsonConvert.SerializeObject(req, Settings));
            if (json == null) return null;
            var result = JsonConvert.DeserializeObject<CreateIncidentResult>(json, DeserializeSettings);
            return result?.Id;
        }

        // Upload a single file attachment; returns error message or null on success
        public string UploadAttachment(int incidentId, byte[] content, string fileName, string contentType)
        {
            try
            {
                var form        = new MultipartFormDataContent();
                var fileContent = new ByteArrayContent(content);
                fileContent.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
                form.Add(fileContent, "file", fileName);

                var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    $"{_base}/api/incidents/{incidentId}/attachments");

                if (!string.IsNullOrEmpty(_token))
                    request.Headers.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);

                request.Content = form;

                var response = System.Threading.Tasks.Task
                    .Run(() => Global.HttpClient.SendAsync(request))
                    .GetAwaiter().GetResult();

                if (response.IsSuccessStatusCode) return null;

                var body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                try
                {
                    dynamic err = JsonConvert.DeserializeObject(body);
                    return err?.error?.ToString() ?? $"Upload failed ({response.StatusCode}).";
                }
                catch
                {
                    return $"Upload failed ({response.StatusCode}).";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public List<AttachmentInfo> GetAttachments(int incidentId)
            => Get<List<AttachmentInfo>>($"/api/incidents/{incidentId}/attachments");

        // Downloads an Excel export and writes it directly to the HTTP response.
        // Returns false if the API returned a non-2xx response.
        public bool ProxyExport(string apiPath, System.Web.HttpResponse response)
        {
            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, $"{_base}{apiPath}"))
                {
                    if (!string.IsNullOrEmpty(_token))
                        request.Headers.Authorization =
                            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);

                    var apiResponse = System.Threading.Tasks.Task
                        .Run(() => Global.HttpClient.SendAsync(request, System.Net.Http.HttpCompletionOption.ResponseHeadersRead))
                        .GetAwaiter().GetResult();

                    if (!apiResponse.IsSuccessStatusCode) return false;

                    var disposition = apiResponse.Content.Headers.ContentDisposition;
                    var fileName    = disposition?.FileNameStar ?? disposition?.FileName
                                      ?? "export.xlsx";

                    response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    response.AddHeader("Content-Disposition", $"attachment; filename=\"{fileName}\"");

                    var stream = apiResponse.Content.ReadAsStreamAsync().GetAwaiter().GetResult();
                    stream.CopyTo(response.OutputStream);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public bool DeleteAttachment(int incidentId, int attachmentId)
            => Delete($"/api/incidents/{incidentId}/attachments/{attachmentId}");

        // Downloads a file and writes it directly to the given HTTP response.
        // Returns false if the API returned a non-2xx response.
        public bool ProxyDownload(int incidentId, int attachmentId, System.Web.HttpResponse response)
        {
            try
            {
                using (var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    $"{_base}/api/incidents/{incidentId}/attachments/{attachmentId}/download"))
                {
                    if (!string.IsNullOrEmpty(_token))
                        request.Headers.Authorization =
                            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);

                    var apiResponse = System.Threading.Tasks.Task
                        .Run(() => Global.HttpClient.SendAsync(request, System.Net.Http.HttpCompletionOption.ResponseHeadersRead))
                        .GetAwaiter().GetResult();

                    if (!apiResponse.IsSuccessStatusCode) return false;

                    var contentType = apiResponse.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
                    var disposition = apiResponse.Content.Headers.ContentDisposition;
                    var fileName    = disposition?.FileNameStar ?? disposition?.FileName ?? "download";

                    response.ContentType = contentType;
                    response.AddHeader("Content-Disposition", $"attachment; filename=\"{fileName}\"");

                    var stream = apiResponse.Content.ReadAsStreamAsync().GetAwaiter().GetResult();
                    stream.CopyTo(response.OutputStream);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public bool UpdateIncident(int id, UpdateIncidentRequest req)
            => Put($"/api/incidents/{id}", req);

        public bool DeleteIncident(int id)
            => Delete($"/api/incidents/{id}");

        // ── Corrective Actions ────────────────────────────────────────────
        public System.Collections.Generic.List<CorrectiveActionSummary> GetCorrectiveActions(int? reportId = null, string status = null)
        {
            var qs = BuildQuery(
                ("reportId", reportId?.ToString()),
                ("status",   status)
            );
            return Get<System.Collections.Generic.List<CorrectiveActionSummary>>($"/api/corrective-actions{qs}");
        }

        public bool CreateCorrectiveAction(CreateCorrectiveActionRequest req)
            => Post("/api/corrective-actions", req);

        public bool UpdateActionStatus(int id, string status)
            => Put($"/api/corrective-actions/{id}/status", new { Status = status });

        public bool DeleteCorrectiveAction(int id)
            => Delete($"/api/corrective-actions/{id}");

        // ── Lookups ───────────────────────────────────────────────────────
        public List<DepartmentItem>  GetDepartments() => Get<List<DepartmentItem>>("/api/lookup/departments");
        public List<CategoryItem>    GetCategories()  => Get<List<CategoryItem>>("/api/lookup/categories");
        public List<UserLookupItem>  GetUsers()       => Get<List<UserLookupItem>>("/api/lookup/users");
        public List<RoleItem>        GetRoles()       => Get<List<RoleItem>>("/api/lookup/roles");

        // ── Admin: Departments ────────────────────────────────────────────
        public List<DepartmentAdminItem> GetAllDepartments()
            => Get<List<DepartmentAdminItem>>("/api/admin/departments");

        public bool CreateDepartment(string name, string locationName, string color)
            => Post("/api/admin/departments", new { Name = name, LocationName = locationName, Color = color });

        public bool UpdateDepartment(int id, string name, string locationName, string color, bool isActive)
            => Put($"/api/admin/departments/{id}", new { Name = name, LocationName = locationName, Color = color, IsActive = isActive });

        public bool DeleteDepartment(int id)
            => Delete($"/api/admin/departments/{id}");

        // ── Admin: Categories ─────────────────────────────────────────────
        public List<CategoryAdminItem> GetAllCategories()
            => Get<List<CategoryAdminItem>>("/api/admin/categories");

        public bool CreateCategory(string name, string description)
            => Post("/api/admin/categories", new { Name = name, Description = description });

        public bool UpdateCategory(int id, string name, string description)
            => Put($"/api/admin/categories/{id}", new { Name = name, Description = description });

        public bool DeleteCategory(int id)
            => Delete($"/api/admin/categories/{id}");

        // ── User Management ───────────────────────────────────────────────
        public List<UserSummary>  GetAllUsers()                              => Get<List<UserSummary>>("/api/users");
        public bool CreateUser(CreateUserRequest req)                         => Post("/api/users", req);
        public bool UpdateUser(int id, UpdateUserRequest req)                 => Put($"/api/users/{id}", req);
        public bool ToggleUserActive(int id)                                  => Put($"/api/users/{id}/toggle-active", new { });
        public bool SendEmailToUser(int id, string subject, string body)      => Post($"/api/users/{id}/send-email", new { Subject = subject, Body = body });

        // ── Helpers ───────────────────────────────────────────────────────
        private T Get<T>(string url)
        {
            var json = SendRaw(HttpMethod.Get, url);
            return json == null ? default(T) : JsonConvert.DeserializeObject<T>(json, DeserializeSettings);
        }

        private bool Post(string url, object body)
            => SendRaw(HttpMethod.Post, url, JsonConvert.SerializeObject(body, Settings)) != null;

        private bool Put(string url, object body)
            => SendRaw(HttpMethod.Put, url, body != null ? JsonConvert.SerializeObject(body, Settings) : "{}") != null;

        private bool Delete(string url)
            => SendRaw(HttpMethod.Delete, url) != null;

        /// <summary>
        /// Sends a request using the shared HttpClient.
        /// Auth token is added per-request via HttpRequestMessage (thread-safe).
        /// Returns null on non-2xx response.
        /// </summary>
        private string SendRaw(HttpMethod method, string path, string jsonBody = null, bool withAuth = true)
        {
            using (var request = new HttpRequestMessage(method, $"{_base}{path}"))
            {
                if (withAuth && !string.IsNullOrEmpty(_token))
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

                if (jsonBody != null)
                    request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                HttpResponseMessage response;
                try
                {
                    response = System.Threading.Tasks.Task
                        .Run(() => Global.HttpClient.SendAsync(request))
                        .GetAwaiter().GetResult();
                }
                catch (Exception)
                {
                    return null;
                }

                if (!response.IsSuccessStatusCode) return null;

                if (response.StatusCode == System.Net.HttpStatusCode.NoContent) return string.Empty;

                return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }
        }

        private static string BuildQuery(params (string key, string value)[] pairs)
        {
            var parts = new List<string>();
            foreach (var (k, v) in pairs)
                if (!string.IsNullOrEmpty(v))
                    parts.Add($"{HttpUtility.UrlEncode(k)}={HttpUtility.UrlEncode(v)}");
            return parts.Count > 0 ? "?" + string.Join("&", parts) : string.Empty;
        }
    }
}
