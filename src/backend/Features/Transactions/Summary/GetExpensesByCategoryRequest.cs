namespace backend.Features.Transactions.Summary;

public sealed class GetExpensesByCategoryRequest
{
    public DateOnly? From { get; set; }

    public DateOnly? To { get; set; }
}
