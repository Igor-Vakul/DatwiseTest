namespace SafetyPortal.Api.Dtos.CorrectiveActions;

public record CorrectiveActionSummaryDto(
    int Id,
    int ReportId,
    string ReportNumber,
    string ActionTitle,
    string AssignedToFullName,
    DateOnly DueDate,
    string Status,
    string PriorityLevel
);
