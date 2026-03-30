namespace SafetyPortal.Api.Entities;

public class IncidentCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<IncidentReport> IncidentReports { get; set; } = new List<IncidentReport>();
}