using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiaryApp.Migrations
{
    /// <inheritdoc />
    public partial class AddImagen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagenUrl",
                table: "Peoples",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Peoples",
                keyColumn: "Id",
                keyValue: 1,
                column: "ImagenUrl",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagenUrl",
                table: "Peoples");
        }
    }
}
