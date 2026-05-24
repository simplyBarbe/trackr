namespace backend.Features.Transactions.Export;

public sealed record ExportTransactionsResponse(string CsvContent, string FileName);
