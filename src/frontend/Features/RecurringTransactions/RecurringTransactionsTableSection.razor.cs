using frontend.Infrastructure;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Trackr.Api;
using Trackr.Api.Models;

namespace frontend.Features.RecurringTransactions;

public partial class RecurringTransactionsTableSection : ComponentBase
{
    [Inject]
    private TrackrApiClient TrackrApi { get; set; } = null!;

    [Parameter, EditorRequired]
    public RecurringListFilters Filters { get; set; } = null!;

    [Parameter]
    public int RefreshVersion { get; set; }

    [Parameter]
    public bool CanEdit { get; set; }

    [Parameter]
    public bool Saving { get; set; }

    [Parameter]
    public EventCallback<RecurringTransactionResponse> OnEdit { get; set; }

    [Parameter]
    public EventCallback<RecurringTransactionResponse> OnGenerateNow { get; set; }

    [Parameter]
    public EventCallback<RecurringTransactionResponse> OnToggleActive { get; set; }

    [Parameter]
    public EventCallback<RecurringTransactionResponse> OnArchive { get; set; }

    private MudTable<RecurringTransactionResponse>? _table;
    private string? _tableError;
    private RecurringListFilters? _loadedForFilters;
    private int _loadedForVersion = -1;

    protected override async Task OnParametersSetAsync()
    {
        if (_loadedForFilters == Filters && _loadedForVersion == RefreshVersion)
            return;

        var filtersChanged = _loadedForFilters is not null && _loadedForFilters != Filters;
        _loadedForFilters = Filters;
        _loadedForVersion = RefreshVersion;

        if (_table is null)
            return;

        if (filtersChanged && _table.CurrentPage != 0)
            _table.NavigateTo(0);

        await _table.ReloadServerData();
    }

    private async Task<TableData<RecurringTransactionResponse>> LoadServerDataAsync(
        TableState state,
        CancellationToken cancellationToken)
    {
        var page = state.Page + 1;
        var pageSize = state.PageSize > 0 ? state.PageSize : PaginationDefaults.PageSize;

        try
        {
            var response = await TrackrApi.RecurringTransactions.GetAsync(config =>
            {
                config.QueryParameters.Page = page;
                config.QueryParameters.PageSize = pageSize;
                if (Filters.IsActive is not null)
                    config.QueryParameters.IsActive = Filters.IsActive;
            }, cancellationToken);

            if (_tableError is not null)
                _tableError = null;

            return new TableData<RecurringTransactionResponse>
            {
                Items = response?.Items ?? [],
                TotalItems = response?.TotalCount ?? 0
            };
        }
        catch (Exception ex)
        {
            _tableError = ApiErrors.GetMessage(ex);
            return new TableData<RecurringTransactionResponse> { Items = [], TotalItems = 0 };
        }
    }

    private static string FormatAmount(decimal? amount) =>
        amount?.ToString("N2") ?? "";
}
