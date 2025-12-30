using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DiaryApp.Migrations
{
    /// <inheritdoc />
    public partial class AddedSeedDataDiaryEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "DiaryEntries",
                columns: new[] { "Id", "Content", "DateCreated", "Title" },
                values: new object[,]
                {
                    { 1, "learning .net mvc with Punjha", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Learning .Net MVC" },
                    { 2, "Learning Migrations mvc with Punjha", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Learning Migrations" },
                    { 3, "Learning Input database with Punjha", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Input database" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DiaryEntries",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "DiaryEntries",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "DiaryEntries",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}
