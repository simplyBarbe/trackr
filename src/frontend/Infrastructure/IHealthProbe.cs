namespace frontend.Infrastructure;

public sealed record HealthProbeResult(string Status, string Database);

public interface IHealthProbe
{
    Task<HealthProbeResult?> GetAsync(CancellationToken cancellationToken = default);
}
