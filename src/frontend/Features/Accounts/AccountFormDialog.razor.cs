using Microsoft.AspNetCore.Components;
using MudBlazor;
using Trackr.Api.Models;

namespace frontend.Features.Accounts;

public partial class AccountFormDialog : ComponentBase
{
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = null!;

    [Parameter]
    public AccountResponse? Account { get; set; }

    [Parameter]
    public string SubmitText { get; set; } = "Create";

    private MudForm? _form;
    private string? _currencyError;
    private string _name = string.Empty;
    private AccountType _type = AccountType.Checking;
    private AccountColor _color = AccountColor.Primary;
    private string _currency = "EUR";
    private decimal _initialBalance;

    protected override void OnParametersSet()
    {
        _currencyError = null;

        if (Account is null)
            return;

        _name = Account.Name ?? string.Empty;
        _type = Account.Type ?? AccountType.Checking;
        _color = Account.Color ?? AccountColor.Primary;
        _currency = Account.Currency ?? "EUR";
        _initialBalance = Account.InitialBalance ?? 0m;
    }

    private void OnTypeChanged(AccountType type)
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
            _color,
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
    AccountType Type,
    AccountColor Color,
    string Currency,
    decimal InitialBalance);
