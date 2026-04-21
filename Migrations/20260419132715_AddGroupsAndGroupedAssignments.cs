using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AUCAPulse.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupsAndGroupedAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Drop the old indexes on course_assignments that we're replacing.
            migrationBuilder.DropIndex(
                name: "IX_course_assignments_course_id",
                table: "course_assignments");

            migrationBuilder.DropIndex(
                name: "IX_course_assignments_lecturer_id_course_id_semester_id",
                table: "course_assignments");

            // 2. Create the groups table first so FKs and seeds can reference it.
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

            // 3. Seed the four default groups (A-D).
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

            migrationBuilder.CreateIndex(
                name: "IX_groups_name",
                table: "groups",
                column: "name",
                unique: true);

            // 4. Add group_id to course_assignments with default 1 (Group A) so any
            //    existing rows get a valid value and pass the FK + unique checks.
            //    Note: duplicate (course_id, semester_id) rows with the same default
            //    group_id would fail the new unique index below — but that's not
            //    possible because the previous unique index allowed the same
            //    (lecturer, course, semester) at most once; each row therefore has
            //    a unique (course, semester) per lecturer, and giving them all
            //    group 1 is safe as long as a course had at most one lecturer
            //    per semester before. If multiple lecturers were assigned to the
            //    same (course, semester), this backfill will fail the unique
            //    index — in that case, manually reassign groups afterwards.
            migrationBuilder.AddColumn<int>(
                name: "group_id",
                table: "course_assignments",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            // 5. Rebuild indexes: non-unique on lecturer_id, plus the new composite
            //    unique on (course_id, semester_id, group_id).
            migrationBuilder.CreateIndex(
                name: "IX_course_assignments_lecturer_id",
                table: "course_assignments",
                column: "lecturer_id");

            migrationBuilder.CreateIndex(
                name: "IX_course_assignments_group_id",
                table: "course_assignments",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "IX_course_assignments_course_id_semester_id_group_id",
                table: "course_assignments",
                columns: new[] { "course_id", "semester_id", "group_id" },
                unique: true);

            // 6. Finally, add the FK to groups.
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

            migrationBuilder.DropIndex(
                name: "IX_course_assignments_course_id_semester_id_group_id",
                table: "course_assignments");

            migrationBuilder.DropIndex(
                name: "IX_course_assignments_group_id",
                table: "course_assignments");

            migrationBuilder.DropIndex(
                name: "IX_course_assignments_lecturer_id",
                table: "course_assignments");

            migrationBuilder.DropColumn(
                name: "group_id",
                table: "course_assignments");

            migrationBuilder.DropTable(
                name: "groups");

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
