using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiaryApp.Core.Migrations
{
    /// <inheritdoc />
    public partial class MakeUserIdOptionalInPerson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Peoples_AspNetUsers_UserId",
                table: "Peoples");

            migrationBuilder.DropIndex(
                name: "IX_Peoples_UserId",
                table: "Peoples");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Peoples",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_Peoples_UserId",
                table: "Peoples",
                column: "UserId",
                unique: true,
                filter: "[UserId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Peoples_AspNetUsers_UserId",
                table: "Peoples",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Peoples_AspNetUsers_UserId",
                table: "Peoples");

            migrationBuilder.DropIndex(
                name: "IX_Peoples_UserId",
                table: "Peoples");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Peoples",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Peoples_UserId",
                table: "Peoples",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Peoples_AspNetUsers_UserId",
                table: "Peoples",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
