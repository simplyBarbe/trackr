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
    private int _refreshVersion;

    private string _accountIdFilter = string.Empty;
    private string _categoryIdFilter = string.Empty;
    private string _typeFilter = string.Empty;
    private string _priorityFilter = string.Empty;
    private Date? _fromFilter;
    private Date? _toFilter;
    private DateTime? _fromPicker;
    private DateTime? _toPicker;
    private TransactionListFilters _listFilters = new();

    private const int FilterLookupPageSize = 200;

    private readonly DebouncedAsync _filterReload = new();

    private bool CanMutate =>
        !_accountsLoading && _accountsError is null && _accounts.Count > 0;

    protected override async Task OnInitializedAsync()
    {
        ApplyCurrentMonthDefault();
        _listFilters = BuildListFilters();
        await Task.WhenAll(LoadAccountsAsync(), LoadCategoriesAsync());
    }

    private void ApplyCurrentMonthDefault()
    {
        var today = DateTime.Today;
        _fromPicker = new DateTime(today.Year, today.Month, 1);
        _toPicker = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
        _fromFilter = ToApiDate(_fromPicker);
        _toFilter = ToApiDate(_toPicker);
    }

    private TransactionListFilters BuildListFilters() => new(
        _accountIdFilter,
        _categoryIdFilter,
        _typeFilter,
        _priorityFilter,
        _fromFilter,
        _toFilter);

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
        return SchedulePublishFiltersAsync();
    }

    private Task OnPriorityFilterChanged(string value)
    {
        _priorityFilter = value;
        return SchedulePublishFiltersAsync();
    }

    private Task OnFromFilterCommitted(DateTime? value)
    {
        _fromPicker = value;
        _fromFilter = ToApiDate(value);
        return SchedulePublishFiltersAsync();
    }

    private Task OnToFilterCommitted(DateTime? value)
    {
        _toPicker = value;
        _toFilter = ToApiDate(value);
        return SchedulePublishFiltersAsync();
    }

    private Task SchedulePublishFiltersAsync() =>
        _filterReload.InvokeAsync(PublishFiltersAsync);

    private Task PublishFiltersAsync()
    {
        _listFilters = BuildListFilters();
        return Task.CompletedTask;
    }

    public void Dispose() => _filterReload.Dispose();

    private static Date? ToApiDate(DateTime? value) =>
        value is null ? null : new Date(value.Value.Year, value.Value.Month, value.Value.Day);

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

        await CreateAsync(form);
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

        await UpdateAsync(transaction.Id, form);
    }

    private async Task CreateAsync(TransactionFormResult form)
    {
        _saving = true;
        try
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

        _refreshVersion++;
    }

    private async Task UpdateAsync(string transactionId, TransactionFormResult form)
    {
        _saving = true;
        try
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

        _refreshVersion++;
    }

    private async Task ExportCsvAsync()
    {
        _exporting = true;
        try
        {
            var response = await TrackrApi.Transactions.Export.GetAsync(configuration =>
            {
                _listFilters.ApplyTo(
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
