namespace SafetyPortal.Api.Entities;

public class ActionStatusOption
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    /// <summary>Completed statuses are used for overdue calculations and auto-complete logic.</summary>
    public bool IsCompleted { get; set; }

    public string Color { get; set; } = "#6c757d";
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsSystem { get; set; }
}
