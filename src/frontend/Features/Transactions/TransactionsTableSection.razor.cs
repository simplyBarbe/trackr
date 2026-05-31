using frontend.Features.Shared;
using frontend.Infrastructure;
using Microsoft.AspNetCore.Components;
using Microsoft.Kiota.Abstractions;
using MudBlazor;
using Trackr.Api;
using Trackr.Api.Models;

namespace frontend.Features.Transactions;

public partial class TransactionsTableSection : ComponentBase
{
    [Inject]
    private TrackrApiClient TrackrApi { get; set; } = null!;

    [Parameter, EditorRequired]
    public TransactionListFilters Filters { get; set; } = null!;

    [Parameter]
    public int RefreshVersion { get; set; }

    [Parameter]
    public bool CanEdit { get; set; }

    [Parameter]
    public bool Saving { get; set; }

    [Parameter]
    public EventCallback<TransactionResponse> OnEdit { get; set; }

    private MudTable<TransactionResponse>? _table;
    private string? _tableError;
    private TransactionListFilters? _loadedForFilters;
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

    private async Task<TableData<TransactionResponse>> LoadServerDataAsync(
        TableState state,
        CancellationToken cancellationToken)
    {
        var page = state.Page + 1;
        var pageSize = state.PageSize > 0 ? state.PageSize : PaginationDefaults.PageSize;

        try
        {
            var response = await TrackrApi.Transactions.GetAsync(configuration =>
                {
                    configuration.QueryParameters.Page = page;
                    configuration.QueryParameters.PageSize = pageSize;
                    Filters.ApplyTo(
                        v => configuration.QueryParameters.AccountId = v,
                        v => configuration.QueryParameters.CategoryId = v,
                        v => configuration.QueryParameters.Type = v,
                        v => configuration.QueryParameters.Priority = v,
                        v => configuration.QueryParameters.From = v,
                        v => configuration.QueryParameters.To = v);
                },
                cancellationToken);

            if (_tableError is not null)
                _tableError = null;

            return new TableData<TransactionResponse>
            {
                Items = response?.Items ?? [],
                TotalItems = response?.TotalCount ?? 0
            };
        }
        catch (Exception ex)
        {
            _tableError = ApiErrors.GetMessage(ex);
            return new TableData<TransactionResponse> { Items = [], TotalItems = 0 };
        }
    }

    private static string FormatCreatedAt(DateTimeOffset? createdAt) =>
        createdAt?.ToLocalTime().ToString("g") ?? "";

    private static string FormatOccurredOn(Date? occurredOn) =>
        occurredOn?.ToString() ?? "";

    private static string FormatAmount(decimal? amount) =>
        MoneyFormat.Format(amount);

    private static string FormatAccount(TransactionResponse transaction)
    {
        if (transaction.Type == TransactionType.Transfer
            && !string.IsNullOrEmpty(transaction.ToAccountName))
        {
            return $"{transaction.AccountName} → {transaction.ToAccountName}";
        }

        return transaction.AccountName ?? "";
    }
}
