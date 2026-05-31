using frontend.Infrastructure;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Trackr.Api;
using Trackr.Api.Models;

namespace frontend.Features.RecurringTransactions;

public partial class RecurringTransactionsPage : ComponentBase
{
    [Inject]
    private TrackrApiClient TrackrApi { get; set; } = null!;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    private QueryState<IReadOnlyList<AccountResponse>> _accountsQuery =
        QueryState<IReadOnlyList<AccountResponse>>.Loading();

    private QueryState<IReadOnlyList<CategoryResponse>> _categoriesQuery =
        QueryState<IReadOnlyList<CategoryResponse>>.Loading();

    private MutationState _mutation = MutationState.Idle;
    private MudTable<RecurringTransactionResponse>? _table;
    private string? _tableError;

    private bool? _activeFilter;

    private bool CanMutate => _accountsQuery.HasData && !_accountsQuery.IsError;

    protected override async Task OnInitializedAsync()
    {
        await Task.WhenAll(LoadAccountsAsync(), LoadCategoriesAsync());
    }

    private async Task LoadAccountsAsync()
    {
        _accountsQuery = await QueryState<IReadOnlyList<AccountResponse>>.RunAsync(async () =>
        {
            var response = await TrackrApi.Accounts.GetAsync(config =>
            {
                config.QueryParameters.Page = 1;
                config.QueryParameters.PageSize = 200;
            });

            return (IReadOnlyList<AccountResponse>)(response?.Items ?? []);
        });
    }

    private async Task LoadCategoriesAsync()
    {
        _categoriesQuery = await QueryState<IReadOnlyList<CategoryResponse>>.RunAsync(async () =>
        {
            var response = await TrackrApi.Categories.GetAsync(config =>
            {
                config.QueryParameters.Page = 1;
                config.QueryParameters.PageSize = 200;
            });

            return (IReadOnlyList<CategoryResponse>)(response?.Items ?? []);
        });
    }

    private async Task<TableData<RecurringTransactionResponse>> LoadServerDataAsync(
        TableState state,
        CancellationToken cancellationToken)
    {
        var page = state.Page + 1;
        var pageSize = state.PageSize > 0 ? state.PageSize : 50;

        try
        {
            var response = await TrackrApi.RecurringTransactions.GetAsync(config =>
            {
                config.QueryParameters.Page = page;
                config.QueryParameters.PageSize = pageSize;
                if (_activeFilter is not null)
                    config.QueryParameters.IsActive = _activeFilter;
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

    private async Task OnActiveFilterChanged(bool? value)
    {
        _activeFilter = value;
        await ReloadTableAsync();
    }

    private async Task OpenCreateDialogAsync()
    {
        var parameters = new DialogParameters<RecurringTransactionFormDialog>
        {
            { x => x.Accounts, _accountsQuery.Data ?? [] },
            { x => x.Categories, _categoriesQuery.Data ?? [] },
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
            { x => x.Accounts, _accountsQuery.Data ?? [] },
            { x => x.Categories, _categoriesQuery.Data ?? [] },
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
        _mutation = MutationState.Pending();
        _mutation = await MutationState.RunAsync(async () =>
        {
            var request = MapCreateRequest(form);
            await TrackrApi.RecurringTransactions.PostAsync(request);
        });

        if (_mutation.Error is not null)
        {
            Snackbar.Add(_mutation.Error, Severity.Error, c => c.VisibleStateDuration = 5000);
            return;
        }

        if (_table is not null)
            await _table.ReloadServerData();
    }

    private async Task UpdateAsync(string id, RecurringTransactionFormResult form)
    {
        _mutation = MutationState.Pending();
        _mutation = await MutationState.RunAsync(async () =>
        {
            var request = MapUpdateRequest(form);
            await TrackrApi.RecurringTransactions[id].PutAsync(request);
        });

        if (_mutation.Error is not null)
        {
            Snackbar.Add(_mutation.Error, Severity.Error, c => c.VisibleStateDuration = 5000);
            return;
        }

        if (_table is not null)
            await _table.ReloadServerData();
    }

    private async Task GenerateNowAsync(RecurringTransactionResponse recurring)
    {
        if (string.IsNullOrWhiteSpace(recurring.Id))
            return;

        _mutation = MutationState.Pending();
        GenerateRecurringTransactionResponse? response = null;
        _mutation = await MutationState.RunAsync(async () =>
        {
            response = await TrackrApi.RecurringTransactions[recurring.Id].GenerateNow.PostAsync();
        });

        if (_mutation.Error is not null)
        {
            Snackbar.Add(_mutation.Error, Severity.Error, c => c.VisibleStateDuration = 5000);
            return;
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

        if (_table is not null)
            await _table.ReloadServerData();
    }

    private async Task ArchiveAsync(RecurringTransactionResponse recurring)
    {
        if (string.IsNullOrWhiteSpace(recurring.Id))
            return;

        _mutation = MutationState.Pending();
        _mutation = await MutationState.RunAsync(async () =>
        {
            await TrackrApi.RecurringTransactions[recurring.Id].Archive.PostAsync();
        });

        if (_mutation.Error is not null)
        {
            Snackbar.Add(_mutation.Error, Severity.Error, c => c.VisibleStateDuration = 5000);
            return;
        }

        if (_table is not null)
            await _table.ReloadServerData();
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

        _mutation = MutationState.Pending();
        _mutation = await MutationState.RunAsync(async () =>
        {
            await TrackrApi.RecurringTransactions[recurring.Id].PutAsync(request);
        });

        if (_mutation.Error is not null)
        {
            Snackbar.Add(_mutation.Error, Severity.Error, c => c.VisibleStateDuration = 5000);
            return;
        }

        if (_table is not null)
            await _table.ReloadServerData();
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

    private static string FormatAmount(decimal? amount) =>
        amount?.ToString("N2") ?? "";

    private async Task ReloadTableAsync()
    {
        if (_table is null)
            return;

        if (_table.CurrentPage != 0)
            _table.NavigateTo(0);

        await _table.ReloadServerData();
    }
}
