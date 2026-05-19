namespace frontend.Infrastructure;

public sealed class MutationState
{
    public bool IsPending { get; init; }
    public string? Error { get; init; }

    public static MutationState Idle { get; } = new();

    public static MutationState Pending() => new() { IsPending = true };

    public static MutationState Failed(string message) => new() { Error = message };

    public static async Task<MutationState> RunAsync(Func<Task> action)
    {
        try
        {
            await action();
            return Idle;
        }
        catch (Exception ex)
        {
            return Failed(ApiErrors.GetMessage(ex));
        }
    }
}
