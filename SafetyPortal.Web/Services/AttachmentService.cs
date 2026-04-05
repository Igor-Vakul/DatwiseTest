using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using SafetyPortal.Shared.Models;

namespace SafetyPortal.Web.Services
{
    public class AttachmentService : ApiBase
    {
        public AttachmentService(string jwtToken = null) : base(jwtToken) { }

        // Upload a single file attachment; returns error message or null on success
        public string UploadAttachment(int incidentId, byte[] content, string fileName, string contentType)
        {
            try
            {
                var form = new MultipartFormDataContent();
                var fileContent = new ByteArrayContent(content);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
                form.Add(fileContent, "file", fileName);

                var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/api/incidents/{incidentId}/attachments");

                if (!string.IsNullOrEmpty(_token))
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);

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

        public bool DeleteAttachment(int incidentId, int attachmentId)
            => Delete($"/api/incidents/{incidentId}/attachments/{attachmentId}");

        // Downloads a file and writes it directly to the given HTTP response.
        // Returns false if the API returned a non-2xx response.
        public bool ProxyDownload(int incidentId, int attachmentId, System.Web.HttpResponse response)
        {
            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/api/incidents/{incidentId}/attachments/{attachmentId}/download"))
                {
                    if (!string.IsNullOrEmpty(_token))
                        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);

                    var apiResponse = System.Threading.Tasks.Task
                        .Run(() => Global.HttpClient.SendAsync(request, System.Net.Http.HttpCompletionOption.ResponseHeadersRead))
                        .GetAwaiter().GetResult();

                    if (!apiResponse.IsSuccessStatusCode) return false;

                    var contentType = apiResponse.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
                    var disposition = apiResponse.Content.Headers.ContentDisposition;
                    var fileName = disposition?.FileNameStar ?? disposition?.FileName ?? "download";

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

        // Downloads an Excel export and writes it directly to the HTTP response.
        // Returns false if the API returned a non-2xx response.
        public bool ProxyExport(string apiPath, System.Web.HttpResponse response)
        {
            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}{apiPath}"))
                {
                    if (!string.IsNullOrEmpty(_token))
                        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);

                    var apiResponse = System.Threading.Tasks.Task
                        .Run(() => Global.HttpClient.SendAsync(request, System.Net.Http.HttpCompletionOption.ResponseHeadersRead))
                        .GetAwaiter().GetResult();

                    if (!apiResponse.IsSuccessStatusCode) return false;

                    var disposition = apiResponse.Content.Headers.ContentDisposition;
                    var fileName = disposition?.FileNameStar ?? disposition?.FileName ?? "export.xlsx";

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
    }
}
