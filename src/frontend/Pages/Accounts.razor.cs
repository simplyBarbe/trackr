using Microsoft.AspNetCore.Components;
using Microsoft.Kiota.Abstractions;
using MudBlazor;
using Trackr.Api;
using Trackr.Api.Models;

namespace frontend.Pages;

public partial class Accounts : ComponentBase
{
    [Inject]
    private TrackrApiClient Api { get; set; } = null!;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    private bool _loading = true;
    private bool _mutating;
    private string? _error;
    private IReadOnlyList<BackendFeaturesAccountsAccountResponse> _rows = [];

    private bool _includeArchived;

    protected override async Task OnInitializedAsync() => await LoadAsync();

    private static string FormatCreatedAt(DateTimeOffset? createdAt) =>
        createdAt?.ToLocalTime().ToString("g") ?? "";

    private async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        _loading = true;
        _error = null;
        try
        {
            var response = await Api.Api.Accounts.GetAsync(configuration =>
                {
                    configuration.QueryParameters.IncludeArchived = _includeArchived;
                },
                cancellationToken);

            var items = response?.Items ?? [];
            _rows = items;
        }
        catch (FastEndpointsErrorResponse ex)
        {
            _error = ex.Message;
            _rows = [];
        }
        catch (ApiException ex)
        {
            _error = ex.Message;
            _rows = [];
        }
        catch (Exception ex)
        {
            _error = ex.Message;
            _rows = [];
        }
        finally
        {
            _loading = false;
        }
    }

    private async Task OnIncludeArchivedChanged(bool value)
    {
        _includeArchived = value;
        await LoadAsync();
    }

    private async Task OpenCreateDialogAsync()
    {
        var parameters = new DialogParameters<CreateAccountDialog>
        {
            { x => x.SubmitText, "Create" }
        };
        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Small,
            FullWidth = true,
            CloseButton = true
        };

        var dialog = await DialogService.ShowAsync<CreateAccountDialog>("New account", parameters, options);
        var result = await dialog.Result;

        if (result is null || result.Canceled || result.Data is not AccountFormResult form)
            return;

        await CreateAsync(form);
    }

    private async Task OpenEditDialogAsync(BackendFeaturesAccountsAccountResponse account)
    {
        if (string.IsNullOrWhiteSpace(account.Id))
            return;

        var parameters = new DialogParameters<CreateAccountDialog>
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

        var dialog = await DialogService.ShowAsync<CreateAccountDialog>("Edit account", parameters, options);
        var result = await dialog.Result;

        if (result is null || result.Canceled || result.Data is not AccountFormResult form)
            return;

        await UpdateAsync(account.Id, form);
    }

    private async Task CreateAsync(AccountFormResult form)
    {
        _mutating = true;
        _error = null;
        try
        {
            var request = new BackendFeaturesAccountsCreateCreateAccountRequest
            {
                Name = form.Name,
                Type = form.Type,
                Currency = form.Currency,
                InitialBalance = form.InitialBalance
            };

            await Api.Api.Accounts.PostAsync(request);
            await LoadAsync();
        }
        catch (FastEndpointsErrorResponse ex)
        {
            _error = ex.Message;
        }
        catch (ApiException ex)
        {
            _error = ex.Message;
        }
        catch (Exception ex)
        {
            _error = ex.Message;
        }
        finally
        {
            _mutating = false;
        }
    }

    private async Task UpdateAsync(string accountId, AccountFormResult form)
    {
        _mutating = true;
        _error = null;
        try
        {
            var request = new BackendFeaturesAccountsUpdateUpdateAccountRequest
            {
                Name = form.Name,
                Type = form.Type,
                Currency = form.Currency,
                InitialBalance = form.InitialBalance
            };

            await Api.Api.Accounts[accountId].PutAsync(request);
            await LoadAsync();
        }
        catch (FastEndpointsErrorResponse ex)
        {
            _error = ex.Message;
        }
        catch (ApiException ex)
        {
            _error = ex.Message;
        }
        catch (Exception ex)
        {
            _error = ex.Message;
        }
        finally
        {
            _mutating = false;
        }
    }
}
