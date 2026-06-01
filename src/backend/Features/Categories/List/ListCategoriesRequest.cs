using backend.Common.Pagination;
using backend.Data.Entities.Enums;

namespace backend.Features.Categories.List;

public sealed class ListCategoriesRequest : PagedRequest
{
    public CategoryKind? Kind { get; set; }

    public bool? IncludeArchived { get; set; }

    public string? Name { get; set; }
}
