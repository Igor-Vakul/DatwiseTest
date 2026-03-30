namespace SafetyPortal.Api.Entities;

public class CorrectiveAction
{
    public int Id { get; set; }

    public int ReportId { get; set; }
    public IncidentReport Report { get; set; } = null!;

    public string ActionTitle { get; set; } = string.Empty;
    public string? ActionDescription { get; set; }

    public int AssignedToUserId { get; set; }
    public User AssignedToUser { get; set; } = null!;

    public DateOnly DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }

    public string Status { get; set; } = string.Empty;
    public string PriorityLevel { get; set; } = string.Empty;
}