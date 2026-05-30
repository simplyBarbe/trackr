using backend.Application.Rules;
using backend.Application.Services;
using backend.Common.Results;
using backend.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Features.RecurringTransactions.Update;

public sealed class UpdateRecurringTransactionHandler(AppDbContext db)
{
    public async Task<Result<RecurringTransactionResponse>> HandleAsync(
        UpdateRecurringTransactionRequest request,
        CancellationToken cancellationToken)
    {
        var rule = await db.RecurringTransactions
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (rule is null)
            return Result<RecurringTransactionResponse>.Failure(Error.NotFound("Recurring transaction not found."));

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

        var scheduleChanged = RecurringScheduleService.ScheduleFieldsChanged(
            rule,
            request.Frequency,
            request.DayOfWeek,
            request.DayOfMonth,
            request.Month,
            request.StartOn);

        rule.Type = request.Type;
        rule.AccountId = request.AccountId;
        rule.ToAccountId = request.ToAccountId;
        rule.CategoryId = request.CategoryId;
        rule.Priority = request.Priority;
        rule.Amount = request.Amount;
        rule.Description = request.Description;
        rule.Frequency = request.Frequency;
        rule.DayOfWeek = request.DayOfWeek;
        rule.DayOfMonth = request.DayOfMonth;
        rule.Month = request.Month;
        rule.StartOn = request.StartOn;
        rule.EndOn = request.EndOn;
        rule.IsActive = request.IsActive;
        rule.UpdatedAt = DateTime.UtcNow;

        if (scheduleChanged)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            rule.NextOccurrenceOn = RecurringScheduleService.ComputeInitialNextOccurrence(rule, today);
        }

        await db.SaveChangesAsync(cancellationToken);

        await db.Entry(rule).Reference(r => r.Account).LoadAsync(cancellationToken);
        if (rule.ToAccountId is not null)
            await db.Entry(rule).Reference(r => r.ToAccount).LoadAsync(cancellationToken);
        if (rule.CategoryId is not null)
            await db.Entry(rule).Reference(r => r.Category).LoadAsync(cancellationToken);

        return Result<RecurringTransactionResponse>.Success(RecurringTransactionMapping.ToResponse(rule));
    }
}
