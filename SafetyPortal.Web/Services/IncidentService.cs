using System;
using Newtonsoft.Json;
using SafetyPortal.Shared;
using SafetyPortal.Shared.Models;

namespace SafetyPortal.Web.Services
{
    public class IncidentService : ApiBase
    {
        public IncidentService(string jwtToken = null) : base(jwtToken) { }

        public PagedResult<IncidentSummary> GetIncidents(
            int page = 1, int pageSize = AppConstants.Pagination.DefaultPageSize,
            string search = null, string status = null,
            string severityLevel = null, int? departmentId = null, int? categoryId = null,
            bool archived = false)
        {
            var qs = BuildQuery(
                Param("page", page.ToString()),
                Param("pageSize", pageSize.ToString()),
                Param("search", search),
                Param("status", status),
                Param("severityLevel", severityLevel),
                Param("departmentId", departmentId?.ToString()),
                Param("categoryId", categoryId?.ToString()),
                Param("archived", archived.ToString().ToLower())
            );
            return Get<PagedResult<IncidentSummary>>($"/api/incidents{qs}");
        }

        public IncidentDetail GetIncident(int id)
            => Get<IncidentDetail>($"/api/incidents/{id}");

        public bool CreateIncident(CreateIncidentRequest req)
            => Post("/api/incidents", req);

        // Returns the new incident ID, or null on failure
        public int? CreateIncidentForId(CreateIncidentRequest req)
        {
            var json = SendRaw(System.Net.Http.HttpMethod.Post, "/api/incidents", JsonConvert.SerializeObject(req, Settings));
            if (json == null) return null;
            var result = JsonConvert.DeserializeObject<CreateIncidentResult>(json, DeserializeSettings);
            return result?.Id;
        }

        public bool UpdateIncident(int id, UpdateIncidentRequest req)
            => Put($"/api/incidents/{id}", req);

        public bool DeleteIncident(int id)
            => Delete($"/api/incidents/{id}");

        public bool ToggleIncidentArchive(int id)
            => Put($"/api/incidents/{id}/archive", (object)null);
    }
}
