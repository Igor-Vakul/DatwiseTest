using System.ComponentModel.DataAnnotations;

namespace SafetyPortal.Api.Dtos.Incidents;

public record UpdateIncidentDto(
    [Required, StringLength(200)] string  Title,
    [Required, StringLength(5000)] string  Description,
    [Range(1, int.MaxValue)] int     CategoryId,
    [Range(1, int.MaxValue)] int     DepartmentId,
    [Required] string  IncidentDate,     // ISO 8601: yyyy-MM-dd
    [StringLength(200)] string? LocationDetails,
    [Required, StringLength(20)] string  SeverityLevel,
    [Required, StringLength(30)] string  Status,
    int?    AssignedToUserId
);
