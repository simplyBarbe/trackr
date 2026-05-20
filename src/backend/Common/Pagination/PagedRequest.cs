namespace backend.Common.Pagination;

public abstract class PagedRequest
{
    public int Page { get; set; } = PaginationDefaults.Page;

    public int PageSize { get; set; } = PaginationDefaults.PageSize;
}
