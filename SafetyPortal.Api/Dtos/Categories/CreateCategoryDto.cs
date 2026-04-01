using System.ComponentModel.DataAnnotations;

namespace SafetyPortal.Api.Dtos.Categories;

public record CreateCategoryDto(
    [Required, StringLength(100)] string Name,
    [StringLength(255)]           string? Description
);
