namespace SafetyPortal.Api.Entities;

public class IncidentReport
{
    public int Id { get; set; }
    public string ReportNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public int CategoryId { get; set; }
    public IncidentCategory Category { get; set; } = null!;

    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;

    public int ReportedByUserId { get; set; }
    public User ReportedByUser { get; set; } = null!;

    public int? AssignedToUserId { get; set; }
    public User? AssignedToUser { get; set; }

    public DateTime IncidentDate { get; set; }
    public DateTime ReportedAt { get; set; } = DateTime.UtcNow;

    public string? LocationDetails { get; set; }
    public string SeverityLevel { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsArchived { get; set; } = false;

    public ICollection<CorrectiveAction>    CorrectiveActions { get; set; } = new List<CorrectiveAction>();
    public ICollection<IncidentAttachment> Attachments       { get; set; } = new List<IncidentAttachment>();
}