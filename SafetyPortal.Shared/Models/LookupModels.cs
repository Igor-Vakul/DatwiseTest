namespace SafetyPortal.Shared.Models
{
    public class DepartmentItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string LocationName { get; set; }
        public string Color { get; set; } = "#6c757d";
    }

    public class DepartmentAdminItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string LocationName { get; set; }
        public string Color { get; set; }
        public bool IsActive { get; set; }
    }

    public class CategoryAdminItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class CategoryItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class UserLookupItem
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string RoleName { get; set; }
    }

    public class RoleItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
