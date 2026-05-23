using frontend.Features.Shared;
using frontend.Infrastructure;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Trackr.Api;
using Trackr.Api.Models;

namespace frontend.Features.Home;

public partial class Home : ComponentBase
{
    private const int AccountsLookupPageSize = 200;

    [Inject]
    private IHealthProbe HealthProbe { get; set; } = null!;

    [Inject]
    private IConfiguration Configuration { get; set; } = null!;

    [Inject]
    private TrackrApiClient TrackrApi { get; set; } = null!;

    private QueryState<HealthProbeResult> _healthQuery = QueryState<HealthProbeResult>.Loading();

    private QueryState<IReadOnlyList<AccountResponse>> _accountsQuery =
        QueryState<IReadOnlyList<AccountResponse>>.Loading();

    private string _apiBaseUrl = string.Empty;
    private string _clientMode = "Kiota";

    protected override async Task OnInitializedAsync()
    {
        _apiBaseUrl = Configuration["ApiBaseUrl"] ?? string.Empty;
        await Task.WhenAll(LoadHealthAsync(), LoadAccountsAsync());
    }

    private async Task LoadHealthAsync()
    {
        _healthQuery = await QueryState<HealthProbeResult>.RunAsync(async () =>
        {
            var health = await HealthProbe.GetAsync();
            return health ?? throw new InvalidOperationException("API unreachable.");
        });
    }

    private async Task LoadAccountsAsync()
    {
        _accountsQuery = await QueryState<IReadOnlyList<AccountResponse>>.RunAsync(async () =>
        {
            var response = await TrackrApi.Accounts.GetAsync(configuration =>
            {
                configuration.QueryParameters.Page = 1;
                configuration.QueryParameters.PageSize = AccountsLookupPageSize;
            });

            return (IReadOnlyList<AccountResponse>)(response?.Items ?? []);
        });
    }

    private static string FormatBalance(AccountResponse account) =>
        MoneyFormat.Format(account.Balance);
}
