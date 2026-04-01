using System.Web;
using System.Web.SessionState;

namespace SafetyPortal.Web.Handlers
{
    /// <summary>
    /// Proxies file downloads from the API, injecting the JWT from the current session.
    /// Usage: /Handlers/DownloadAttachment.ashx?incidentId=X&attachmentId=Y
    /// </summary>
    public class DownloadAttachment : IHttpHandler, IRequiresSessionState
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

            if (!int.TryParse(context.Request.QueryString["incidentId"], out int incidentId) ||
                !int.TryParse(context.Request.QueryString["attachmentId"], out int attachmentId))
            {
                context.Response.StatusCode = 400;
                context.Response.End();
                return;
            }

            var token = SessionHelper.GetToken(context.Session);
            var client = new ApiClient(token);

            bool ok = client.ProxyDownload(incidentId, attachmentId, context.Response);
            if (!ok)
            {
                context.Response.StatusCode = 404;
            }

            context.Response.End();
        }
    }
}
