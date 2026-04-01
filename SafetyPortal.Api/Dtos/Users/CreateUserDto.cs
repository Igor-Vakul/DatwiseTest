using System.ComponentModel.DataAnnotations;

namespace SafetyPortal.Api.Dtos.Users;

public record CreateUserDto(
    [Required, StringLength(100)] string FullName,
    [Required, EmailAddress, StringLength(150)] string Email,
    [Required, StringLength(100, MinimumLength = 8),
    RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+\-=\[\]{}|;':"",./<>?]).{8,}$", ErrorMessage = "Password must contain uppercase, lowercase, digit and special character.")]
    string Password,
    [Range(1, int.MaxValue)] int RoleId
);
