using backend.Data.Entities.Enums;

namespace backend.Data.Entities;

public class Account
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public AccountType Type { get; set; }

    public AccountColor Color { get; set; } = AccountColor.Primary;

    public decimal InitialBalance { get; set; }

    public bool IsArchived { get; set; }

    public DateTime CreatedAt { get; set; }

    public ICollection<Transaction> Transactions { get; set; } = [];

    public ICollection<Transaction> IncomingTransfers { get; set; } = [];
}
