using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiaryApp.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminFieldToPeoples : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Admin",
                table: "Peoples",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Admin",
                table: "Peoples");
        }
    }
}
