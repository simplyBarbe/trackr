using frontend.Infrastructure;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
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

    private QueryState<IReadOnlyList<AccountResponse>> _query =
        QueryState<IReadOnlyList<AccountResponse>>.Loading();

    private MutationState _mutation = MutationState.Idle;

    private bool _includeArchived;
    private string _typeQuery = string.Empty;
    private string _nameDraft = string.Empty;
    private string _nameFilter = string.Empty;

    protected override async Task OnInitializedAsync() => await LoadAsync();

    private static string FormatCreatedAt(DateTimeOffset? createdAt) =>
        createdAt?.ToLocalTime().ToString("g") ?? "";

    private async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        if (_query.Data is not null)
            _query = QueryState<IReadOnlyList<AccountResponse>>.Fetching(_query.Data);
        else
            _query = QueryState<IReadOnlyList<AccountResponse>>.Loading();

        _query = await QueryState<IReadOnlyList<AccountResponse>>.RunAsync(async () =>
        {
            var response = await Api.Api.Accounts.GetAsync(configuration =>
                {
                    configuration.QueryParameters.IncludeArchived = _includeArchived;
                    if (!string.IsNullOrEmpty(_nameFilter))
                        configuration.QueryParameters.Name = _nameFilter;
                    if (!string.IsNullOrEmpty(_typeQuery))
                        configuration.QueryParameters.Type = _typeQuery;
                },
                cancellationToken);

            return (IReadOnlyList<AccountResponse>)(response?.Items ?? []);
        });
    }

    private async Task OnIncludeArchivedChanged(bool value)
    {
        _includeArchived = value;
        await LoadAsync();
    }

    private async Task OnTypeQueryChanged(string value)
    {
        _typeQuery = value;
        await LoadAsync();
    }

    private async Task CommitNameFilterAsync()
    {
        var trimmed = (_nameDraft ?? string.Empty).Trim();
        if (string.Equals(trimmed, _nameFilter, StringComparison.Ordinal))
            return;

        _nameFilter = trimmed;
        await LoadAsync();
    }

    private Task ApplyNameFilterOnBlur(FocusEventArgs _) => CommitNameFilterAsync();

    private async Task OnNameFilterKeyDown(KeyboardEventArgs e)
    {
        if (e.Key != "Enter")
            return;

        await CommitNameFilterAsync();
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

    private async Task OpenEditDialogAsync(AccountResponse account)
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
            var request = new CreateAccountRequest
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
            var request = new UpdateAccountRequest
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
