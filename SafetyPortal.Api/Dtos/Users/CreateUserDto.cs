using System.ComponentModel.DataAnnotations;

namespace SafetyPortal.Api.Dtos.Users;

public record CreateUserDto(
    [Required, StringLength(100)] string FullName,
    [Required, EmailAddress, StringLength(150)] string Email,
    [Required, StringLength(100, MinimumLength = 6)] string Password,
    [Range(1, int.MaxValue)] int RoleId
);
