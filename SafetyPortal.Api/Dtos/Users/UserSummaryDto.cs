namespace SafetyPortal.Api.Dtos.Users;

public record UserSummaryDto(
    int Id,
    string FullName,
    string Email,
    string RoleName,
    bool IsActive
);
