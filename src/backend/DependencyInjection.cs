using backend.Application.Services;
using backend.Common;
using backend.Data;
using backend.Infrastructure;
using System.Reflection;

namespace backend;

public static class DependencyInjection
{
    public static IServiceCollection AddTrackrApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RecurringTransactionOptions>(
            configuration.GetSection(RecurringTransactionOptions.SectionName));

        services.AddScoped<IAccountBalanceService, AccountBalanceService>();
        services.AddScoped<RecurringTransactionGenerationService>();
        services.AddScoped<DataSeeder>();

        services.AddFeatureHandlers(typeof(AssemblyMarker).Assembly);

        services.AddHostedService<RecurringTransactionBackgroundService>();

        return services;
    }

    private static IServiceCollection AddFeatureHandlers(this IServiceCollection services, Assembly assembly)
    {
        var handlerTypes = assembly.GetTypes()
            .Where(type =>
                type is { IsClass: true, IsAbstract: false }
                && type.Name.EndsWith("Handler", StringComparison.Ordinal)
                && type.Namespace?.StartsWith("backend.Features.", StringComparison.Ordinal) == true);

        foreach (var handlerType in handlerTypes)
        {
            services.AddScoped(handlerType);
        }

        return services;
    }
}
