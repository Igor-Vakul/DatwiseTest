using SafetyPortal.Api.Dtos.CorrectiveActions;

namespace SafetyPortal.Api.Dtos.Incidents;

public record IncidentDetailDto(
    int Id,
    string ReportNumber,
    string Title,
    string Description,
    int CategoryId,
    string CategoryName,
    int DepartmentId,
    string DepartmentName,
    int ReportedByUserId,
    string ReportedByFullName,
    int? AssignedToUserId,
    string? AssignedToFullName,
    DateTime IncidentDate,
    DateTime ReportedAt,
    string? LocationDetails,
    string SeverityLevel,
    string Status,
    bool IsArchived,
    List<CorrectiveActionSummaryDto> CorrectiveActions
);
