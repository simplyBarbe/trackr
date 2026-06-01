using frontend.Features.Shared;
using frontend.Infrastructure;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.Kiota.Abstractions;
using MudBlazor;
using Trackr.Api;
using Trackr.Api.Models;

namespace frontend.Features.Transactions;

public partial class TransactionsPage : ComponentBase, IDisposable
{
    [Inject]
    private TrackrApiClient TrackrApi { get; set; } = null!;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = null!;

    private IReadOnlyList<AccountResponse> _accounts = [];
    private IReadOnlyList<CategoryResponse> _categories = [];
    private bool _accountsLoading = true;
    private bool _categoriesLoading = true;
    private string? _accountsError;
    private string? _categoriesError;

    private bool _saving;
    private bool _exporting;

    private MudTable<TransactionResponse>? _table;
    private string? _tableError;

    private GetTransactionSummaryResponse _summary = new();
    private string? _summaryError;
    private bool _summaryInitialLoading = true;

    private string _accountIdFilter = string.Empty;
    private string _categoryIdFilter = string.Empty;
    private string _typeFilter = string.Empty;
    private string _priorityFilter = string.Empty;
    private DateTime? _fromPicker;
    private DateTime? _toPicker;
    private TransactionListFilters _listFilters = new();

    private const int FilterLookupPageSize = 200;

    private readonly DebouncedAsync _filterReload = new();

    private bool CanMutate =>
        !_accountsLoading && _accountsError is null && _accounts.Count > 0;

    private bool IsCategoryFilterDisabled =>
        _categoriesLoading
        || _categoriesError is not null
        || (TryGetTypeFilter(out var type) && type == TransactionType.Transfer);

