using backend.Application.Services;
using backend.Data;
using backend.Data.Entities;
using backend.Data.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace backend.Tests;

public class RecurringTransactionGenerationServiceTests
{
    [Fact]
    public async Task ProcessDue_CreatesTransaction_AndAdvancesNextOccurrence()
    {
        await using var db = CreateDb();
        var account = new Account
        {
            Id = Guid.NewGuid(),
            Name = "Checking",
            Type = AccountType.Checking,
            Color = AccountColor.Primary,
            CreatedAt = DateTime.UtcNow
        };
        db.Accounts.Add(account);

        var rule = new RecurringTransaction
        {
            Id = Guid.NewGuid(),
            Type = TransactionType.Expense,
            AccountId = account.Id,
            Amount = 1200m,
            Description = "Rent",
            Frequency = RecurrenceFrequency.Monthly,
            DayOfMonth = 1,
            StartOn = new DateOnly(2024, 1, 1),
            NextOccurrenceOn = new DateOnly(2024, 1, 1),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.RecurringTransactions.Add(rule);
        await db.SaveChangesAsync();

        var service = new RecurringTransactionGenerationService(db, NullLogger<RecurringTransactionGenerationService>.Instance);
        var created = await service.ProcessDueAsync(new DateOnly(2024, 1, 1));

        Assert.Equal(1, created);
        Assert.Single(db.Transactions);
        Assert.Equal(new DateOnly(2024, 2, 1), rule.NextOccurrenceOn);

        var createdAgain = await service.ProcessDueAsync(new DateOnly(2024, 1, 1));
        Assert.Equal(0, createdAgain);
        Assert.Single(db.Transactions);
    }

    [Fact]
    public async Task ProcessDue_ArchivedAccount_DoesNotAdvance()
    {
        await using var db = CreateDb();
        var account = new Account
        {
            Id = Guid.NewGuid(),
            Name = "Checking",
            Type = AccountType.Checking,
            Color = AccountColor.Primary,
            IsArchived = true,
            CreatedAt = DateTime.UtcNow
        };
        db.Accounts.Add(account);

        var occurrence = new DateOnly(2024, 1, 1);
        var rule = new RecurringTransaction
        {
            Id = Guid.NewGuid(),
            Type = TransactionType.Expense,
            AccountId = account.Id,
            Amount = 50m,
            Frequency = RecurrenceFrequency.Monthly,
            DayOfMonth = 1,
            StartOn = occurrence,
            NextOccurrenceOn = occurrence,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.RecurringTransactions.Add(rule);
        await db.SaveChangesAsync();

        var service = new RecurringTransactionGenerationService(db, NullLogger<RecurringTransactionGenerationService>.Instance);
        var created = await service.ProcessDueAsync(occurrence);

        Assert.Equal(0, created);
        Assert.Empty(db.Transactions);
        Assert.Equal(occurrence, rule.NextOccurrenceOn);
    }

    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        var db = new AppDbContext(options);
        db.Database.OpenConnection();
        db.Database.EnsureCreated();
        return db;
    }
}
