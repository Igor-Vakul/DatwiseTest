using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Script.Serialization;
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
        private readonly string             _base;
        private readonly string             _token;
        private readonly JavaScriptSerializer _jss = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };

        public ApiClient(string jwtToken = null)
        {
            _base  = (WebConfigurationManager.AppSettings["ApiBaseUrl"] ?? "https://localhost:7182").TrimEnd('/');
            _token = jwtToken;
        }

        // ── Auth ──────────────────────────────────────────────────────────
        public LoginResponse Login(string email, string password)
        {
            var body = _jss.Serialize(new { Email = email, Password = password });
            var resp = SendRaw(HttpMethod.Post, "/api/auth/login", body, withAuth: false);
            return resp == null ? null : _jss.Deserialize<LoginResponse>(resp);
        }

        // ── Dashboard ─────────────────────────────────────────────────────
        public DashboardStats GetDashboardStats()
            => Get<DashboardStats>("/api/dashboard/stats");

        // ── Incidents ─────────────────────────────────────────────────────
        public PagedResult<IncidentSummary> GetIncidents(
            int page = 1, int pageSize = 20,
            string search = null, string status = null,
            string severityLevel = null, int? departmentId = null, int? categoryId = null)
        {
            var qs = BuildQuery(
                ("page",          page.ToString()),
                ("pageSize",      pageSize.ToString()),
                ("search",        search),
                ("status",        status),
                ("severityLevel", severityLevel),
                ("departmentId",  departmentId?.ToString()),
                ("categoryId",    categoryId?.ToString())
            );
            return Get<PagedResult<IncidentSummary>>($"/api/incidents{qs}");
        }

        public IncidentDetail GetIncident(int id)
            => Get<IncidentDetail>($"/api/incidents/{id}");

        public bool CreateIncident(CreateIncidentRequest req)
            => Post("/api/incidents", req);

        public bool UpdateIncident(int id, UpdateIncidentRequest req)
            => Put($"/api/incidents/{id}", req);

        public bool DeleteIncident(int id)
            => Delete($"/api/incidents/{id}");

        // ── Corrective Actions ────────────────────────────────────────────
        public List<CorrectiveActionSummary> GetCorrectiveActions(int? reportId = null, string status = null)
        {
            var qs = BuildQuery(
                ("reportId", reportId?.ToString()),
                ("status",   status)
            );
            return Get<List<CorrectiveActionSummary>>($"/api/corrective-actions{qs}");
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

        // ── User Management ───────────────────────────────────────────────
        public List<UserSummary> GetAllUsers()    => Get<List<UserSummary>>("/api/users");
        public bool CreateUser(CreateUserRequest req) => Post("/api/users", req);
        public bool ToggleUserActive(int id)          => Put($"/api/users/{id}/toggle-active", new { });

        // ── Helpers ───────────────────────────────────────────────────────
        private T Get<T>(string url)
        {
            var json = SendRaw(HttpMethod.Get, url);
            return json == null ? default : _jss.Deserialize<T>(json);
        }

        private bool Post(string url, object body)
        {
            return SendRaw(HttpMethod.Post, url, _jss.Serialize(body)) != null;
        }

        private bool Put(string url, object body)
        {
            return SendRaw(HttpMethod.Put, url, body != null ? _jss.Serialize(body) : "{}") != null;
        }

        private bool Delete(string url)
        {
            return SendRaw(HttpMethod.Delete, url) != null;
        }

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
                    response = Global.HttpClient.SendAsync(request).GetAwaiter().GetResult();
                }
                catch (Exception)
                {
                    return null;
                }

                if (!response.IsSuccessStatusCode) return null;

                // DELETE returns 204 No Content — treat as success
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
