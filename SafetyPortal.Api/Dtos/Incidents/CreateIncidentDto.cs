namespace SafetyPortal.Api.Dtos.Incidents;

public record CreateIncidentDto(
    string  Title,
    string  Description,
    int     CategoryId,
    int     DepartmentId,
    string  IncidentDate,     // ISO 8601: yyyy-MM-dd
    string? LocationDetails,
    string  SeverityLevel,
    int?    AssignedToUserId
);
