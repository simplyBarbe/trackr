using backend.Application.Rules;
using backend.Data;
using backend.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Transaction = backend.Data.Entities.Transaction;

namespace backend.Application.Services;

public sealed class RecurringTransactionGenerationService(
    AppDbContext db,
    ILogger<RecurringTransactionGenerationService> logger)
{
    public async Task<int> ProcessDueAsync(DateOnly today, CancellationToken cancellationToken = default)
    {
        var dueRules = await db.RecurringTransactions
            .Where(r => r.IsActive && r.NextOccurrenceOn <= today)
            .OrderBy(r => r.NextOccurrenceOn)
            .ToListAsync(cancellationToken);

        var created = 0;
        foreach (var rule in dueRules)
            created += await ProcessRuleAsync(rule, today, cancellationToken);

        return created;
    }

    public async Task<int> ProcessRuleByIdAsync(Guid ruleId, DateOnly today, CancellationToken cancellationToken = default)
    {
        var rule = await db.RecurringTransactions
            .FirstOrDefaultAsync(r => r.Id == ruleId, cancellationToken);

        if (rule is null)
            return 0;

        return await ProcessRuleAsync(rule, today, cancellationToken);
    }

    private async Task<int> ProcessRuleAsync(
        RecurringTransaction rule,
        DateOnly today,
        CancellationToken cancellationToken)
    {
        var created = 0;

        while (rule.IsActive
               && rule.NextOccurrenceOn <= today
               && (rule.EndOn is null || rule.NextOccurrenceOn <= rule.EndOn))
        {
            var occurrence = rule.NextOccurrenceOn;

            var alreadyExists = await db.Transactions.AnyAsync(
                t => t.RecurringTransactionId == rule.Id && t.OccurredOn == occurrence,
                cancellationToken);

            if (alreadyExists)
            {
                logger.LogInformation(
                    "Skipping recurring transaction {RecurringId} for {OccurrenceOn}: already posted",
                    rule.Id,
                    occurrence);
                rule.NextOccurrenceOn = RecurringScheduleService.Advance(rule, occurrence);
                rule.UpdatedAt = DateTime.UtcNow;
                continue;
            }

            var validation = await TransactionRules.ValidateForCreateOrUpdateAsync(
                db,
                rule.Type,
                rule.AccountId,
                rule.ToAccountId,
                rule.CategoryId,
                rule.Amount,
                cancellationToken);

            if (!validation.IsSuccess)
            {
                logger.LogWarning(
                    "Cannot post recurring transaction {RecurringId} for {OccurrenceOn}: {Reason}",
                    rule.Id,
                    occurrence,
                    validation.Error!.Message);
                break;
            }

            Category? category = null;
            if (rule.CategoryId is not null)
            {
                category = await db.Categories
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == rule.CategoryId, cancellationToken);
            }

            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                Type = rule.Type,
                AccountId = rule.AccountId,
                ToAccountId = rule.ToAccountId,
                CategoryId = rule.CategoryId,
                Priority = TransactionRules.ResolvePriority(rule.Type, rule.Priority, category),
                Amount = rule.Amount,
                OccurredOn = occurrence,
                Description = rule.Description,
                RecurringTransactionId = rule.Id,
                CreatedAt = DateTime.UtcNow
            };

            db.Transactions.Add(transaction);
            rule.NextOccurrenceOn = RecurringScheduleService.Advance(rule, occurrence);
            rule.UpdatedAt = DateTime.UtcNow;
            created++;

            logger.LogInformation(
                "Posted recurring transaction {RecurringId} as {TransactionId} for {OccurrenceOn}",
                rule.Id,
                transaction.Id,
                occurrence);
        }

        if (created > 0 || db.ChangeTracker.HasChanges())
            await db.SaveChangesAsync(cancellationToken);

        return created;
    }
}
