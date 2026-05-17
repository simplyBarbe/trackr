using System.Net.Http.Json;

namespace frontend.Infrastructure;

/// <summary>HTTP health probe until Kiota client is generated (see scripts/generate-api.ps1).</summary>
public sealed class HttpHealthProbe(IHttpClientFactory httpClientFactory) : IHealthProbe
{
    public async Task<HealthProbeResult?> GetAsync(CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient(ApiClientExtensions.HttpClientName);
        try
        {
            var response = await client.GetFromJsonAsync<HealthApiResponse>(
                "api/health",
                cancellationToken);

            return response is null
                ? null
                : new HealthProbeResult(response.Status, response.Database);
        }
        catch
        {
            return null;
        }
    }

    private sealed record HealthApiResponse(string Status, string Database);
}
