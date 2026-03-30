namespace SafetyPortal.Api.Dtos.CorrectiveActions;

public record CreateCorrectiveActionDto(
    int ReportId,
    string ActionTitle,
    string? ActionDescription,
    int AssignedToUserId,
    DateOnly DueDate,
    string PriorityLevel
);
