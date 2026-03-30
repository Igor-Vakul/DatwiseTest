namespace SafetyPortal.Api.Dtos.Users;

public record UpdateUserDto(
    string FullName,
    int    RoleId,
    string? Password  // null = keep existing password
);
