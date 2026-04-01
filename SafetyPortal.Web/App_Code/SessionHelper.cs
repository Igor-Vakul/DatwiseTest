using System.Web;
using System.Web.SessionState;
using SafetyPortal.Web.Models;

namespace SafetyPortal.Web
{
    public static class SessionHelper
    {
        private const string KeyToken = "SP_Token";
        private const string KeyUserId = "SP_UserId";
        private const string KeyFullName = "SP_FullName";
        private const string KeyEmail = "SP_Email";
        private const string KeyRole = "SP_Role";

        public static void SetUser(HttpSessionState session, LoginResponse resp)
        {
            session[KeyToken] = resp.AccessToken;
            session[KeyUserId] = resp.UserId;
            session[KeyFullName] = resp.FullName;
            session[KeyEmail] = resp.Email;
            session[KeyRole] = resp.Role;
        }

        public static void Clear(HttpSessionState session)
        {
            session.Remove(KeyToken);
            session.Remove(KeyUserId);
            session.Remove(KeyFullName);
            session.Remove(KeyEmail);
            session.Remove(KeyRole);
        }

        public static bool IsLoggedIn(HttpSessionState session)
            => session[KeyToken] != null;

        public static string GetToken(HttpSessionState session)
            => session[KeyToken] as string;

        public static int GetUserId(HttpSessionState session)
            => session[KeyUserId] is int id ? id : 0;

        public static string GetFullName(HttpSessionState session)
            => session[KeyFullName] as string ?? string.Empty;

        public static string GetEmail(HttpSessionState session)
            => session[KeyEmail] as string ?? string.Empty;

        public static string GetRole(HttpSessionState session)
            => session[KeyRole] as string ?? string.Empty;

        public static bool IsAdmin(HttpSessionState session)
            => GetRole(session) == RoleName.Admin.ToString();

        public static bool IsManagerOrAdmin(HttpSessionState session)
        {
            var role = GetRole(session);
            return role == RoleName.Admin.ToString() || role == RoleName.SafetyManager.ToString();
        }

        public static bool IsSupervisorOrAbove(HttpSessionState session)
        {
            var role = GetRole(session);
            return role == RoleName.Admin.ToString() || role == RoleName.SafetyManager.ToString() || role == RoleName.Supervisor.ToString();
        }
    }
}
