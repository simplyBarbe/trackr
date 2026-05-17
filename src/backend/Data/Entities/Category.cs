using backend.Data.Entities.Enums;

namespace backend.Data.Entities;

public class Category
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public CategoryKind Kind { get; set; }

    public Guid? ParentId { get; set; }

    public int SortOrder { get; set; }

    public bool IsArchived { get; set; }

    public Category? Parent { get; set; }

    public ICollection<Category> Children { get; set; } = [];

    public ICollection<Transaction> Transactions { get; set; } = [];
}
