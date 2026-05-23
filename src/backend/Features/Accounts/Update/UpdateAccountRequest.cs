using backend.Data.Entities.Enums;

namespace backend.Features.Accounts.Update;

public sealed class UpdateAccountRequest
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public AccountType Type { get; set; }

    public AccountColor Color { get; set; } = AccountColor.Primary;

    public string Currency { get; set; } = "EUR";

    public decimal InitialBalance { get; set; }
}
