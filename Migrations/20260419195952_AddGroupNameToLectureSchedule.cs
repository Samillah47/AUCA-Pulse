using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AUCAPulse.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupNameToLectureSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "group_name",
                table: "lecture_schedules",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "group_name",
                table: "lecture_schedules");
        }
    }
}
