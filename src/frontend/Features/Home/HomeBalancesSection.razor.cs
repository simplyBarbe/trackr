using frontend.Infrastructure;
using Microsoft.AspNetCore.Components;
using Trackr.Api;
using Trackr.Api.Models;

namespace frontend.Features.Home;

public partial class HomeBalancesSection : ComponentBase
{
    private const int AccountsLookupPageSize = 200;

    [Inject]
    private TrackrApiClient TrackrApi { get; set; } = null!;

    private IReadOnlyList<AccountResponse> _accounts = [];
    private bool _loading = true;
    private string? _error;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var response = await TrackrApi.Accounts.GetAsync(configuration =>
            {
                configuration.QueryParameters.Page = 1;
                configuration.QueryParameters.PageSize = AccountsLookupPageSize;
            });

            _accounts = response?.Items ?? [];
        }
        catch (Exception ex)
        {
            _error = ApiErrors.GetMessage(ex);
        }
        finally
        {
            _loading = false;
        }
    }
}
