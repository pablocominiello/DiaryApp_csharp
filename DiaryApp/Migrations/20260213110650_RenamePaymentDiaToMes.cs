using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiaryApp.Migrations
{
    /// <inheritdoc />
    public partial class RenamePaymentDiaToMes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Dia",
                table: "Payments",
                newName: "Mes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Mes",
                table: "Payments",
                newName: "Dia");
        }
    }
}
