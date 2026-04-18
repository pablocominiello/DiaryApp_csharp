using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiaryApp.Core.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUniqueConstraintFromPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Payments_PeoplesId_Ano_Mes",
                table: "Payments");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PeoplesId_Ano_Mes",
                table: "Payments",
                columns: new[] { "PeoplesId", "Ano", "Mes" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Payments_PeoplesId_Ano_Mes",
                table: "Payments");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PeoplesId_Ano_Mes",
                table: "Payments",
                columns: new[] { "PeoplesId", "Ano", "Mes" },
                unique: true);
        }
    }
}
