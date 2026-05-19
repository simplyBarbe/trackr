using Microsoft.AspNetCore.Components;
using MudBlazor;
using Trackr.Api.Models;

namespace frontend.Pages;

public partial class CreateAccountDialog : ComponentBase
{
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = null!;

    [Parameter]
    public BackendFeaturesAccountsAccountResponse? Account { get; set; }

    [Parameter]
    public string SubmitText { get; set; } = "Create";

    private MudForm? _form;
    private string? _currencyError;
    private string _name = string.Empty;
    private BackendDataEntitiesEnumsAccountType _type = BackendDataEntitiesEnumsAccountType.Checking;
    private string _currency = "EUR";
    private decimal _initialBalance;

    protected override void OnParametersSet()
    {
        _currencyError = null;

        if (Account is null)
            return;

        _name = Account.Name ?? string.Empty;
        _type = Account.Type ?? BackendDataEntitiesEnumsAccountType.Checking;
        _currency = Account.Currency ?? "EUR";
        _initialBalance = Account.InitialBalance ?? 0m;
    }

    private void OnTypeChanged(BackendDataEntitiesEnumsAccountType type)
    {
        _type = type;
    }

    private async Task SubmitAsync()
    {
        if (_form is null)
            return;

        _currencyError = null;

        await _form.ValidateAsync();
        if (!_form.IsValid)
            return;

        var currency = (_currency ?? string.Empty).Trim().ToUpperInvariant();
        if (currency.Length != 3)
        {
            _currencyError = "Currency must be exactly 3 letters (ISO 4217).";
            return;
        }

        var result = new AccountFormResult(
            _name.Trim(),
            _type,
            currency,
            _initialBalance);

        MudDialog.Close(DialogResult.Ok(result));
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }
}

public sealed record AccountFormResult(
    string Name,
    BackendDataEntitiesEnumsAccountType Type,
    string Currency,
    decimal InitialBalance);
