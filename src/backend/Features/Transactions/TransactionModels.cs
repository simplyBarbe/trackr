using backend.Data.Entities;
using backend.Data.Entities.Enums;
using backend.Common.Pagination;

namespace backend.Features.Transactions;

public sealed record TransactionResponse(
    Guid Id,
    TransactionType Type,
    Guid AccountId,
    string AccountName,
    Guid? ToAccountId,
    string? ToAccountName,
    Guid? CategoryId,
    string? CategoryName,
    decimal Amount,
    DateOnly OccurredOn,
    string? Description,
    DateTime CreatedAt);

public sealed record ListTransactionsResponse(
    IReadOnlyList<TransactionResponse> Items,
    int Page,
    int PageSize,
    int TotalCount)
    : PagedResponse<TransactionResponse>(Items, Page, PageSize, TotalCount);

public sealed record GetTransactionSummaryResponse(decimal TotalIncome, decimal TotalExpense);

public static class TransactionMapping
{
    public static TransactionResponse ToResponse(Transaction transaction) =>
        new(
            transaction.Id,
            transaction.Type,
            transaction.AccountId,
            transaction.Account.Name,
            transaction.ToAccountId,
            transaction.ToAccount?.Name,
            transaction.CategoryId,
            transaction.Category?.Name,
            transaction.Amount,
            transaction.OccurredOn,
            transaction.Description,
            transaction.CreatedAt);
}
