using Microsoft.AspNetCore.Components;
using Microsoft.Kiota.Abstractions;
using MudBlazor;
using Trackr.Api.Models;

namespace frontend.Features.RecurringTransactions;

public partial class RecurringTransactionFormDialog : ComponentBase
{
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = null!;

    [Parameter]
    public RecurringTransactionResponse? Recurring { get; set; }

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
    private string _description = string.Empty;
    private RecurrenceFrequency _frequency = RecurrenceFrequency.Monthly;
    private DayOfWeekObject _dayOfWeek = DayOfWeekObject.Monday;
    private int _dayOfMonth = 1;
    private int _month = 1;
    private DateTime? _startPicker = DateTime.Today;
    private DateTime? _endPicker;
    private bool _isActive = true;

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
    private bool ShowDayOfWeek => _frequency is RecurrenceFrequency.Weekly or RecurrenceFrequency.Biweekly;
    private bool ShowDayOfMonth => _frequency is RecurrenceFrequency.Monthly or RecurrenceFrequency.Yearly;
    private bool ShowMonth => _frequency == RecurrenceFrequency.Yearly;
    private bool IsEdit => Recurring is not null;

    protected override void OnParametersSet()
    {
        _formError = null;

        if (Recurring is null)
        {
            _priorityManuallySet = false;
            _priority = ExpensePriority.Discretionary;
            _isActive = true;
            return;
        }

        _type = Recurring.Type ?? TransactionType.Expense;
        _accountId = Recurring.AccountId ?? string.Empty;
        _toAccountId = Recurring.ToAccountId ?? string.Empty;
        _categoryId = Recurring.CategoryId ?? string.Empty;
        _priority = Recurring.Priority ?? ExpensePriority.Discretionary;
        _priorityManuallySet = Recurring.Priority is not null;
        _amount = Recurring.Amount ?? 0m;
        _description = Recurring.Description ?? string.Empty;
        _frequency = Recurring.Frequency ?? RecurrenceFrequency.Monthly;
        _dayOfWeek = Recurring.DayOfWeek ?? DayOfWeekObject.Monday;
        _dayOfMonth = Recurring.DayOfMonth ?? 1;
        _month = Recurring.Month ?? 1;
        _startPicker = RecurringFormat.FromApiDate(Recurring.StartOn) ?? DateTime.Today;
        _endPicker = RecurringFormat.FromApiDate(Recurring.EndOn);
        _isActive = Recurring.IsActive ?? true;
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

        if (RecurringFormat.ToApiDate(_startPicker) is not { } startOn)
        {
            _formError = "Start date is required.";
            return;
        }

        if (_frequency is RecurrenceFrequency.Monthly or RecurrenceFrequency.Yearly
            && (_dayOfMonth < 1 || _dayOfMonth > 31))
        {
            _formError = "Day of month must be between 1 and 31.";
            return;
        }

        if (_frequency == RecurrenceFrequency.Yearly && (_month < 1 || _month > 12))
        {
            _formError = "Month must be between 1 and 12.";
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

        var result = new RecurringTransactionFormResult(
            _type,
            _accountId,
            toAccountId,
            categoryId,
            priority,
            _amount,
            string.IsNullOrWhiteSpace(_description) ? null : _description.Trim(),
            _frequency,
            ShowDayOfWeek ? _dayOfWeek : null,
            ShowDayOfMonth ? _dayOfMonth : null,
            ShowMonth ? _month : null,
            startOn,
            RecurringFormat.ToApiDate(_endPicker),
            _isActive);

        MudDialog.Close(DialogResult.Ok(result));
    }

    private void Cancel() => MudDialog.Cancel();
}

public sealed record RecurringTransactionFormResult(
    TransactionType Type,
    string AccountId,
    string? ToAccountId,
    string? CategoryId,
    ExpensePriority? Priority,
    decimal Amount,
    string? Description,
    RecurrenceFrequency Frequency,
    DayOfWeekObject? DayOfWeek,
    int? DayOfMonth,
    int? Month,
    Date StartOn,
    Date? EndOn,
    bool IsActive);
