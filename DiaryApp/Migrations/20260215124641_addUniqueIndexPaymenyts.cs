using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiaryApp.Migrations
{
    /// <inheritdoc />
    public partial class addUniqueIndexPaymenyts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Payments_PeoplesId",
                table: "Payments");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PeoplesId_Ano_Mes",
                table: "Payments",
                columns: new[] { "PeoplesId", "Ano", "Mes" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Payments_PeoplesId_Ano_Mes",
                table: "Payments");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PeoplesId",
                table: "Payments",
                column: "PeoplesId");
        }
    }
}
