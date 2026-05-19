using Microsoft.Extensions.DependencyInjection;

namespace frontend.Infrastructure;

public static class ApiClientExtensions
{
    public const string HttpClientName = "trackr-api";

    public static IServiceCollection AddTrackrApiClient(
        this IServiceCollection services,
        string apiBaseUrl)
    {
        services.AddSingleton(new ApiOptions { BaseUrl = apiBaseUrl.TrimEnd('/') });

        services.AddHttpClient(HttpClientName, client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl.TrimEnd('/') + "/");
        });

        services.AddKiotaTrackrApiClient();

        return services;
    }
}
