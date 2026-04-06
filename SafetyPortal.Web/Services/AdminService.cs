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
    }
}
