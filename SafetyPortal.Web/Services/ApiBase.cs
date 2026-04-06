using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Web.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SafetyPortal.Web.Services
{
    /// <summary>
    /// Shared HTTP infrastructure for all API services.
    /// Uses the shared Global.HttpClient but adds JWT per-request via HttpRequestMessage
    /// to avoid thread-safety issues with DefaultRequestHeaders.
    /// </summary>
    public abstract class ApiBase
    {
        protected static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            NullValueHandling = NullValueHandling.Ignore
        };

        protected static readonly JsonSerializerSettings DeserializeSettings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            },
            DateFormatHandling = DateFormatHandling.IsoDateFormat
        };

        protected readonly string _baseUrl;
        protected readonly string _token;

        protected ApiBase(string jwtToken = null)
        {
            _baseUrl = (WebConfigurationManager.AppSettings["ApiBaseUrl"] ?? "https://localhost:7182").TrimEnd('/');
            _token = jwtToken;
        }

        protected T Get<T>(string url)
        {
            var json = SendRaw(HttpMethod.Get, url);
            return json == null ? default(T) : JsonConvert.DeserializeObject<T>(json, DeserializeSettings);
        }

        protected bool Post(string url, object body)
            => SendRaw(HttpMethod.Post, url, JsonConvert.SerializeObject(body, Settings)) != null;

        protected bool Put(string url, object body)
            => SendRaw(HttpMethod.Put, url, body != null ? JsonConvert.SerializeObject(body, Settings) : "{}") != null;

        protected bool Delete(string url)
            => SendRaw(HttpMethod.Delete, url) != null;

        /// <summary>
        /// Sends a request using the shared HttpClient.
        /// Auth token is added per-request via HttpRequestMessage (thread-safe).
        /// Returns null on non-2xx response.
        /// </summary>
        protected string SendRaw(HttpMethod method, string path, string jsonBody = null, bool withAuth = true)
        {
            using (var request = new HttpRequestMessage(method, $"{_baseUrl}{path}"))
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

        protected static KeyValuePair<string, string> Param(string key, string value)
            => new KeyValuePair<string, string>(key, value);

        protected static string BuildQuery(params KeyValuePair<string, string>[] pairs)
        {
            var parts = new List<string>();
            foreach (var pair in pairs)
                if (!string.IsNullOrEmpty(pair.Value))
                    parts.Add($"{HttpUtility.UrlEncode(pair.Key)}={HttpUtility.UrlEncode(pair.Value)}");
            return parts.Count > 0 ? "?" + string.Join("&", parts) : string.Empty;
        }
    }
}
