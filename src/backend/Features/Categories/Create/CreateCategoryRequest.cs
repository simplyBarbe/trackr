using backend.Data.Entities.Enums;

namespace backend.Features.Categories.Create;

public sealed class CreateCategoryRequest
{
    public required string Name { get; set; }

    public CategoryKind Kind { get; set; }

    public Guid? ParentId { get; set; }

    public int SortOrder { get; set; }
}
