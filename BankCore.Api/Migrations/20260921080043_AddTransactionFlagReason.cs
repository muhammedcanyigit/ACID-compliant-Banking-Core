using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankCore.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionFlagReason : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FlagReason",
                table: "Transactions",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FlagReason",
                table: "Transactions");
        }
    }
}
