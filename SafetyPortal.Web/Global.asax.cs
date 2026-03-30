using System;
using System.Net.Http;

namespace SafetyPortal.Web
{
    public class Global : System.Web.HttpApplication
    {
        public static readonly HttpClient HttpClient = new HttpClient(
            new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = delegate { return true; }
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