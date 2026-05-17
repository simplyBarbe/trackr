using backend.Data.Entities.Enums;

namespace backend.Features.Categories.List;

public sealed class ListCategoriesRequest
{
    public CategoryKind? Kind { get; set; }

    public bool IncludeArchived { get; set; }
}
