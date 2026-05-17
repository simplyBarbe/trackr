using backend.Data.Entities;
using backend.Data.Entities.Enums;

namespace backend.Features.Categories;

public sealed record CategoryResponse(
    Guid Id,
    string Name,
    CategoryKind Kind,
    Guid? ParentId,
    int SortOrder,
    bool IsArchived);

public sealed record ListCategoriesResponse(IReadOnlyList<CategoryResponse> Items);

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
