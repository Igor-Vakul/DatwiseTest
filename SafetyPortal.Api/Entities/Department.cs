namespace SafetyPortal.Api.Entities;

public class Department
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LocationName { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<IncidentReport> IncidentReports { get; set; } = new List<IncidentReport>();
}