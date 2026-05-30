using backend.Application.Services;
using backend.Common.Results;
using backend.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Features.RecurringTransactions.GenerateNow;

public sealed class GenerateRecurringTransactionHandler(
    AppDbContext db,
    RecurringTransactionGenerationService generationService)
{
    public async Task<Result<GenerateRecurringTransactionResponse>> HandleAsync(
        GenerateRecurringTransactionRequest request,
        CancellationToken cancellationToken)
    {
        var exists = await db.RecurringTransactions
            .AnyAsync(r => r.Id == request.Id, cancellationToken);

        if (!exists)
            return Result<GenerateRecurringTransactionResponse>.Failure(Error.NotFound("Recurring transaction not found."));

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var created = await generationService.ProcessRuleByIdAsync(request.Id, today, cancellationToken);

        return Result<GenerateRecurringTransactionResponse>.Success(
            new GenerateRecurringTransactionResponse(created));
    }
}
