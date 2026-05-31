namespace frontend.Infrastructure;

public sealed record PagedList<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public static PagedList<T> Empty(int page = 1, int pageSize = PaginationDefaults.PageSize) =>
        new([], page, pageSize, 0);
}
