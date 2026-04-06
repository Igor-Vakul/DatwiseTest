using System.Collections.Generic;
using SafetyPortal.Shared.Models;

namespace SafetyPortal.Web.Services
{
    public class AdminService : ApiBase
    {
        public AdminService(string jwtToken = null) : base(jwtToken) { }

        // ── Departments ───────────────────────────────────────────────────────

        public List<DepartmentAdminItem> GetAllDepartments()
            => Get<List<DepartmentAdminItem>>("/api/admin/departments");

        public bool CreateDepartment(string name, string locationName, string color)
            => Post("/api/admin/departments", new { Name = name, LocationName = locationName, Color = color });

        public bool UpdateDepartment(int id, string name, string locationName, string color, bool isActive)
            => Put($"/api/admin/departments/{id}", new { Name = name, LocationName = locationName, Color = color, IsActive = isActive });

        public bool ToggleDepartmentActive(int id)
            => Put($"/api/admin/departments/{id}/toggle-active", (object)null);

        public bool DeleteDepartment(int id)
            => Delete($"/api/admin/departments/{id}");

        // ── Categories ────────────────────────────────────────────────────────

        public List<CategoryAdminItem> GetAllCategories()
            => Get<List<CategoryAdminItem>>("/api/admin/categories");

        public bool CreateCategory(string name, string description)
            => Post("/api/admin/categories", new { Name = name, Description = description });

        public bool UpdateCategory(int id, string name, string description, bool isActive)
            => Put($"/api/admin/categories/{id}", new { Name = name, Description = description, IsActive = isActive });

        public bool ToggleCategoryActive(int id)
            => Put($"/api/admin/categories/{id}/toggle-active", (object)null);

        public bool DeleteCategory(int id)
            => Delete($"/api/admin/categories/{id}");

        // ── Incident Statuses ─────────────────────────────────────────────────

        public List<IncidentStatusItem> GetAllIncidentStatuses()
            => Get<List<IncidentStatusItem>>("/api/admin/incident-statuses");

        public bool CreateIncidentStatus(string name, bool isClosing, string color)
            => Post("/api/admin/incident-statuses", new { Name = name, IsClosing = isClosing, Color = color, IsActive = true });

        public bool UpdateIncidentStatus(int id, string name, bool isClosing, string color, bool isActive)
            => Put($"/api/admin/incident-statuses/{id}", new { Name = name, IsClosing = isClosing, Color = color, IsActive = isActive });

        public bool DeleteIncidentStatus(int id)
            => Delete($"/api/admin/incident-statuses/{id}");

        // ── Severity Levels ───────────────────────────────────────────────────

        public List<SeverityLevelItem> GetAllSeverityLevels()
            => Get<List<SeverityLevelItem>>("/api/admin/severity-levels");

        public bool CreateSeverityLevel(string name, string color)
            => Post("/api/admin/severity-levels", new { Name = name, Color = color, IsActive = true });

        public bool UpdateSeverityLevel(int id, string name, string color, bool isActive)
            => Put($"/api/admin/severity-levels/{id}", new { Name = name, Color = color, IsActive = isActive });

        public bool DeleteSeverityLevel(int id)
            => Delete($"/api/admin/severity-levels/{id}");

        // ── Action Statuses ───────────────────────────────────────────────────

        public List<ActionStatusItem> GetAllActionStatuses()
            => Get<List<ActionStatusItem>>("/api/admin/action-statuses");

        public bool CreateActionStatus(string name, bool isCompleted, string color)
            => Post("/api/admin/action-statuses", new { Name = name, IsCompleted = isCompleted, Color = color, IsActive = true });

        public bool UpdateActionStatus(int id, string name, bool isCompleted, string color, bool isActive)
            => Put($"/api/admin/action-statuses/{id}", new { Name = name, IsCompleted = isCompleted, Color = color, IsActive = isActive });

        public bool DeleteActionStatus(int id)
            => Delete($"/api/admin/action-statuses/{id}");
    }
}
