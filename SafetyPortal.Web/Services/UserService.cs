using System.Collections.Generic;
using SafetyPortal.Shared.Models;

namespace SafetyPortal.Web.Services
{
    public class UserService : ApiBase
    {
        public UserService(string jwtToken = null) : base(jwtToken) { }

        public List<UserSummary> GetAllUsers()
            => Get<List<UserSummary>>("/api/users");

        public bool CreateUser(CreateUserRequest req)
            => Post("/api/users", req);

        public bool UpdateUser(int id, UpdateUserRequest req)
            => Put($"/api/users/{id}", req);

        public bool ToggleUserActive(int id)
            => Put($"/api/users/{id}/toggle-active", new { });

        public bool SendEmailToUser(int id, string subject, string body)
            => Post($"/api/users/{id}/send-email", new { Subject = subject, Body = body });
    }
}
