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

    private QueryState<IReadOnlyList<BackendFeaturesAccountsAccountResponse>> _accountsQuery =
        QueryState<IReadOnlyList<BackendFeaturesAccountsAccountResponse>>.Loading();

    private QueryState<IReadOnlyList<BackendFeaturesCategoriesCategoryResponse>> _categoriesQuery =
        QueryState<IReadOnlyList<BackendFeaturesCategoriesCategoryResponse>>.Loading();

    private MutationState _mutation = MutationState.Idle;

    private bool _includeArchived;
    private string _categoryIdFilter = string.Empty;
    private bool _categoryFilterLoading;
    private HashSet<string>? _accountIdsForCategory;

    protected override async Task OnInitializedAsync() =>
        await Task.WhenAll(LoadCategoriesAsync(), LoadAccountsAsync());

    private IReadOnlyList<BackendFeaturesAccountsAccountResponse> DisplayedAccounts =>
        FilterAccounts(_accountsQuery.Data);

    private static string FormatCreatedAt(DateTimeOffset? createdAt) =>
        createdAt?.ToLocalTime().ToString("g") ?? "";

    private IReadOnlyList<BackendFeaturesAccountsAccountResponse> FilterAccounts(
        IReadOnlyList<BackendFeaturesAccountsAccountResponse>? rows)
    {
        if (rows is null)
            return [];

        if (string.IsNullOrEmpty(_categoryIdFilter) || _accountIdsForCategory is null)
            return rows;

        return rows
            .Where(a => !string.IsNullOrEmpty(a.Id) && _accountIdsForCategory.Contains(a.Id))
            .ToList();
    }

    private async Task LoadCategoriesAsync(CancellationToken cancellationToken = default)
    {
        if (_categoriesQuery.Data is not null)
            _categoriesQuery = QueryState<IReadOnlyList<BackendFeaturesCategoriesCategoryResponse>>.Fetching(_categoriesQuery.Data);
        else
            _categoriesQuery = QueryState<IReadOnlyList<BackendFeaturesCategoriesCategoryResponse>>.Loading();

        _categoriesQuery = await QueryState<IReadOnlyList<BackendFeaturesCategoriesCategoryResponse>>.RunAsync(async () =>
        {
            var response = await Api.Api.Categories.GetAsync(configuration =>
                {
                    configuration.QueryParameters.IncludeArchived = false;
                },
                cancellationToken);

            return (IReadOnlyList<BackendFeaturesCategoriesCategoryResponse>)(response?.Items ?? []);
        });
    }

    private async Task LoadAccountsAsync(CancellationToken cancellationToken = default)
    {
        if (_accountsQuery.Data is not null)
            _accountsQuery = QueryState<IReadOnlyList<BackendFeaturesAccountsAccountResponse>>.Fetching(_accountsQuery.Data);
        else
            _accountsQuery = QueryState<IReadOnlyList<BackendFeaturesAccountsAccountResponse>>.Loading();

        _accountsQuery = await QueryState<IReadOnlyList<BackendFeaturesAccountsAccountResponse>>.RunAsync(async () =>
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
        await LoadAccountsAsync();
    }

    private async Task OnCategoryFilterChanged(string value)
    {
        _categoryIdFilter = value;
        _accountIdsForCategory = null;

        if (string.IsNullOrEmpty(value))
            return;

        _categoryFilterLoading = true;
        try
        {
            var response = await Api.Api.Transactions.GetAsync(configuration =>
            {
                configuration.QueryParameters.CategoryId = value;
                configuration.QueryParameters.Page = 1;
                configuration.QueryParameters.PageSize = 500;
            });

            var items = response?.Items ?? [];
            _accountIdsForCategory = items
                .Select(t => t.AccountId)
                .Where(id => !string.IsNullOrEmpty(id))
                .ToHashSet(StringComparer.Ordinal)!;
        }
        catch (Exception ex)
        {
            _categoryIdFilter = string.Empty;
            Snackbar.Add(ApiErrors.GetMessage(ex), Severity.Error, c => c.VisibleStateDuration = 5000);
        }
        finally
        {
            _categoryFilterLoading = false;
        }
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

        await LoadAccountsAsync();
        if (!string.IsNullOrEmpty(_categoryIdFilter))
            await OnCategoryFilterChanged(_categoryIdFilter);
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

        await LoadAccountsAsync();
        if (!string.IsNullOrEmpty(_categoryIdFilter))
            await OnCategoryFilterChanged(_categoryIdFilter);
    }
}
