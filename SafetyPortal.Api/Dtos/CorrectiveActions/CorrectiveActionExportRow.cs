using System.ComponentModel;

namespace SafetyPortal.Api.Dtos.CorrectiveActions;

public class CorrectiveActionExportRow
{
    [DisplayName("Report #")]
    public string ReportNumber { get; set; } = string.Empty;

    [DisplayName("Action Title")]
    public string ActionTitle { get; set; } = string.Empty;

    [DisplayName("Assigned To")]
    public string AssignedTo { get; set; } = string.Empty;

    [DisplayName("Due Date")]
    public string DueDate { get; set; } = string.Empty;

    [DisplayName("Priority")]
    public string Priority { get; set; } = string.Empty;

    [DisplayName("Status")]
    public string Status { get; set; } = string.Empty;
}
