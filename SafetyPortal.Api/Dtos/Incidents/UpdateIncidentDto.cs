namespace SafetyPortal.Api.Dtos.Incidents;

public record UpdateIncidentDto(
    string  Title,
    string  Description,
    int     CategoryId,
    int     DepartmentId,
    string  IncidentDate,     // ISO 8601: yyyy-MM-dd
    string? LocationDetails,
    string  SeverityLevel,
    string  Status,
    int?    AssignedToUserId
);
