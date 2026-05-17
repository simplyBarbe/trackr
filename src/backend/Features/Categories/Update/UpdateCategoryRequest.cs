using backend.Data.Entities.Enums;

namespace backend.Features.Categories.Update;

public sealed class UpdateCategoryRequest
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public CategoryKind Kind { get; set; }

    public Guid? ParentId { get; set; }

    public int SortOrder { get; set; }
}
