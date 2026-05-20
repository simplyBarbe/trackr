using backend.Data.Entities;
using backend.Data.Entities.Enums;
using backend.Common.Pagination;

namespace backend.Features.Categories;

public sealed record CategoryResponse(
    Guid Id,
    string Name,
    CategoryKind Kind,
    Guid? ParentId,
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
            category.ParentId,
            category.SortOrder,
            category.IsArchived);
}
