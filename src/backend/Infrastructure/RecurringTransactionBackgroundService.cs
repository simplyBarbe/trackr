using backend.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace backend.Infrastructure;

public sealed class RecurringTransactionBackgroundService(
    IServiceScopeFactory scopeFactory,
    IOptions<RecurringTransactionOptions> options,
    ILogger<RecurringTransactionBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromMinutes(Math.Max(1, options.Value.PollIntervalMinutes));
        using var timer = new PeriodicTimer(interval);

        do
        {
            try
            {
                await PollAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Recurring transaction poll failed");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task PollAsync(CancellationToken stoppingToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var generation = scope.ServiceProvider.GetRequiredService<RecurringTransactionGenerationService>();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var created = await generation.ProcessDueAsync(today, stoppingToken);

        if (created > 0)
        {
            logger.LogInformation(
                "Recurring transaction poll created {Count} transaction(s) for {Today}",
                created,
                today);
        }
    }
}

public sealed class RecurringTransactionOptions
{
    public const string SectionName = "RecurringTransactions";

    public int PollIntervalMinutes { get; set; } = 60;
}
