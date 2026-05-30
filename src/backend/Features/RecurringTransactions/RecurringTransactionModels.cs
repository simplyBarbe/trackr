using backend.Common.Pagination;
using backend.Data.Entities;
using backend.Data.Entities.Enums;

namespace backend.Features.RecurringTransactions;

public sealed record RecurringTransactionResponse(
    Guid Id,
    TransactionType Type,
    Guid AccountId,
    string AccountName,
    Guid? ToAccountId,
    string? ToAccountName,
    Guid? CategoryId,
    string? CategoryName,
    ExpensePriority? Priority,
    decimal Amount,
    string? Description,
    RecurrenceFrequency Frequency,
    DayOfWeek? DayOfWeek,
    int? DayOfMonth,
    int? Month,
    DateOnly StartOn,
    DateOnly? EndOn,
    DateOnly NextOccurrenceOn,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record ListRecurringTransactionsResponse(
    IReadOnlyList<RecurringTransactionResponse> Items,
    int Page,
    int PageSize,
    int TotalCount)
    : PagedResponse<RecurringTransactionResponse>(Items, Page, PageSize, TotalCount);

public sealed record GenerateRecurringTransactionResponse(int TransactionsCreated);

public static class RecurringTransactionMapping
{
    public static RecurringTransactionResponse ToResponse(RecurringTransaction rule) =>
        new(
            rule.Id,
            rule.Type,
            rule.AccountId,
            rule.Account.Name,
            rule.ToAccountId,
            rule.ToAccount?.Name,
            rule.CategoryId,
            rule.Category?.Name,
            rule.Priority,
            rule.Amount,
            rule.Description,
            rule.Frequency,
            rule.DayOfWeek,
            rule.DayOfMonth,
            rule.Month,
            rule.StartOn,
            rule.EndOn,
            rule.NextOccurrenceOn,
            rule.IsActive,
            rule.CreatedAt,
            rule.UpdatedAt);

    public static string FormatFrequencySummary(RecurringTransaction rule) =>
        rule.Frequency switch
        {
            RecurrenceFrequency.Weekly => $"Weekly on {rule.DayOfWeek}",
            RecurrenceFrequency.Biweekly => $"Every 2 weeks from {rule.StartOn:yyyy-MM-dd}",
            RecurrenceFrequency.Monthly => $"Monthly on day {rule.DayOfMonth}",
            RecurrenceFrequency.Yearly => $"Yearly on {rule.Month}/{rule.DayOfMonth}",
            _ => rule.Frequency.ToString()
        };
}
