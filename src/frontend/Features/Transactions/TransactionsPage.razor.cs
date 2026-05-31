using frontend.Features.Shared;
using frontend.Infrastructure;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.Kiota.Abstractions;
using MudBlazor;
using Trackr.Api;
using Trackr.Api.Models;

namespace frontend.Features.Transactions;

public partial class TransactionsPage : ComponentBase
{
    [Inject]
    private TrackrApiClient TrackrApi { get; set; } = null!;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = null!;

    private QueryState<IReadOnlyList<AccountResponse>> _accountsQuery =
        QueryState<IReadOnlyList<AccountResponse>>.Loading();

    private QueryState<IReadOnlyList<CategoryResponse>> _categoriesQuery =
        QueryState<IReadOnlyList<CategoryResponse>>.Loading();

    private QueryState<GetTransactionSummaryResponse> _summaryQuery =
        QueryState<GetTransactionSummaryResponse>.Loading();

    private MutationState _mutation = MutationState.Idle;
    private bool _exporting;
    private MudTable<TransactionResponse>? _table;
    private string? _tableError;

    private string _accountIdFilter = string.Empty;
    private string _categoryIdFilter = string.Empty;
    private string _typeFilter = string.Empty;
    private string _priorityFilter = string.Empty;
    private Date? _fromFilter;
    private Date? _toFilter;
    private DateTime? _fromPicker;
    private DateTime? _toPicker;

    private const int FilterLookupPageSize = 200;

    private bool CanMutate =>
        !_accountsQuery.IsLoading
        && !_accountsQuery.IsError
        && _accountsQuery.HasData;

    private IReadOnlyList<AccountResponse> ActiveAccounts =>
        _accountsQuery.Data ?? [];

    private IReadOnlyList<CategoryResponse> ActiveCategories =>
        _categoriesQuery.Data ?? [];

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

    protected override async Task OnInitializedAsync()
    {
        ApplyCurrentMonthDefault();
        await Task.WhenAll(LoadAccountsAsync(), LoadCategoriesAsync(), LoadSummaryAsync());
    }

    private void ApplyCurrentMonthDefault()
    {
        var today = DateTime.Today;
        _fromPicker = new DateTime(today.Year, today.Month, 1);
        _toPicker = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
        _fromFilter = ToApiDate(_fromPicker);
        _toFilter = ToApiDate(_toPicker);
    }

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
        var page = state.Page + 1;
        var pageSize = state.PageSize > 0 ? state.PageSize : 50;

