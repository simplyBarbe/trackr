using backend.Common.Pagination;
using backend.Data.Entities;
using backend.Data.Entities.Enums;

namespace backend.Features.Categories;

public sealed record CategorySummary(Guid Id, string Name);

public sealed record CategoryResponse(
    Guid Id,
    string Name,
    CategoryKind Kind,
    ExpensePriority Priority,
    CategorySummary? Parent,
    int SortOrder,
    bool IsArchived);

public sealed record ListCategoriesResponse(
    IReadOnlyList<CategoryResponse> Items,
    int Page,
    int PageSize,
    int TotalCount)
    : PagedResponse<CategoryResponse>(Items, Page, PageSize, TotalCount);

public static class CategoryMapping
{
    public static CategoryResponse ToResponse(Category category) =>
        new(
            category.Id,
            category.Name,
            category.Kind,
            category.Priority,
            category.ParentId is null
                ? null
                : new CategorySummary(category.ParentId.Value, category.Parent!.Name),
            category.SortOrder,
            category.IsArchived);
}
