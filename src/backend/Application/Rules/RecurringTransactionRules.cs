using backend.Common.Results;
using backend.Data.Entities.Enums;

namespace backend.Application.Rules;

public static class RecurringTransactionRules
{
    public static Result ValidateSchedule(
        RecurrenceFrequency frequency,
        DayOfWeek? dayOfWeek,
        int? dayOfMonth,
        int? month)
    {
        switch (frequency)
        {
            case RecurrenceFrequency.Weekly:
            case RecurrenceFrequency.Biweekly:
                if (dayOfWeek is null)
                    return Result.Failure(Error.Validation("DayOfWeek is required for weekly and biweekly schedules."));
                break;

            case RecurrenceFrequency.Monthly:
                if (dayOfMonth is null or < 1 or > 31)
                    return Result.Failure(Error.Validation("DayOfMonth must be between 1 and 31 for monthly schedules."));
                break;

            case RecurrenceFrequency.Yearly:
                if (month is null or < 1 or > 12)
                    return Result.Failure(Error.Validation("Month must be between 1 and 12 for yearly schedules."));
                if (dayOfMonth is null or < 1 or > 31)
                    return Result.Failure(Error.Validation("DayOfMonth must be between 1 and 31 for yearly schedules."));
                break;

            default:
                return Result.Failure(Error.Validation("Invalid recurrence frequency."));
        }

        return Result.Success();
    }
}
