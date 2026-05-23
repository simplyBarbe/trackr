using backend.Data.Entities;
using backend.Data.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public sealed class DataSeeder(AppDbContext db, ILogger<DataSeeder> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await db.Accounts.AnyAsync(cancellationToken))
        {
            logger.LogDebug("Database already has data; skipping seed.");
            return;
        }

        logger.LogInformation("Seeding development data...");

        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);

        var checking = new Account
        {
            Id = Guid.NewGuid(),
            Name = "Main Checking",
            Type = AccountType.Checking,
            Color = AccountColor.Primary,
            InitialBalance = 2_500m,
            CreatedAt = now
        };

        var savings = new Account
        {
            Id = Guid.NewGuid(),
            Name = "Emergency Savings",
            Type = AccountType.Savings,
            Color = AccountColor.Success,
            InitialBalance = 15_000m,
            CreatedAt = now
        };

        var wallet = new Account
        {
            Id = Guid.NewGuid(),
            Name = "Wallet",
            Type = AccountType.Cash,
            Color = AccountColor.Warning,
            InitialBalance = 120m,
            CreatedAt = now
        };

        var creditCard = new Account
        {
            Id = Guid.NewGuid(),
            Name = "Visa Credit Card",
            Type = AccountType.CreditCard,
            Color = AccountColor.Info,
            InitialBalance = 0m,
            CreatedAt = now
        };

        db.Accounts.AddRange(checking, savings, wallet, creditCard);

        var salary = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Salary",
            Kind = CategoryKind.Income,
            SortOrder = 1
        };

        var freelance = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Freelance",
            Kind = CategoryKind.Income,
            SortOrder = 2
        };

        var housing = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Housing",
            Kind = CategoryKind.Expense,
            Priority = ExpensePriority.Essential,
            SortOrder = 1
        };

        var food = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Food",
            Kind = CategoryKind.Expense,
            Priority = ExpensePriority.Important,
            SortOrder = 2
        };

        var groceries = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Groceries",
            Kind = CategoryKind.Expense,
            Priority = ExpensePriority.Important,
            ParentId = food.Id,
            SortOrder = 1
        };

        var diningOut = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Dining Out",
            Kind = CategoryKind.Expense,
            Priority = ExpensePriority.Discretionary,
            ParentId = food.Id,
            SortOrder = 2
        };

        var transport = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Transport",
            Kind = CategoryKind.Expense,
            Priority = ExpensePriority.Important,
            SortOrder = 3
        };

        var utilities = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Utilities",
            Kind = CategoryKind.Expense,
            Priority = ExpensePriority.Essential,
            SortOrder = 4
        };

        db.Categories.AddRange(
            salary, freelance, housing, food, groceries, diningOut, transport, utilities);

        db.Transactions.AddRange(
            Income(checking.Id, salary.Id, 3_200m, today.AddMonths(-2), "January salary", now),
            Expense(checking.Id, housing.Id, housing.Priority, 950m, today.AddMonths(-2).AddDays(2), "Rent", now),
            Expense(checking.Id, groceries.Id, groceries.Priority, 142.50m, today.AddMonths(-2).AddDays(5), "Supermarket", now),
            Expense(wallet.Id, diningOut.Id, diningOut.Priority, 28m, today.AddMonths(-2).AddDays(8), "Lunch", now),
            Transfer(checking.Id, savings.Id, 500m, today.AddMonths(-2).AddDays(10), "Monthly savings", now),

            Income(checking.Id, salary.Id, 3_200m, today.AddMonths(-1), "February salary", now),
            Expense(checking.Id, housing.Id, housing.Priority, 950m, today.AddMonths(-1).AddDays(1), "Rent", now),
            Expense(checking.Id, utilities.Id, utilities.Priority, 89.20m, today.AddMonths(-1).AddDays(4), "Electricity", now),
            Expense(creditCard.Id, transport.Id, transport.Priority, 45m, today.AddMonths(-1).AddDays(6), "Transit pass", now),
            Expense(checking.Id, groceries.Id, groceries.Priority, 118.30m, today.AddMonths(-1).AddDays(12), "Supermarket", now),
            Transfer(checking.Id, creditCard.Id, 320m, today.AddMonths(-1).AddDays(15), "Card payment", now),

            Income(checking.Id, salary.Id, 3_200m, today.AddDays(-14), "March salary", now),
            Income(checking.Id, freelance.Id, 450m, today.AddDays(-10), "Side project", now),
            Expense(checking.Id, housing.Id, housing.Priority, 950m, today.AddDays(-12), "Rent", now),
            Expense(checking.Id, diningOut.Id, diningOut.Priority, 62m, today.AddDays(-7), "Dinner with friends", now),
            Expense(wallet.Id, transport.Id, transport.Priority, 12.50m, today.AddDays(-3), "Taxi", now),
            Transfer(checking.Id, savings.Id, 500m, today.AddDays(-1), "Monthly savings", now));

        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Development data seeded.");
    }

    private static Transaction Income(
        Guid accountId,
        Guid categoryId,
        decimal amount,
        DateOnly occurredOn,
        string description,
        DateTime createdAt) =>
        new()
        {
            Id = Guid.NewGuid(),
            Type = TransactionType.Income,
            AccountId = accountId,
            CategoryId = categoryId,
            Amount = amount,
            OccurredOn = occurredOn,
            Description = description,
            CreatedAt = createdAt
        };

    private static Transaction Expense(
        Guid accountId,
        Guid categoryId,
        ExpensePriority priority,
        decimal amount,
        DateOnly occurredOn,
        string description,
        DateTime createdAt) =>
        new()
        {
            Id = Guid.NewGuid(),
            Type = TransactionType.Expense,
            AccountId = accountId,
            CategoryId = categoryId,
            Priority = priority,
            Amount = amount,
            OccurredOn = occurredOn,
            Description = description,
            CreatedAt = createdAt
        };

    private static Transaction Transfer(
        Guid fromAccountId,
        Guid toAccountId,
        decimal amount,
        DateOnly occurredOn,
        string description,
        DateTime createdAt) =>
        new()
        {
            Id = Guid.NewGuid(),
            Type = TransactionType.Transfer,
            AccountId = fromAccountId,
            ToAccountId = toAccountId,
            Amount = amount,
            OccurredOn = occurredOn,
            Description = description,
            CreatedAt = createdAt
        };
}
