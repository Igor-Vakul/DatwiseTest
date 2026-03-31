using System.ComponentModel.DataAnnotations;

namespace SafetyPortal.Api.Dtos.Users;

public record UpdateUserDto(
    [Required, StringLength(100)] string FullName,
    [Range(1, int.MaxValue)] int    RoleId,
    [StringLength(100, MinimumLength = 6)] string? Password  // null = keep existing password
);
