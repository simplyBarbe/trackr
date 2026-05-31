using frontend.Infrastructure;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Trackr.Api;
using Trackr.Api.Models;

namespace frontend.Features.RecurringTransactions;

public partial class RecurringTransactionsPage : ComponentBase, IDisposable
{
    [Inject]
    private TrackrApiClient TrackrApi { get; set; } = null!;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    private IReadOnlyList<AccountResponse> _accounts = [];
    private IReadOnlyList<CategoryResponse> _categories = [];
    private bool _accountsLoading = true;
    private bool _categoriesLoading = true;
    private string? _accountsError;

    private bool _saving;
    private int _refreshVersion;
    private bool? _activeFilterDraft;
    private RecurringListFilters _listFilters = new();

    private readonly DebouncedAsync _filterPublish = new();

    private bool CanMutate =>
        !_accountsLoading && _accountsError is null && _accounts.Count > 0;

    protected override async Task OnInitializedAsync()
    {
        await Task.WhenAll(LoadAccountsAsync(), LoadCategoriesAsync());
    }

    private async Task LoadAccountsAsync()
    {
        try
        {
            var response = await TrackrApi.Accounts.GetAsync(config =>
            {
                config.QueryParameters.Page = 1;
                config.QueryParameters.PageSize = 200;
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
            var response = await TrackrApi.Categories.GetAsync(config =>
            {
                config.QueryParameters.Page = 1;
                config.QueryParameters.PageSize = 200;
            });

            _categories = response?.Items ?? [];
        }
        finally
        {
            _categoriesLoading = false;
        }
    }

    private Task OnActiveFilterChanged(bool? value)
    {
        _activeFilterDraft = value;
        return SchedulePublishFiltersAsync();
    }

    private Task SchedulePublishFiltersAsync() =>
        _filterPublish.InvokeAsync(PublishFiltersAsync);

    private Task PublishFiltersAsync()
    {
        _listFilters = new RecurringListFilters(_activeFilterDraft);
        return Task.CompletedTask;
    }

    public void Dispose() => _filterPublish.Dispose();

    private async Task OpenCreateDialogAsync()
    {
        var parameters = new DialogParameters<RecurringTransactionFormDialog>
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

        var dialog = await DialogService.ShowAsync<RecurringTransactionFormDialog>(
            "New recurring transaction",
            parameters,
            options);
        var result = await dialog.Result;

        if (result is null || result.Canceled || result.Data is not RecurringTransactionFormResult form)
            return;

        await CreateAsync(form);
    }

    private async Task OpenEditDialogAsync(RecurringTransactionResponse recurring)
    {
        if (string.IsNullOrWhiteSpace(recurring.Id))
            return;

        var parameters = new DialogParameters<RecurringTransactionFormDialog>
        {
            { x => x.Recurring, recurring },
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

        var dialog = await DialogService.ShowAsync<RecurringTransactionFormDialog>(
            "Edit recurring transaction",
            parameters,
            options);
        var result = await dialog.Result;

        if (result is null || result.Canceled || result.Data is not RecurringTransactionFormResult form)
            return;

        await UpdateAsync(recurring.Id, form);
    }

    private async Task CreateAsync(RecurringTransactionFormResult form)
    {
        _saving = true;
        try
        {
            await TrackrApi.RecurringTransactions.PostAsync(MapCreateRequest(form));
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

    private async Task UpdateAsync(string id, RecurringTransactionFormResult form)
    {
        _saving = true;
        try
        {
            await TrackrApi.RecurringTransactions[id].PutAsync(MapUpdateRequest(form));
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

    private async Task GenerateNowAsync(RecurringTransactionResponse recurring)
    {
        if (string.IsNullOrWhiteSpace(recurring.Id))
            return;

        _saving = true;
        GenerateRecurringTransactionResponse? response = null;
        try
        {
            response = await TrackrApi.RecurringTransactions[recurring.Id].GenerateNow.PostAsync();
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

        var created = response?.TransactionsCreated ?? 0;
        if (created > 0)
        {
            Snackbar.Add(
                created == 1 ? "Created 1 transaction." : $"Created {created} transactions.",
                Severity.Success,
                c => c.VisibleStateDuration = 5000);
        }
        else
        {
            Snackbar.Add("Nothing due to post.", Severity.Info, c => c.VisibleStateDuration = 5000);
        }

        _refreshVersion++;
    }

    private async Task ArchiveAsync(RecurringTransactionResponse recurring)
    {
        if (string.IsNullOrWhiteSpace(recurring.Id))
            return;

        _saving = true;
        try
        {
            await TrackrApi.RecurringTransactions[recurring.Id].Archive.PostAsync();
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

    private async Task ToggleActiveAsync(RecurringTransactionResponse recurring)
    {
        if (string.IsNullOrWhiteSpace(recurring.Id))
            return;

        var request = new UpdateRecurringTransactionRequest
        {
            Type = recurring.Type,
            AccountId = recurring.AccountId,
            ToAccountId = recurring.ToAccountId,
            CategoryId = recurring.CategoryId,
            Priority = recurring.Priority,
            Amount = recurring.Amount,
            Description = recurring.Description,
            Frequency = recurring.Frequency,
            DayOfWeek = recurring.DayOfWeek,
            DayOfMonth = recurring.DayOfMonth,
            Month = recurring.Month,
            StartOn = recurring.StartOn,
            EndOn = recurring.EndOn,
            IsActive = !(recurring.IsActive ?? true)
        };

        _saving = true;
        try
        {
            await TrackrApi.RecurringTransactions[recurring.Id].PutAsync(request);
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

    private static CreateRecurringTransactionRequest MapCreateRequest(RecurringTransactionFormResult form) =>
        new()
        {
            Type = form.Type,
            AccountId = form.AccountId,
            ToAccountId = form.ToAccountId,
            CategoryId = form.CategoryId,
            Priority = form.Priority,
            Amount = form.Amount,
            Description = form.Description,
            Frequency = form.Frequency,
            DayOfWeek = form.DayOfWeek,
            DayOfMonth = form.DayOfMonth,
            Month = form.Month,
            StartOn = form.StartOn,
            EndOn = form.EndOn
        };

    private static UpdateRecurringTransactionRequest MapUpdateRequest(RecurringTransactionFormResult form) =>
        new()
        {
            Type = form.Type,
            AccountId = form.AccountId,
            ToAccountId = form.ToAccountId,
            CategoryId = form.CategoryId,
            Priority = form.Priority,
            Amount = form.Amount,
            Description = form.Description,
            Frequency = form.Frequency,
            DayOfWeek = form.DayOfWeek,
            DayOfMonth = form.DayOfMonth,
            Month = form.Month,
            StartOn = form.StartOn,
            EndOn = form.EndOn,
            IsActive = form.IsActive
        };
}
