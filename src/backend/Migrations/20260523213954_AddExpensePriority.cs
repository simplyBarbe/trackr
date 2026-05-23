using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddExpensePriority : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Priority",
                table: "Categories",
                type: "TEXT",
                maxLength: 16,
                nullable: false,
                defaultValue: "Discretionary");

            migrationBuilder.AddColumn<string>(
                name: "Priority",
                table: "Transactions",
                type: "TEXT",
                maxLength: 16,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE Transactions
                SET Priority = (
                    SELECT c.Priority FROM Categories c WHERE c.Id = Transactions.CategoryId
                )
                WHERE Type = 'Expense' AND CategoryId IS NOT NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Categories");
        }
    }
}
