using System.Collections.Generic;
using SafetyPortal.Shared.Models;

namespace SafetyPortal.Web.Services
{
    public class CorrectiveActionService : ApiBase
    {
        public CorrectiveActionService(string jwtToken = null) : base(jwtToken) { }

        public List<CorrectiveActionSummary> GetCorrectiveActions(int? reportId = null, string status = null)
        {
            var qs = BuildQuery(
                Param("reportId", reportId?.ToString()),
                Param("status", status)
            );
            return Get<List<CorrectiveActionSummary>>($"/api/corrective-actions{qs}");
        }

        public bool CreateCorrectiveAction(CreateCorrectiveActionRequest req)
            => Post("/api/corrective-actions", req);

        public bool UpdateActionStatus(int id, string status)
            => Put($"/api/corrective-actions/{id}/status", new { Status = status });

        public bool DeleteCorrectiveAction(int id)
            => Delete($"/api/corrective-actions/{id}");
    }
}
