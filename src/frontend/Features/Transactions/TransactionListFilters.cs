using Microsoft.Kiota.Abstractions;
using Trackr.Api.Api.Transactions;
using Trackr.Api.Api.Transactions.Export;
using Trackr.Api.Api.Transactions.Summary;

namespace frontend.Features.Transactions;

public sealed record TransactionListFilters(
    string AccountId = "",
    string CategoryId = "",
    string Type = "",
    string Priority = "",
    Date? From = null,
    Date? To = null)
{
    public void ApplyTo(TransactionsRequestBuilder.TransactionsRequestBuilderGetQueryParameters query) =>
        ApplyCore(
            v => query.AccountId = v,
            v => query.CategoryId = v,
            v => query.Type = v,
            v => query.Priority = v,
            v => query.From = v,
            v => query.To = v);

    public void ApplyTo(SummaryRequestBuilder.SummaryRequestBuilderGetQueryParameters query) =>
        ApplyCore(
            v => query.AccountId = v,
            v => query.CategoryId = v,
            v => query.Type = v,
            v => query.Priority = v,
            v => query.From = v,
            v => query.To = v);

    public void ApplyTo(ExportRequestBuilder.ExportRequestBuilderGetQueryParameters query) =>
        ApplyCore(
            v => query.AccountId = v,
            v => query.CategoryId = v,
            v => query.Type = v,
            v => query.Priority = v,
            v => query.From = v,
            v => query.To = v);

    private void ApplyCore(
        Action<string?> setAccountId,
        Action<string?> setCategoryId,
        Action<string?> setType,
        Action<string?> setPriority,
        Action<Date?> setFrom,
        Action<Date?> setTo)
    {
        if (!string.IsNullOrEmpty(AccountId))
            setAccountId(AccountId);

        if (!string.IsNullOrEmpty(CategoryId))
            setCategoryId(CategoryId);

        if (!string.IsNullOrEmpty(Type))
            setType(Type);

        if (!string.IsNullOrEmpty(Priority))
            setPriority(Priority);

        if (From is not null)
            setFrom(From);

        if (To is not null)
            setTo(To);
    }
}
