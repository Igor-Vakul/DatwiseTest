using System.Collections.Generic;
using SafetyPortal.Shared.Models;

namespace SafetyPortal.Web.Services
{
    public class LookupService : ApiBase
    {
        public LookupService(string jwtToken = null) : base(jwtToken) { }

        public List<DepartmentItem> GetDepartments()
            => Get<List<DepartmentItem>>("/api/lookup/departments");

        public List<CategoryItem> GetCategories()
            => Get<List<CategoryItem>>("/api/lookup/categories");

        public List<UserLookupItem> GetUsers()
            => Get<List<UserLookupItem>>("/api/lookup/users");

        public List<RoleItem> GetRoles()
            => Get<List<RoleItem>>("/api/lookup/roles");
    }
}
