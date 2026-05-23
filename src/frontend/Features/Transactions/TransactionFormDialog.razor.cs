using Microsoft.AspNetCore.Components;
using Microsoft.Kiota.Abstractions;
using MudBlazor;
using Trackr.Api.Models;

namespace frontend.Features.Transactions;

public partial class TransactionFormDialog : ComponentBase
{
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = null!;

    [Parameter]
    public TransactionResponse? Transaction { get; set; }

    [Parameter]
    public IReadOnlyList<AccountResponse> Accounts { get; set; } = [];

    [Parameter]
    public IReadOnlyList<CategoryResponse> Categories { get; set; } = [];

    [Parameter]
    public string SubmitText { get; set; } = "Create";

    private MudForm? _form;
    private string? _formError;
    private TransactionType _type = TransactionType.Expense;
    private string _accountId = string.Empty;
    private string _toAccountId = string.Empty;
    private string _categoryId = string.Empty;
    private ExpensePriority _priority = ExpensePriority.Discretionary;
    private bool _priorityManuallySet;
    private decimal _amount;
    private DateTime? _occurredPicker = DateTime.Today;
    private string _description = string.Empty;

    private IEnumerable<AccountResponse> ActiveAccounts =>
        Accounts.Where(a => a.IsArchived != true).OrderBy(a => a.Name);

    private IEnumerable<AccountResponse> DestinationAccounts =>
        ActiveAccounts.Where(a => a.Id != _accountId);

    private IEnumerable<CategoryResponse> CategoryOptions =>
        Categories
            .Where(c => c.IsArchived != true && c.Kind == ExpectedCategoryKind)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name);

    private CategoryKind? ExpectedCategoryKind =>
        _type switch
        {
            TransactionType.Income => CategoryKind.Income,
            TransactionType.Expense => CategoryKind.Expense,
            _ => null
        };

    private bool ShowToAccount => _type == TransactionType.Transfer;

    private bool ShowCategory => _type is TransactionType.Income or TransactionType.Expense;

    private bool ShowPriority => _type == TransactionType.Expense;

    protected override void OnParametersSet()
    {
        _formError = null;

        if (Transaction is null)
        {
            _priorityManuallySet = false;
            _priority = ExpensePriority.Discretionary;
            return;
        }

        _type = Transaction.Type ?? TransactionType.Expense;
        _accountId = Transaction.AccountId ?? string.Empty;
        _toAccountId = Transaction.ToAccountId ?? string.Empty;
        _categoryId = Transaction.CategoryId ?? string.Empty;
        _priority = Transaction.Priority ?? ExpensePriority.Discretionary;
        _priorityManuallySet = Transaction.Priority is not null;
        _amount = Transaction.Amount ?? 0m;
        _occurredPicker = FromApiDate(Transaction.OccurredOn) ?? DateTime.Today;
        _description = Transaction.Description ?? string.Empty;
    }

    private void OnTypeChanged(TransactionType type)
    {
        _type = type;

        if (type == TransactionType.Transfer)
        {
            _categoryId = string.Empty;
            _priorityManuallySet = false;
            if (_toAccountId == _accountId)
                _toAccountId = string.Empty;
            return;
        }

        _toAccountId = string.Empty;

        if (!CategoryOptions.Any(c => c.Id == _categoryId))
            _categoryId = string.Empty;

        if (type == TransactionType.Expense)
        {
            _priorityManuallySet = false;
            ApplyCategoryPriority();
        }
    }

    private void OnAccountIdChanged(string accountId)
    {
        _accountId = accountId;
        if (_toAccountId == _accountId)
            _toAccountId = string.Empty;
    }

    private void OnToAccountIdChanged(string toAccountId) => _toAccountId = toAccountId;

    private void OnCategoryIdChanged(string categoryId)
    {
        _categoryId = categoryId;
        if (!_priorityManuallySet)
            ApplyCategoryPriority();
    }

    private void OnPriorityChanged(ExpensePriority priority)
    {
        _priority = priority;
        _priorityManuallySet = true;
    }

    private void ApplyCategoryPriority()
    {
        var category = CategoryOptions.FirstOrDefault(c => c.Id == _categoryId);
        _priority = category?.Priority ?? ExpensePriority.Discretionary;
    }

    private async Task SubmitAsync()
    {
        if (_form is null)
            return;

        _formError = null;

        await _form.ValidateAsync();
        if (!_form.IsValid)
            return;

        if (string.IsNullOrWhiteSpace(_accountId))
        {
            _formError = "Account is required.";
            return;
        }

        if (_amount <= 0)
        {
            _formError = "Amount must be greater than zero.";
            return;
        }

        if (ToApiDate(_occurredPicker) is not { } occurredOn)
        {
            _formError = "Date is required.";
            return;
        }

        string? toAccountId = null;
        string? categoryId = null;
        ExpensePriority? priority = null;

        switch (_type)
        {
            case TransactionType.Transfer:
                if (string.IsNullOrWhiteSpace(_toAccountId))
                {
                    _formError = "Destination account is required for transfers.";
                    return;
                }

                if (_toAccountId == _accountId)
                {
                    _formError = "Source and destination accounts must differ.";
                    return;
                }

                toAccountId = _toAccountId;
                break;

            case TransactionType.Income:
            case TransactionType.Expense:
                categoryId = string.IsNullOrWhiteSpace(_categoryId) ? null : _categoryId;
                if (_type == TransactionType.Expense)
                    priority = _priority;
                break;
        }

        var description = string.IsNullOrWhiteSpace(_description) ? null : _description.Trim();

        var result = new TransactionFormResult(
            _type,
            _accountId,
            toAccountId,
            categoryId,
            priority,
            _amount,
            occurredOn,
            description);

        MudDialog.Close(DialogResult.Ok(result));
    }

    private void Cancel() => MudDialog.Cancel();

    private static Date? ToApiDate(DateTime? value) =>
        value is null ? null : new Date(value.Value.Year, value.Value.Month, value.Value.Day);

    private static DateTime? FromApiDate(Date? value)
    {
        if (value is null)
            return null;

        var d = value.Value;
        return new DateTime(d.Year, d.Month, d.Day);
    }
}

public sealed record TransactionFormResult(
    TransactionType Type,
    string AccountId,
    string? ToAccountId,
    string? CategoryId,
    ExpensePriority? Priority,
    decimal Amount,
    Date OccurredOn,
    string? Description);
