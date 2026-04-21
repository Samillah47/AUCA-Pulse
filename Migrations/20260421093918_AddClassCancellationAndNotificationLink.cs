using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AUCAPulse.Migrations
{
    /// <inheritdoc />
    public partial class AddClassCancellationAndNotificationLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_course_assignments_course_id",
                table: "course_assignments");

            migrationBuilder.DropIndex(
                name: "IX_course_assignments_lecturer_id_course_id_semester_id",
                table: "course_assignments");

            migrationBuilder.AddColumn<string>(
                name: "course_info",
                table: "rooms",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "link",
                table: "notifications",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cancellation_reason",
                table: "lecture_schedules",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "cancelled_on",
                table: "lecture_schedules",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "group_name",
                table: "lecture_schedules",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "group_id",
                table: "course_assignments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "chat_messages",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sender_id = table.Column<int>(type: "integer", nullable: false),
                    receiver_id = table.Column<int>(type: "integer", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    is_read = table.Column<bool>(type: "boolean", nullable: false),
                    read_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chat_messages", x => x.id);
                    table.ForeignKey(
                        name: "FK_chat_messages_users_receiver_id",
                        column: x => x.receiver_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_chat_messages_users_sender_id",
                        column: x => x.sender_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "groups",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_groups", x => x.id);
                });

            migrationBuilder.InsertData(
                table: "groups",
                columns: new[] { "id", "created_at", "description", "name" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Group A", "A" },
                    { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Group B", "B" },
                    { 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Group C", "C" },
                    { 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Group D", "D" }
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_course_assignments_course_id_semester_id_group_id",
                table: "course_assignments",
                columns: new[] { "course_id", "semester_id", "group_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_course_assignments_group_id",
                table: "course_assignments",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "IX_course_assignments_lecturer_id",
                table: "course_assignments",
                column: "lecturer_id");

            migrationBuilder.CreateIndex(
                name: "IX_chat_messages_receiver_id",
                table: "chat_messages",
                column: "receiver_id");

            migrationBuilder.CreateIndex(
                name: "IX_chat_messages_sender_id",
                table: "chat_messages",
                column: "sender_id");

            migrationBuilder.CreateIndex(
                name: "IX_groups_name",
                table: "groups",
                column: "name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_course_assignments_groups_group_id",
                table: "course_assignments",
                column: "group_id",
                principalTable: "groups",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_course_assignments_groups_group_id",
                table: "course_assignments");

            migrationBuilder.DropTable(
                name: "chat_messages");

            migrationBuilder.DropTable(
                name: "groups");

            migrationBuilder.DropIndex(
                name: "IX_course_assignments_course_id_semester_id_group_id",
                table: "course_assignments");

            migrationBuilder.DropIndex(
                name: "IX_course_assignments_group_id",
                table: "course_assignments");

            migrationBuilder.DropIndex(
                name: "IX_course_assignments_lecturer_id",
                table: "course_assignments");

            migrationBuilder.DeleteData(
                table: "semesters",
                keyColumn: "id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "semesters",
                keyColumn: "id",
                keyValue: 3);

            migrationBuilder.DropColumn(
                name: "course_info",
                table: "rooms");

            migrationBuilder.DropColumn(
                name: "link",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "cancellation_reason",
                table: "lecture_schedules");

            migrationBuilder.DropColumn(
                name: "cancelled_on",
                table: "lecture_schedules");

            migrationBuilder.DropColumn(
                name: "group_name",
                table: "lecture_schedules");

            migrationBuilder.DropColumn(
                name: "group_id",
                table: "course_assignments");

            migrationBuilder.UpdateData(
                table: "semesters",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "end_date", "is_current", "name", "start_date" },
                values: new object[] { new DateTime(2025, 1, 31, 0, 0, 0, 0, DateTimeKind.Utc), true, "Fall 2024/2025", new DateTime(2024, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.CreateIndex(
                name: "IX_course_assignments_course_id",
                table: "course_assignments",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "IX_course_assignments_lecturer_id_course_id_semester_id",
                table: "course_assignments",
                columns: new[] { "lecturer_id", "course_id", "semester_id" },
                unique: true);
        }
    }
}
