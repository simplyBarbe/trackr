using frontend.Infrastructure;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Trackr.Api;
using Trackr.Api.Models;

namespace frontend.Features.Accounts;

public partial class AccountsPage : ComponentBase
{
    [Inject]
    private TrackrApiClient Api { get; set; } = null!;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    private QueryState<IReadOnlyList<BackendFeaturesAccountsAccountResponse>> _query =
        QueryState<IReadOnlyList<BackendFeaturesAccountsAccountResponse>>.Loading();

    private MutationState _mutation = MutationState.Idle;

    private bool _includeArchived;

    protected override async Task OnInitializedAsync() => await LoadAsync();

    private static string FormatCreatedAt(DateTimeOffset? createdAt) =>
        createdAt?.ToLocalTime().ToString("g") ?? "";

    private async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        if (_query.Data is not null)
            _query = QueryState<IReadOnlyList<BackendFeaturesAccountsAccountResponse>>.Fetching(_query.Data);
        else
            _query = QueryState<IReadOnlyList<BackendFeaturesAccountsAccountResponse>>.Loading();

        _query = await QueryState<IReadOnlyList<BackendFeaturesAccountsAccountResponse>>.RunAsync(async () =>
        {
            var response = await Api.Api.Accounts.GetAsync(configuration =>
                {
                    configuration.QueryParameters.IncludeArchived = _includeArchived;
                },
                cancellationToken);

            return (IReadOnlyList<BackendFeaturesAccountsAccountResponse>)(response?.Items ?? []);
        });
    }

    private async Task OnIncludeArchivedChanged(bool value)
    {
        _includeArchived = value;
        await LoadAsync();
    }

    private async Task OpenCreateDialogAsync()
    {
        var parameters = new DialogParameters<AccountFormDialog>
        {
            { x => x.SubmitText, "Create" }
        };
        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Small,
            FullWidth = true,
            CloseButton = true
        };

        var dialog = await DialogService.ShowAsync<AccountFormDialog>("New account", parameters, options);
        var result = await dialog.Result;

        if (result is null || result.Canceled || result.Data is not AccountFormResult form)
            return;

        await CreateAsync(form);
    }

    private async Task OpenEditDialogAsync(BackendFeaturesAccountsAccountResponse account)
    {
        if (string.IsNullOrWhiteSpace(account.Id))
            return;

        var parameters = new DialogParameters<AccountFormDialog>
        {
            { x => x.Account, account },
            { x => x.SubmitText, "Save" }
        };
        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Small,
            FullWidth = true,
            CloseButton = true
        };

        var dialog = await DialogService.ShowAsync<AccountFormDialog>("Edit account", parameters, options);
        var result = await dialog.Result;

        if (result is null || result.Canceled || result.Data is not AccountFormResult form)
            return;

        await UpdateAsync(account.Id, form);
    }

    private async Task CreateAsync(AccountFormResult form)
    {
        _mutation = MutationState.Pending();
        _mutation = await MutationState.RunAsync(async () =>
        {
            var request = new BackendFeaturesAccountsCreateCreateAccountRequest
            {
                Name = form.Name,
                Type = form.Type,
                Currency = form.Currency,
                InitialBalance = form.InitialBalance
            };

            await Api.Api.Accounts.PostAsync(request);
        });

        if (_mutation.Error is not null)
        {
            Snackbar.Add(_mutation.Error, Severity.Error, c => c.VisibleStateDuration = 5000);
            return;
        }

        await LoadAsync();
    }

    private async Task UpdateAsync(string accountId, AccountFormResult form)
    {
        _mutation = MutationState.Pending();
        _mutation = await MutationState.RunAsync(async () =>
        {
            var request = new BackendFeaturesAccountsUpdateUpdateAccountRequest
            {
                Name = form.Name,
                Type = form.Type,
                Currency = form.Currency,
                InitialBalance = form.InitialBalance
            };

            await Api.Api.Accounts[accountId].PutAsync(request);
        });

        if (_mutation.Error is not null)
        {
            Snackbar.Add(_mutation.Error, Severity.Error, c => c.VisibleStateDuration = 5000);
            return;
        }

        await LoadAsync();
    }
}
