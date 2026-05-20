using frontend.Infrastructure;
using Microsoft.AspNetCore.Components;
using Microsoft.Kiota.Abstractions;
using MudBlazor;
using Trackr.Api;
using Trackr.Api.Models;

namespace frontend.Features.Transactions;

public partial class TransactionsPage : ComponentBase
{
    [Inject]
    private TrackrApiClient TrackrApi { get; set; } = null!;

    private QueryState<PagedList<TransactionResponse>> _query =
        QueryState<PagedList<TransactionResponse>>.Loading();

    private QueryState<IReadOnlyList<AccountResponse>> _accountsQuery =
        QueryState<IReadOnlyList<AccountResponse>>.Loading();

    private QueryState<IReadOnlyList<CategoryResponse>> _categoriesQuery =
        QueryState<IReadOnlyList<CategoryResponse>>.Loading();

    private MudTable<TransactionResponse>? _table;
    private int _tableVersion;

    private string _accountIdFilter = string.Empty;
    private string _categoryIdFilter = string.Empty;
    private string _typeFilter = string.Empty;
    private Date? _fromFilter;
    private Date? _toFilter;
    private DateTime? _fromPicker;
    private DateTime? _toPicker;

    private const int FilterLookupPageSize = 200;

    private static string FormatCreatedAt(DateTimeOffset? createdAt) =>
        createdAt?.ToLocalTime().ToString("g") ?? "";

    private static string FormatOccurredOn(Date? occurredOn) =>
        occurredOn?.ToString() ?? "";

    private static string FormatAccount(TransactionResponse transaction)
    {
        if (transaction.Type == TransactionType.Transfer
            && !string.IsNullOrEmpty(transaction.ToAccountName))
        {
            return $"{transaction.AccountName} → {transaction.ToAccountName}";
        }

        return transaction.AccountName ?? "";
    }

    protected override async Task OnInitializedAsync() =>
        await Task.WhenAll(LoadAccountsAsync(), LoadCategoriesAsync());

    private async Task LoadAccountsAsync()
    {
        _accountsQuery = await QueryState<IReadOnlyList<AccountResponse>>.RunAsync(async () =>
        {
            var response = await TrackrApi.Accounts.GetAsync(configuration =>
            {
                configuration.QueryParameters.Page = 1;
                configuration.QueryParameters.PageSize = FilterLookupPageSize;
            });

            return (IReadOnlyList<AccountResponse>)(response?.Items ?? []);
        });
    }

    private async Task LoadCategoriesAsync()
    {
        _categoriesQuery = await QueryState<IReadOnlyList<CategoryResponse>>.RunAsync(async () =>
        {
            var response = await TrackrApi.Categories.GetAsync(configuration =>
            {
                configuration.QueryParameters.Page = 1;
                configuration.QueryParameters.PageSize = FilterLookupPageSize;
            });

            return (IReadOnlyList<CategoryResponse>)(response?.Items ?? []);
        });
    }

    private async Task<TableData<TransactionResponse>> LoadServerDataAsync(
        TableState state,
        CancellationToken cancellationToken)
    {
        if (_query.Data is not null)
            _query = QueryState<PagedList<TransactionResponse>>.Fetching(_query.Data);
        else
            _query = QueryState<PagedList<TransactionResponse>>.Loading();

        var page = state.Page + 1;
        var pageSize = state.PageSize > 0 ? state.PageSize : 50;

        _query = await QueryState<PagedList<TransactionResponse>>.RunAsync(async () =>
        {
            var response = await TrackrApi.Transactions.GetAsync(configuration =>
                {
                    configuration.QueryParameters.Page = page;
                    configuration.QueryParameters.PageSize = pageSize;
                    if (!string.IsNullOrEmpty(_accountIdFilter))
                        configuration.QueryParameters.AccountId = _accountIdFilter;
                    if (!string.IsNullOrEmpty(_categoryIdFilter))
                        configuration.QueryParameters.CategoryId = _categoryIdFilter;
                    if (!string.IsNullOrEmpty(_typeFilter))
                        configuration.QueryParameters.Type = _typeFilter;
                    if (_fromFilter is not null)
                        configuration.QueryParameters.From = _fromFilter;
                    if (_toFilter is not null)
                        configuration.QueryParameters.To = _toFilter;
                },
                cancellationToken);

            return new PagedList<TransactionResponse>(
                response?.Items ?? [],
                response?.Page ?? page,
                response?.PageSize ?? pageSize,
                response?.TotalCount ?? 0);
        });

        if (_query.Data is null)
            return new TableData<TransactionResponse> { Items = [], TotalItems = 0 };

        return new TableData<TransactionResponse>
        {
            Items = _query.Data.Items,
            TotalItems = _query.Data.TotalCount
        };
    }

    private async Task OnAccountFilterChanged(string value)
    {
        _accountIdFilter = value;
        await ResetToFirstPageAndReloadAsync();
    }

    private async Task OnCategoryFilterChanged(string value)
    {
        _categoryIdFilter = value;
        await ResetToFirstPageAndReloadAsync();
    }

    private async Task OnTypeFilterChanged(string value)
    {
        _typeFilter = value;
        await ResetToFirstPageAndReloadAsync();
    }

    private async Task OnFromPickerChanged(DateTime? value)
    {
        _fromPicker = value;
        _fromFilter = ToApiDate(value);
        await ResetToFirstPageAndReloadAsync();
    }

    private async Task OnToPickerChanged(DateTime? value)
    {
        _toPicker = value;
        _toFilter = ToApiDate(value);
        await ResetToFirstPageAndReloadAsync();
    }

    private static Date? ToApiDate(DateTime? value) =>
        value is null ? null : new Date(value.Value.Year, value.Value.Month, value.Value.Day);

    private Task ResetToFirstPageAndReloadAsync()
    {
        _tableVersion++;
        return InvokeAsync(StateHasChanged);
    }
}
