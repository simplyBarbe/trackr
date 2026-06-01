using backend.Common.Results;
using backend.Data;
using backend.Data.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;

namespace backend.Features.Transactions.Export;

public sealed class ExportTransactionsHandler(AppDbContext db)
{
    public async Task<Result<ExportTransactionsResponse>> HandleAsync(
        ExportTransactionsRequest request,
        CancellationToken cancellationToken)
    {
        var transactions = await db.Transactions
            .AsNoTracking()
            .Include(t => t.Account)
            .Include(t => t.ToAccount)
            .Include(t => t.Category)
            .ApplyListFilters(
                request.AccountId,
                request.CategoryId,
                request.Type,
                request.Priority,
                request.From,
                request.To)
            .OrderByDescending(t => t.OccurredOn)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

        var csv = BuildCsv(transactions);
        var fileName = BuildFileName(request.From, request.To);

        return Result<ExportTransactionsResponse>.Success(
            new ExportTransactionsResponse(csv, fileName));
    }

    private static string BuildCsv(IReadOnlyList<Data.Entities.Transaction> transactions)
    {
        var sb = new StringBuilder();
        sb.Append('\uFEFF');
        sb.AppendLine("Date,Type,Account,Category,Priority,Amount,Description,Created");

        foreach (var transaction in transactions)
        {
            var row = new[]
            {
                transaction.OccurredOn.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                transaction.Type.ToString(),
                FormatAccount(transaction),
                transaction.Category?.Name ?? "",
                FormatPriority(transaction),
                transaction.Amount.ToString(CultureInfo.InvariantCulture),
                transaction.Description ?? "",
                transaction.CreatedAt.ToString("o", CultureInfo.InvariantCulture)
            };

            sb.AppendLine(string.Join(",", row.Select(EscapeCsvField)));
        }

        return sb.ToString();
    }

    private static string FormatAccount(Data.Entities.Transaction transaction)
    {
        if (transaction.Type == TransactionType.Transfer
            && transaction.ToAccount is not null)
        {
            return $"{transaction.Account.Name} → {transaction.ToAccount.Name}";
        }

        return transaction.Account.Name;
    }

    private static string FormatPriority(Data.Entities.Transaction transaction) =>
        transaction.Type == TransactionType.Expense && transaction.Priority is not null
            ? transaction.Priority.Value.ToString()
            : "";

    private static string EscapeCsvField(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "";

        if (value.Contains('"') || value.Contains(',') || value.Contains('\n') || value.Contains('\r'))
            return $"\"{value.Replace("\"", "\"\"")}\"";

        return value;
    }

    private static string BuildFileName(DateOnly? from, DateOnly? to)
    {
        var stamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);

        if (from is not null && to is not null)
            return $"transactions-{from:yyyy-MM-dd}-to-{to:yyyy-MM-dd}-{stamp}.csv";

        if (from is not null)
            return $"transactions-from-{from:yyyy-MM-dd}-{stamp}.csv";

        if (to is not null)
            return $"transactions-to-{to:yyyy-MM-dd}-{stamp}.csv";

        return $"transactions-{stamp}.csv";
    }
}
