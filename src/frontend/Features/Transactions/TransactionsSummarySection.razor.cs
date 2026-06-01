using frontend.Infrastructure;
using Microsoft.AspNetCore.Components;
using Trackr.Api;
using Trackr.Api.Models;

namespace frontend.Features.Transactions;

public partial class TransactionsSummarySection : ComponentBase
{
    [Inject]
    private TrackrApiClient TrackrApi { get; set; } = null!;

    [Parameter, EditorRequired]
    public TransactionListFilters Filters { get; set; } = null!;

    [Parameter]
    public int RefreshVersion { get; set; }

    private GetTransactionSummaryResponse _summary = new();
    private string? _error;
    private bool _initialLoading = true;
    private TransactionListFilters? _loadedForFilters;
    private int _loadedForVersion = -1;

    protected override async Task OnParametersSetAsync()
    {
        if (_loadedForFilters == Filters && _loadedForVersion == RefreshVersion)
            return;

        _loadedForFilters = Filters;
        _loadedForVersion = RefreshVersion;
        await LoadSummaryAsync();
    }

    private async Task LoadSummaryAsync()
    {
        try
        {
            var response = await TrackrApi.Transactions.Summary.GetAsync(configuration =>
                Filters.ApplyTo(configuration.QueryParameters));

            _summary = response ?? new GetTransactionSummaryResponse();
            _error = null;
        }
        catch (Exception ex)
        {
            _error = ApiErrors.GetMessage(ex);
        }
        finally
        {
            _initialLoading = false;
        }
    }
}
