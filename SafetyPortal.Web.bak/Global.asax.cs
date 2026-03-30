using System;
using System.Net.Http;
using System.Web;

namespace SafetyPortal.Web
{
    public class Global : HttpApplication
    {
        // Shared HttpClient — re-used across requests (thread-safe)
        public static readonly HttpClient HttpClient = new HttpClient(new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true  // dev only
        });

        protected void Application_Start(object sender, EventArgs e)
        {
        }

        protected void Application_End(object sender, EventArgs e)
        {
            HttpClient.Dispose();
        }
    }
}
