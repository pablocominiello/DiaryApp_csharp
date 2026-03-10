using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiaryApp.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToPersons : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Peoples",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Peoples",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Peoples",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Peoples",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Peoples_AspNetUsers_UserId",
                table: "Peoples");

            migrationBuilder.DropIndex(
                name: "IX_Peoples_UserId",
                table: "Peoples");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Peoples");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Peoples",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Peoples",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250);

            migrationBuilder.InsertData(
                table: "Peoples",
                columns: new[] { "Id", "Born", "Content", "ImagenUrl", "Nombre" },
                values: new object[] { 1, new DateTime(1976, 6, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "Kili", null, "Pablo Eugenio Cominiello" });
        }
    }
}