        try
        {
            var response = await TrackrApi.Transactions.GetAsync(configuration =>
                {
                    configuration.QueryParameters.Page = page;
                    configuration.QueryParameters.PageSize = pageSize;
                    ApplyTransactionFilters(
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

    private async Task LoadSummaryAsync()
    {
        if (_summaryQuery.Data is not null)
            _summaryQuery = QueryState<GetTransactionSummaryResponse>.Fetching(_summaryQuery.Data);
        else
            _summaryQuery = QueryState<GetTransactionSummaryResponse>.Loading();

        _summaryQuery = await QueryState<GetTransactionSummaryResponse>.RunAsync(async () =>
        {
            var response = await TrackrApi.Transactions.Summary.GetAsync(configuration =>
            {
                ApplyTransactionFilters(
                    v => configuration.QueryParameters.AccountId = v,
                    v => configuration.QueryParameters.CategoryId = v,
                    v => configuration.QueryParameters.Type = v,
                    v => configuration.QueryParameters.Priority = v,
                    v => configuration.QueryParameters.From = v,
                    v => configuration.QueryParameters.To = v);
            });

            return response ?? new GetTransactionSummaryResponse();
        });
    }

    private void ApplyTransactionFilters(
        Action<string?> setAccountId,
        Action<string?> setCategoryId,
        Action<string?> setType,
        Action<string?> setPriority,
        Action<Date?> setFrom,
        Action<Date?> setTo)
    {
        if (!string.IsNullOrEmpty(_accountIdFilter))
            setAccountId(_accountIdFilter);

        if (!string.IsNullOrEmpty(_categoryIdFilter))
            setCategoryId(_categoryIdFilter);

        if (!string.IsNullOrEmpty(_typeFilter))
            setType(_typeFilter);

        if (!string.IsNullOrEmpty(_priorityFilter))
            setPriority(_priorityFilter);

        if (_fromFilter is not null)
            setFrom(_fromFilter);

        if (_toFilter is not null)
            setTo(_toFilter);
    }

    private async Task OnAccountFilterChanged(string value)
    {
        _accountIdFilter = value;
        await ReloadFiltersAsync();
    }

    private async Task OnCategoryFilterChanged(string value)
    {
        _categoryIdFilter = value;
        await ReloadFiltersAsync();
    }

    private async Task OnTypeFilterChanged(string value)
    {
        _typeFilter = value;
        await ReloadFiltersAsync();
    }

    private async Task OnPriorityFilterChanged(string value)
    {
        _priorityFilter = value;
        await ReloadFiltersAsync();
    }

    private async Task OnFromPickerChanged(DateTime? value)
    {
        _fromPicker = value;
        _fromFilter = ToApiDate(value);
        await ReloadFiltersAsync();
    }

    private async Task OnToPickerChanged(DateTime? value)
    {
        _toPicker = value;
        _toFilter = ToApiDate(value);
        await ReloadFiltersAsync();
    }

    private async Task ReloadFiltersAsync()
    {
        await Task.WhenAll(LoadSummaryAsync(), ReloadTableAsync());
    }

    private async Task ReloadTableAsync()
    {
        if (_table is null)
            return;

        if (_table.CurrentPage != 0)
            _table.NavigateTo(0);

        await _table.ReloadServerData();
    }

    private static Date? ToApiDate(DateTime? value) =>
        value is null ? null : new Date(value.Value.Year, value.Value.Month, value.Value.Day);

    private async Task OpenCreateDialogAsync()
    {
        var parameters = new DialogParameters<TransactionFormDialog>
        {
            { x => x.Accounts, ActiveAccounts },
            { x => x.Categories, ActiveCategories },
            { x => x.SubmitText, "Create" }
        };
        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true,
            CloseButton = true
        };

        var dialog = await DialogService.ShowAsync<TransactionFormDialog>(
            "New transaction",
            parameters,
            options);
        var result = await dialog.Result;

        if (result is null || result.Canceled || result.Data is not TransactionFormResult form)
            return;

        await CreateAsync(form);
    }

    private async Task OpenEditDialogAsync(TransactionResponse transaction)
    {
        if (string.IsNullOrWhiteSpace(transaction.Id))
            return;

        var parameters = new DialogParameters<TransactionFormDialog>
        {
            { x => x.Transaction, transaction },
            { x => x.Accounts, ActiveAccounts },
            { x => x.Categories, ActiveCategories },
            { x => x.SubmitText, "Save" }
        };
        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true,
            CloseButton = true
        };

        var dialog = await DialogService.ShowAsync<TransactionFormDialog>(
            "Edit transaction",
            parameters,
            options);
        var result = await dialog.Result;

        if (result is null || result.Canceled || result.Data is not TransactionFormResult form)
            return;

        await UpdateAsync(transaction.Id, form);
    }

    private async Task CreateAsync(TransactionFormResult form)
    {
        _mutation = MutationState.Pending();
        _mutation = await MutationState.RunAsync(async () =>
        {
            var request = new CreateTransactionRequest
            {
                Type = form.Type,
                AccountId = form.AccountId,
                ToAccountId = form.ToAccountId,
                CategoryId = form.CategoryId,
                Priority = form.Priority,
                Amount = form.Amount,
                OccurredOn = form.OccurredOn,
                Description = form.Description
            };

            await TrackrApi.Transactions.PostAsync(request);
        });

        if (_mutation.Error is not null)
        {
            Snackbar.Add(_mutation.Error, Severity.Error, c => c.VisibleStateDuration = 5000);
            return;
        }

        await ReloadAfterMutationAsync();
    }

    private async Task UpdateAsync(string transactionId, TransactionFormResult form)
    {
        _mutation = MutationState.Pending();
        _mutation = await MutationState.RunAsync(async () =>
        {
            var request = new UpdateTransactionRequest
            {
                Type = form.Type,
                AccountId = form.AccountId,
                ToAccountId = form.ToAccountId,
                CategoryId = form.CategoryId,
                Priority = form.Priority,
                Amount = form.Amount,
                OccurredOn = form.OccurredOn,
                Description = form.Description
            };

            await TrackrApi.Transactions[transactionId].PutAsync(request);
        });

        if (_mutation.Error is not null)
        {
            Snackbar.Add(_mutation.Error, Severity.Error, c => c.VisibleStateDuration = 5000);
            return;
        }

        await ReloadAfterMutationAsync();
    }

    private async Task ReloadAfterMutationAsync()
    {
        await LoadSummaryAsync();
        await ReloadTableAsync();
    }

    private async Task ExportCsvAsync()
    {
        _exporting = true;
        try
        {
            var response = await TrackrApi.Transactions.Export.GetAsync(configuration =>
            {
                ApplyTransactionFilters(
                    v => configuration.QueryParameters.AccountId = v,
                    v => configuration.QueryParameters.CategoryId = v,
                    v => configuration.QueryParameters.Type = v,
                    v => configuration.QueryParameters.Priority = v,
                    v => configuration.QueryParameters.From = v,
                    v => configuration.QueryParameters.To = v);
            });

            if (string.IsNullOrEmpty(response?.CsvContent))
            {
                Snackbar.Add("Export returned no data.", Severity.Error, c => c.VisibleStateDuration = 5000);
                return;
            }

            var fileName = string.IsNullOrWhiteSpace(response.FileName)
                ? "transactions.csv"
                : response.FileName;

            await JSRuntime.InvokeVoidAsync("downloadFile", fileName, response.CsvContent);
            Snackbar.Add("CSV exported.", Severity.Success, c => c.VisibleStateDuration = 3000);
        }
        catch (Exception ex)
        {
            Snackbar.Add(ApiErrors.GetMessage(ex), Severity.Error, c => c.VisibleStateDuration = 5000);
        }
        finally
        {
            _exporting = false;
        }
    }
}
