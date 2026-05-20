using Trackr.Api;

namespace frontend.Infrastructure;

public sealed class KiotaHealthProbe(TrackrApiClient apiClient) : IHealthProbe
{
    public async Task<HealthProbeResult?> GetAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await apiClient.Health.GetAsync(cancellationToken: cancellationToken);
            return response is null
                ? null
                : new HealthProbeResult(response.Status ?? "unknown", response.Database ?? "unknown");
        }
        catch
        {
            return null;
        }
    }
}