    private IEnumerable<CategoryResponse> FilterCategoryOptions
    {
        get
        {
            var active = _categories
                .Where(c => c.IsArchived != true)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name);

            if (!TryGetTypeFilter(out var transactionType))
                return active;

            return transactionType switch
            {
                TransactionType.Income => active.Where(c => c.Kind == CategoryKind.Income),
                TransactionType.Expense => active.Where(c => c.Kind == CategoryKind.Expense),
                TransactionType.Transfer => [],
                _ => active
            };
        }
    }

    protected override async Task OnInitializedAsync()
    {
        ApplyCurrentMonthDefault();
        _listFilters = BuildListFilters();
        await Task.WhenAll(LoadAccountsAsync(), LoadCategoriesAsync());
        await LoadSummaryAsync();
    }

    private void ApplyCurrentMonthDefault()
    {
        var today = DateTime.Today;
        _fromPicker = new DateTime(today.Year, today.Month, 1);
        _toPicker = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
    }

    private TransactionListFilters BuildListFilters() => new(
        _accountIdFilter,
        _categoryIdFilter,
        _typeFilter,
        _priorityFilter,
        ToApiDate(_fromPicker),
        ToApiDate(_toPicker));

    private async Task LoadAccountsAsync()
    {
        try
        {
            var response = await TrackrApi.Accounts.GetAsync(configuration =>
            {
                configuration.QueryParameters.Page = 1;
                configuration.QueryParameters.PageSize = FilterLookupPageSize;
            });

            _accounts = response?.Items ?? [];
            _accountsError = null;
        }
        catch (Exception ex)
        {
            _accountsError = ApiErrors.GetMessage(ex);
        }
        finally
        {
            _accountsLoading = false;
        }
    }

    private async Task LoadCategoriesAsync()
    {
        try
        {
            var response = await TrackrApi.Categories.GetAsync(configuration =>
            {
                configuration.QueryParameters.Page = 1;
                configuration.QueryParameters.PageSize = FilterLookupPageSize;
            });

            _categories = response?.Items ?? [];
            _categoriesError = null;
        }
        catch (Exception ex)
        {
            _categoriesError = ApiErrors.GetMessage(ex);
        }
        finally
        {
            _categoriesLoading = false;
        }
    }

    private async Task LoadSummaryAsync()
    {
        try
        {
            var response = await TrackrApi.Transactions.Summary.GetAsync(configuration =>
                _listFilters.ApplyTo(configuration.QueryParameters));

            _summary = response ?? new GetTransactionSummaryResponse();
            _summaryError = null;
        }
        catch (Exception ex)
        {
            _summaryError = ApiErrors.GetMessage(ex);
        }
        finally
        {
            _summaryInitialLoading = false;
        }
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
                    _listFilters.ApplyTo(configuration.QueryParameters);
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

    private Task OnAccountFilterChanged(string value)
    {
        _accountIdFilter = value;
        return SchedulePublishFiltersAsync();
    }

    private Task OnCategoryFilterChanged(string value)
    {
        _categoryIdFilter = value;
        return SchedulePublishFiltersAsync();
    }

    private Task OnTypeFilterChanged(string value)
    {
        _typeFilter = value;
        if (!FilterCategoryOptions.Any(c => c.Id == _categoryIdFilter))
            _categoryIdFilter = string.Empty;

        return SchedulePublishFiltersAsync();
    }

    private bool TryGetTypeFilter(out TransactionType type)
    {
        if (string.IsNullOrEmpty(_typeFilter))
        {
            type = default;
            return false;
        }

        foreach (var candidate in Enum.GetValues<TransactionType>())
        {
            if (ApiEnumWire.GetValue(candidate) == _typeFilter)
            {
                type = candidate;
                return true;
            }
        }

        type = default;
        return false;
    }

    private Task OnPriorityFilterChanged(string value)
    {
        _priorityFilter = value;
        return SchedulePublishFiltersAsync();
    }

    private Task OnFromFilterCommitted(DateTime? value)
    {
        _fromPicker = value;
        return SchedulePublishFiltersAsync();
    }

    private Task OnToFilterCommitted(DateTime? value)
    {
        _toPicker = value;
        return SchedulePublishFiltersAsync();
    }

    private Task SchedulePublishFiltersAsync() =>
        _filterReload.InvokeAsync(PublishFiltersAsync);

    private async Task PublishFiltersAsync()
    {
        _listFilters = BuildListFilters();
        await LoadSummaryAsync();
        await ReloadTableAsync();
    }

    private async Task ReloadTableAsync()
    {
        if (_table is null)
            return;

        if (_table.CurrentPage != 0)
            _table.NavigateTo(0);

        await _table.ReloadServerData();
    }

    public void Dispose() => _filterReload.Dispose();

    private static Date? ToApiDate(DateTime? value) =>
        value is null ? null : new Date(value.Value.Year, value.Value.Month, value.Value.Day);

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

    private async Task OpenCreateDialogAsync()
    {
        var parameters = new DialogParameters<TransactionFormDialog>
        {
            { x => x.Accounts, _accounts },
            { x => x.Categories, _categories },
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

        await SaveAsync(id: null, form);
    }

    private async Task OpenEditDialogAsync(TransactionResponse transaction)
    {
        if (string.IsNullOrWhiteSpace(transaction.Id))
            return;

        var parameters = new DialogParameters<TransactionFormDialog>
        {
            { x => x.Transaction, transaction },
            { x => x.Accounts, _accounts },
            { x => x.Categories, _categories },
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

        await SaveAsync(transaction.Id, form);
    }

    private async Task SaveAsync(string? id, TransactionFormResult form)
    {
        _saving = true;
        try
        {
            if (id is null)
            {
                await TrackrApi.Transactions.PostAsync(new CreateTransactionRequest
                {
                    Type = form.Type,
                    AccountId = form.AccountId,
                    ToAccountId = form.ToAccountId,
                    CategoryId = form.CategoryId,
                    Priority = form.Priority,
                    Amount = form.Amount,
                    OccurredOn = form.OccurredOn,
                    Description = form.Description
                });
            }
            else
            {
                await TrackrApi.Transactions[id].PutAsync(new UpdateTransactionRequest
                {
                    Type = form.Type,
                    AccountId = form.AccountId,
                    ToAccountId = form.ToAccountId,
                    CategoryId = form.CategoryId,
                    Priority = form.Priority,
                    Amount = form.Amount,
                    OccurredOn = form.OccurredOn,
                    Description = form.Description
                });
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add(ApiErrors.GetMessage(ex), Severity.Error, c => c.VisibleStateDuration = 5000);
            return;
        }
        finally
        {
            _saving = false;
        }

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
                _listFilters.ApplyTo(configuration.QueryParameters);
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
