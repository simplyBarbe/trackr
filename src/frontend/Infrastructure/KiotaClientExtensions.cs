using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Trackr.Api;

namespace frontend.Infrastructure;

public static class KiotaClientExtensions
{
    public static IServiceCollection AddKiotaTrackrApiClient(this IServiceCollection services)
    {
        services.AddScoped<IHealthProbe, KiotaHealthProbe>();
        services.AddScoped(sp =>
        {
            var httpClient = sp.GetRequiredService<IHttpClientFactory>()
                .CreateClient(ApiClientExtensions.HttpClientName);
            var authProvider = new AnonymousAuthenticationProvider();
            var adapter = new HttpClientRequestAdapter(authProvider, httpClient: httpClient);
            var options = sp.GetRequiredService<ApiOptions>();
            adapter.BaseUrl = options.BaseUrl;
            return new TrackrApiClient(adapter);
        });

        return services;
    }
}
