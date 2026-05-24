using backend.Application.Services;
using backend.Data;
using backend.Features.Accounts.Archive;
using backend.Features.Accounts.Create;
using backend.Features.Accounts.Get;
using backend.Features.Accounts.List;
using backend.Features.Accounts.Update;
using backend.Features.Categories.Archive;
using backend.Features.Categories.Create;
using backend.Features.Categories.Get;
using backend.Features.Categories.List;
using backend.Features.Categories.Update;
using backend.Features.Health.Get;
using backend.Features.Transactions.Create;
using backend.Features.Transactions.Delete;
using backend.Features.Transactions.Get;
using backend.Features.Transactions.List;
using backend.Features.Transactions.Summary;
using backend.Features.Transactions.Update;

namespace backend;

public static class DependencyInjection
{
    public static IServiceCollection AddTrackrApplication(this IServiceCollection services)
    {
        services.AddScoped<IAccountBalanceService, AccountBalanceService>();
        services.AddScoped<DataSeeder>();

        services.AddScoped<GetHealthHandler>();

        services.AddScoped<ListAccountsHandler>();
        services.AddScoped<GetAccountHandler>();
        services.AddScoped<CreateAccountHandler>();
        services.AddScoped<UpdateAccountHandler>();
        services.AddScoped<ArchiveAccountHandler>();

        services.AddScoped<ListCategoriesHandler>();
        services.AddScoped<GetCategoryHandler>();
        services.AddScoped<CreateCategoryHandler>();
        services.AddScoped<UpdateCategoryHandler>();
        services.AddScoped<ArchiveCategoryHandler>();

        services.AddScoped<ListTransactionsHandler>();
        services.AddScoped<GetTransactionSummaryHandler>();
        services.AddScoped<GetExpensesByCategoryHandler>();
        services.AddScoped<GetTransactionHandler>();
        services.AddScoped<CreateTransactionHandler>();
        services.AddScoped<UpdateTransactionHandler>();
        services.AddScoped<DeleteTransactionHandler>();

        return services;
    }
}
