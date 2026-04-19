using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AUCAPulse.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseInfoToRoom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "course_info",
                table: "rooms",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "rooms",
                keyColumn: "id",
                keyValue: 1,
                column: "course_info",
                value: null);

            migrationBuilder.UpdateData(
                table: "rooms",
                keyColumn: "id",
                keyValue: 2,
                column: "course_info",
                value: null);

            migrationBuilder.UpdateData(
                table: "rooms",
                keyColumn: "id",
                keyValue: 3,
                column: "course_info",
                value: null);

            migrationBuilder.UpdateData(
                table: "rooms",
                keyColumn: "id",
                keyValue: 4,
                column: "course_info",
                value: null);

            migrationBuilder.UpdateData(
                table: "rooms",
                keyColumn: "id",
                keyValue: 5,
                column: "course_info",
                value: null);

            migrationBuilder.UpdateData(
                table: "rooms",
                keyColumn: "id",
                keyValue: 6,
                column: "course_info",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "course_info",
                table: "rooms");
        }
    }
}
