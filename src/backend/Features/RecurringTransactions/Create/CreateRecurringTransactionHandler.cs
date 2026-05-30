using backend.Application.Rules;
using backend.Application.Services;
using backend.Common.Results;
using backend.Data;
using backend.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Features.RecurringTransactions.Create;

public sealed class CreateRecurringTransactionHandler(AppDbContext db)
{
    public async Task<Result<RecurringTransactionResponse>> HandleAsync(
        CreateRecurringTransactionRequest request,
        CancellationToken cancellationToken)
    {
        var scheduleValidation = RecurringTransactionRules.ValidateSchedule(
            request.Frequency,
            request.DayOfWeek,
            request.DayOfMonth,
            request.Month);

        if (!scheduleValidation.IsSuccess)
            return Result<RecurringTransactionResponse>.Failure(scheduleValidation.Error!);

        var templateValidation = await TransactionRules.ValidateForCreateOrUpdateAsync(
            db,
            request.Type,
            request.AccountId,
            request.ToAccountId,
            request.CategoryId,
            request.Amount,
            cancellationToken);

        if (!templateValidation.IsSuccess)
            return Result<RecurringTransactionResponse>.Failure(templateValidation.Error!);

        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);
        var rule = new RecurringTransaction
        {
            Id = Guid.NewGuid(),
            Type = request.Type,
            AccountId = request.AccountId,
            ToAccountId = request.ToAccountId,
            CategoryId = request.CategoryId,
            Priority = request.Priority,
            Amount = request.Amount,
            Description = request.Description,
            Frequency = request.Frequency,
            DayOfWeek = request.DayOfWeek,
            DayOfMonth = request.DayOfMonth,
            Month = request.Month,
            StartOn = request.StartOn,
            EndOn = request.EndOn,
            NextOccurrenceOn = RecurringScheduleService.ComputeInitialNextOccurrence(
                new RecurringTransaction
                {
                    Frequency = request.Frequency,
                    DayOfWeek = request.DayOfWeek,
                    DayOfMonth = request.DayOfMonth,
                    Month = request.Month,
                    StartOn = request.StartOn
                },
                today),
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.RecurringTransactions.Add(rule);
        await db.SaveChangesAsync(cancellationToken);

        await db.Entry(rule).Reference(r => r.Account).LoadAsync(cancellationToken);
        if (rule.ToAccountId is not null)
            await db.Entry(rule).Reference(r => r.ToAccount).LoadAsync(cancellationToken);
        if (rule.CategoryId is not null)
            await db.Entry(rule).Reference(r => r.Category).LoadAsync(cancellationToken);

        return Result<RecurringTransactionResponse>.Success(RecurringTransactionMapping.ToResponse(rule));
    }
}
