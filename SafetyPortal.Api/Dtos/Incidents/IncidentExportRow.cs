using System.ComponentModel;

namespace SafetyPortal.Api.Dtos.Incidents;

public class IncidentExportRow
{
    [DisplayName("Report #")]
    public string ReportNumber { get; set; } = string.Empty;

    [DisplayName("Title")]
    public string Title { get; set; } = string.Empty;

    [DisplayName("Category")]
    public string CategoryName { get; set; } = string.Empty;

    [DisplayName("Department")]
    public string DepartmentName { get; set; } = string.Empty;

    [DisplayName("Reported By")]
    public string ReportedBy { get; set; } = string.Empty;

    [DisplayName("Assigned To")]
    public string AssignedTo { get; set; } = string.Empty;

    [DisplayName("Incident Date")]
    public DateTime IncidentDate { get; set; }

    [DisplayName("Severity")]
    public string SeverityLevel { get; set; } = string.Empty;

    [DisplayName("Status")]
    public string Status { get; set; } = string.Empty;

    [DisplayName("Location")]
    public string Location { get; set; } = string.Empty;

    [DisplayName("Corrective Actions")]
    public int CorrectiveActionsCount { get; set; }
}
