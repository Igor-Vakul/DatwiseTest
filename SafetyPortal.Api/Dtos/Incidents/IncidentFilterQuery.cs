namespace SafetyPortal.Api.Dtos.Incidents;

/// <summary>Query-string parameters for incident list and export endpoints.</summary>
public record IncidentFilterQuery(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    string? Status = null,
    string? SeverityLevel = null,
    int? DepartmentId = null,
    int? CategoryId = null,
    bool Archived = false
);
