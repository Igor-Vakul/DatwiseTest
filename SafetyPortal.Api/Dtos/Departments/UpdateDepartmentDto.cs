using System.ComponentModel.DataAnnotations;

namespace SafetyPortal.Api.Dtos.Departments;

public record UpdateDepartmentDto(
    [Required, StringLength(100)] string Name,
    [StringLength(100)] string? LocationName,
    [StringLength(7), RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color must be a hex value like #1a2b3c.")] string Color,
    bool IsActive
);
