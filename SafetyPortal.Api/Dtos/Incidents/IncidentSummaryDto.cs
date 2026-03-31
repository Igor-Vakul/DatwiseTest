namespace SafetyPortal.Api.Dtos.Incidents;

public record IncidentSummaryDto(
    int Id,
    string ReportNumber,
    string Title,
    string CategoryName,
    string DepartmentName,
    string ReportedByFullName,
    DateTime IncidentDate,
    string SeverityLevel,
    string Status,
    int CorrectiveActionsCount,
    int AttachmentsCount,
    bool IsArchived
);
