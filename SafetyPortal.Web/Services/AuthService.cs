using System;
using Newtonsoft.Json;
using SafetyPortal.Shared.Models;

namespace SafetyPortal.Web.Services
{
    public class AuthService : ApiBase
    {
        public AuthService(string jwtToken = null) : base(jwtToken) { }

        public LoginResponse Login(string email, string password)
        {
            var body = JsonConvert.SerializeObject(new { Email = email, Password = password });
            var resp = SendRaw(System.Net.Http.HttpMethod.Post, "/api/auth/login", body, withAuth: false);
            return resp == null ? null : JsonConvert.DeserializeObject<LoginResponse>(resp, DeserializeSettings);
        }
    }
}
