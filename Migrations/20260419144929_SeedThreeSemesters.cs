using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AUCAPulse.Migrations
{
    /// <inheritdoc />
    public partial class SeedThreeSemesters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "semesters",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "end_date", "is_current", "name", "start_date" },
                values: new object[] { new DateTime(2025, 12, 20, 0, 0, 0, 0, DateTimeKind.Utc), false, "Semester 1, 2025/2026", new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.InsertData(
                table: "semesters",
                columns: new[] { "id", "created_at", "end_date", "is_current", "name", "start_date" },
                values: new object[,]
                {
                    { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 4, 30, 0, 0, 0, 0, DateTimeKind.Utc), true, "Semester 2, 2025/2026", new DateTime(2026, 1, 12, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 8, 21, 0, 0, 0, 0, DateTimeKind.Utc), false, "Semester 3, 2025/2026", new DateTime(2026, 5, 18, 0, 0, 0, 0, DateTimeKind.Utc) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "semesters",
                keyColumn: "id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "semesters",
                keyColumn: "id",
                keyValue: 3);

            migrationBuilder.UpdateData(
                table: "semesters",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "end_date", "is_current", "name", "start_date" },
                values: new object[] { new DateTime(2025, 1, 31, 0, 0, 0, 0, DateTimeKind.Utc), true, "Fall 2024/2025", new DateTime(2024, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc) });
        }
    }
}
