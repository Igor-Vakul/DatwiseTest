using SafetyPortal.Shared.Models;

namespace SafetyPortal.Web.Services
{
    public class DashboardService : ApiBase
    {
        public DashboardService(string jwtToken = null) : base(jwtToken) { }

        public DashboardStats GetDashboardStats()
            => Get<DashboardStats>("/api/dashboard/stats");
    }
}
