namespace frontend.Infrastructure;

public sealed class DebouncedAsync(int delayMs = 300) : IDisposable
{
    private CancellationTokenSource? _cts;

    public async Task InvokeAsync(Func<Task> action)
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        try
        {
            await Task.Delay(delayMs, token);
            await action();
        }
        catch (OperationCanceledException)
        {
        }
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }
}
