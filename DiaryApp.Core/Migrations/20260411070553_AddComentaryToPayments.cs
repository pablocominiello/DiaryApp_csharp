using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiaryApp.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddComentaryToPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Comentary",
                table: "Payments",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Comentary",
                table: "Payments");
        }
    }
}
