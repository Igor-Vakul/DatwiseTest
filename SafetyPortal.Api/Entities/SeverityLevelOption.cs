namespace SafetyPortal.Api.Entities;

public class SeverityLevelOption
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#6c757d";
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsSystem { get; set; }
}
