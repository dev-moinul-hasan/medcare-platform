using System.ComponentModel.DataAnnotations;

namespace MedCare.Application.DTOs.Common;

public record PageQuery(
    string? Search = null,
    string? SortBy = "id",
    string? SortDirection = "asc",
    [Range (1, int.MaxValue)]
    int Page = 1,
    [Range (1, 100)]
    int PageSize = 20
);
