namespace SafetyPortal.Api.Entities;

public class IncidentStatusOption
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    /// <summary>Closing statuses allow archiving and auto-complete corrective actions.</summary>
    public bool IsClosing { get; set; }
    public string Color { get; set; } = "#6c757d";

    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    /// <summary>System statuses cannot be deleted.</summary>
    public bool IsSystem { get; set; }
}
