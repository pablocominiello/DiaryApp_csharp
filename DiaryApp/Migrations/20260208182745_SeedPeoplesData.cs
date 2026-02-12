using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiaryApp.Migrations
{
    /// <inheritdoc />
    public partial class SeedPeoplesData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_People",
                table: "People");

            migrationBuilder.RenameTable(
                name: "People",
                newName: "Peoples");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Peoples",
                table: "Peoples",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Peoples",
                table: "Peoples");

            migrationBuilder.RenameTable(
                name: "Peoples",
                newName: "People");

            migrationBuilder.AddPrimaryKey(
                name: "PK_People",
                table: "People",
                column: "Id");
        }
    }
}
