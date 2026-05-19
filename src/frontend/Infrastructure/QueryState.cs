namespace frontend.Infrastructure;

public enum QueryStatus
{
    Loading,
    Success,
    Error
}

public sealed class QueryState<T>
{
    public QueryStatus Status { get; init; }
    public T? Data { get; init; }
    public string? Error { get; init; }

    public bool IsLoading => Status == QueryStatus.Loading && Data is null;
    public bool IsFetching => Status == QueryStatus.Loading && Data is not null;
    public bool IsError => Status == QueryStatus.Error;
    public bool HasData => Status == QueryStatus.Success && Data is not null;

    public static QueryState<T> Loading() => new() { Status = QueryStatus.Loading };

    public static QueryState<T> Fetching(T previousData) => new()
    {
        Status = QueryStatus.Loading,
        Data = previousData
    };

    public static QueryState<T> Success(T data) => new()
    {
        Status = QueryStatus.Success,
        Data = data
    };

    public static QueryState<T> FromError(string message) => new()
    {
        Status = QueryStatus.Error,
        Error = message
    };

    public static async Task<QueryState<T>> RunAsync(Func<Task<T>> fetch)
    {
        try
        {
            var data = await fetch();
            return Success(data);
        }
        catch (Exception ex)
        {
            return FromError(ApiErrors.GetMessage(ex));
        }
    }
}
