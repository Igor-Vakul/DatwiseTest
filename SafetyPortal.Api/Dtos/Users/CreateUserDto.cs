namespace SafetyPortal.Api.Dtos.Users;

public record CreateUserDto(
    string FullName,
    string Email,
    string Password,
    int RoleId
);
