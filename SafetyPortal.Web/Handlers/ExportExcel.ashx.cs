using System.Web;
using System.Web.SessionState;

namespace SafetyPortal.Web.Handlers
{
    /// <summary>
    /// Proxies Excel export requests to the API, injecting JWT from the session.
    /// Usage: /Handlers/ExportExcel.ashx?type=incidents[&search=...&status=...&severity=...&dept=...&cat=...]
    ///        /Handlers/ExportExcel.ashx?type=actions[&status=...]
    /// </summary>
    public class ExportExcel : IHttpHandler, IRequiresSessionState
    {
        public bool IsReusable => false;

        public void ProcessRequest(HttpContext context)
        {
            if (!SessionHelper.IsLoggedIn(context.Session))
            {
                context.Response.StatusCode = 401;
                context.Response.End();
                return;
            }

            var qs = context.Request.QueryString;
            var type = qs["type"] ?? string.Empty;

            string apiPath;
            if (type == "incidents")
            {
                apiPath = BuildIncidentsExportPath(qs);
            }
            else if (type == "actions")
            {
                apiPath = BuildActionsExportPath(qs);
            }
            else
            {
                context.Response.StatusCode = 400;
                context.Response.End();
                return;
            }

            var token = SessionHelper.GetToken(context.Session);
            var client = new ApiClient(token);
            bool ok = client.ProxyExport(apiPath, context.Response);

            if (!ok)
                context.Response.StatusCode = 500;

            context.Response.End();
        }

        private static string BuildIncidentsExportPath(System.Collections.Specialized.NameValueCollection qs)
        {
            var parts = new System.Collections.Generic.List<string>();
            if (!string.IsNullOrEmpty(qs["search"])) parts.Add("search=" + HttpUtility.UrlEncode(qs["search"]));
            if (!string.IsNullOrEmpty(qs["status"])) parts.Add("status=" + HttpUtility.UrlEncode(qs["status"]));
            if (!string.IsNullOrEmpty(qs["severity"])) parts.Add("severityLevel=" + HttpUtility.UrlEncode(qs["severity"]));
            if (!string.IsNullOrEmpty(qs["dept"])) parts.Add("departmentId=" + HttpUtility.UrlEncode(qs["dept"]));
            if (!string.IsNullOrEmpty(qs["cat"])) parts.Add("categoryId=" + HttpUtility.UrlEncode(qs["cat"]));
            var query = parts.Count > 0 ? "?" + string.Join("&", parts) : string.Empty;
            return "/api/incidents/export" + query;
        }

        private static string BuildActionsExportPath(System.Collections.Specialized.NameValueCollection qs)
        {
            var parts = new System.Collections.Generic.List<string>();
            if (!string.IsNullOrEmpty(qs["status"])) parts.Add("status=" + HttpUtility.UrlEncode(qs["status"]));
            var query = parts.Count > 0 ? "?" + string.Join("&", parts) : string.Empty;
            return "/api/corrective-actions/export" + query;
        }
    }
}
