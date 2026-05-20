using backend.Data.Entities.Enums;
using backend.Common.Pagination;

namespace backend.Features.Categories.List;

public sealed class ListCategoriesRequest : PagedRequest
{
    public CategoryKind? Kind { get; set; }

    public bool? IncludeArchived { get; set; }

    public string? Name { get; set; }
}
