using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GeldApp2.Database.Migrations
{
    public partial class ImportedExpense : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ImportedExpenses",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    BookingDay = table.Column<DateTime>(nullable: false),
                    Valuta = table.Column<DateTime>(nullable: false),
                    Imported = table.Column<DateTimeOffset>(nullable: false),
                    Type = table.Column<string>(nullable: true),
                    Partner = table.Column<string>(nullable: true),
                    AccountNumber = table.Column<string>(nullable: true),
                    BankingCode = table.Column<string>(nullable: true),
                    Amount = table.Column<decimal>(nullable: false),
                    DebteeId = table.Column<string>(nullable: true),
                    Detail = table.Column<string>(nullable: true),
                    Reference1 = table.Column<string>(nullable: true),
                    Reference2 = table.Column<string>(nullable: true),
                    IsHandled = table.Column<bool>(nullable: false),
                    ExpenseId = table.Column<long>(nullable: true),
                    AccountId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportedExpenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImportedExpenses_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ImportedExpenses_Expenses_ExpenseId",
                        column: x => x.ExpenseId,
                        principalTable: "Expenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ImportedExpenses_AccountId",
                table: "ImportedExpenses",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportedExpenses_ExpenseId",
                table: "ImportedExpenses",
                column: "ExpenseId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImportedExpenses");
        }
    }
}
