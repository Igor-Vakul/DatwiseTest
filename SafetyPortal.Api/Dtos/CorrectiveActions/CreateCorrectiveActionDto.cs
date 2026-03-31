using System.ComponentModel.DataAnnotations;

namespace SafetyPortal.Api.Dtos.CorrectiveActions;

public record CreateCorrectiveActionDto(
    [Range(1, int.MaxValue)] int ReportId,
    [Required, StringLength(200)] string ActionTitle,
    [StringLength(500)] string? ActionDescription,
    [Range(1, int.MaxValue)] int AssignedToUserId,
    DateOnly DueDate,
    [Required, StringLength(20)] string PriorityLevel
);
