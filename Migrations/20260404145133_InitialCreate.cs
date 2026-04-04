using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AUCAPulse.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "locations",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    parent_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_locations", x => x.id);
                    table.ForeignKey(
                        name: "FK_locations_locations_parent_id",
                        column: x => x.parent_id,
                        principalTable: "locations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    rolename = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "semesters",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_current = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_semesters", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    identification_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    phone_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    department = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    otp_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    otp_expiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    role_id = table.Column<int>(type: "integer", nullable: false),
                    location_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                    table.ForeignKey(
                        name: "FK_users_locations_location_id",
                        column: x => x.location_id,
                        principalTable: "locations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_users_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "lecture_schedules",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    lecturer_id = table.Column<int>(type: "integer", nullable: false),
                    day_of_week = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    start_time = table.Column<TimeSpan>(type: "interval", nullable: false),
                    end_time = table.Column<TimeSpan>(type: "interval", nullable: false),
                    course_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    course_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    room_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    semester_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lecture_schedules", x => x.id);
                    table.ForeignKey(
                        name: "FK_lecture_schedules_semesters_semester_id",
                        column: x => x.semester_id,
                        principalTable: "semesters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_lecture_schedules_users_lecturer_id",
                        column: x => x.lecturer_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lecturer_statuses",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    lecturer_id = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    location = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    available_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    available_until = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lecturer_statuses", x => x.id);
                    table.ForeignKey(
                        name: "FK_lecturer_statuses_users_lecturer_id",
                        column: x => x.lecturer_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    message = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    is_read = table.Column<bool>(type: "boolean", nullable: false),
                    read_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.id);
                    table.ForeignKey(
                        name: "FK_notifications_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "offices",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    office_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    office_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    building = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    floor = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    department = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    phone_extension = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    availability_status = table.Column<string>(type: "text", nullable: false),
                    regular_open_time = table.Column<TimeSpan>(type: "interval", nullable: true),
                    regular_close_time = table.Column<TimeSpan>(type: "interval", nullable: true),
                    status_updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    staff_user_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_offices", x => x.id);
                    table.ForeignKey(
                        name: "FK_offices_users_staff_user_id",
                        column: x => x.staff_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "password_reset_requests",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    token = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    expiry_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    reviewed_by = table.Column<int>(type: "integer", nullable: true),
                    reviewed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    rejection_reason = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_password_reset_requests", x => x.id);
                    table.ForeignKey(
                        name: "FK_password_reset_requests_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "rooms",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    room_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    room_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    capacity = table.Column<int>(type: "integer", nullable: true),
                    building = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    floor = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    room_type = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    current_lecturer_id = table.Column<int>(type: "integer", nullable: true),
                    occupied_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    occupied_until = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rooms", x => x.id);
                    table.ForeignKey(
                        name: "FK_rooms_users_current_lecturer_id",
                        column: x => x.current_lecturer_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "verification_requests",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    submitted_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    request_type = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    reviewed_by = table.Column<int>(type: "integer", nullable: true),
                    reviewed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    rejection_reason = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_verification_requests", x => x.id);
                    table.ForeignKey(
                        name: "FK_verification_requests_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "locations",
                columns: new[] { "id", "code", "created_at", "name", "parent_id", "type" },
                values: new object[,]
                {
                    { 1, "KGL", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Kigali", null, "PROVINCE" },
                    { 2, "EST", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Eastern Province", null, "PROVINCE" },
                    { 3, "NTH", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Northern Province", null, "PROVINCE" },
                    { 4, "STH", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Southern Province", null, "PROVINCE" },
                    { 5, "WST", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Western Province", null, "PROVINCE" }
                });

            migrationBuilder.InsertData(
                table: "offices",
                columns: new[] { "id", "availability_status", "building", "created_at", "department", "floor", "office_name", "office_number", "phone_extension", "regular_close_time", "regular_open_time", "staff_user_id", "status_updated_at" },
                values: new object[,]
                {
                    { 1, "CLOSED", "Admin Building", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "1st Floor", "Registrar Office", "Admin-101", null, new TimeSpan(0, 17, 0, 0, 0), new TimeSpan(0, 8, 0, 0, 0), null, null },
                    { 2, "CLOSED", "Admin Building", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "1st Floor", "Finance Office", "Admin-102", null, new TimeSpan(0, 17, 0, 0, 0), new TimeSpan(0, 8, 0, 0, 0), null, null },
                    { 3, "CLOSED", "Admin Building", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "2nd Floor", "Student Affairs", "Admin-201", null, new TimeSpan(0, 17, 0, 0, 0), new TimeSpan(0, 8, 0, 0, 0), null, null },
                    { 4, "CLOSED", "Admin Building", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "2nd Floor", "Academic Affairs", "Admin-202", null, new TimeSpan(0, 17, 0, 0, 0), new TimeSpan(0, 8, 0, 0, 0), null, null }
                });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "id", "description", "rolename" },
                values: new object[,]
                {
                    { 1, "Student user with search and view permissions", "STUDENT" },
                    { 2, "Lecturer with room occupation and status update permissions", "LECTURER" },
                    { 3, "Staff member with office management permissions", "STAFF" },
                    { 4, "Administrator with full system access", "ADMIN" }
                });

            migrationBuilder.InsertData(
                table: "rooms",
                columns: new[] { "id", "building", "capacity", "created_at", "current_lecturer_id", "floor", "occupied_at", "occupied_until", "room_name", "room_number", "room_type", "status", "updated_at" },
                values: new object[,]
                {
                    { 1, "Academic Block A", 100, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "1st Floor", null, null, "Lecture Hall 1", "A-101", "LECTURE_HALL", "AVAILABLE", null },
                    { 2, "Academic Block A", 100, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "1st Floor", null, null, "Lecture Hall 2", "A-102", "LECTURE_HALL", "AVAILABLE", null },
                    { 3, "Academic Block A", 40, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "2nd Floor", null, null, "Computer Lab 1", "A-204", "LAB", "AVAILABLE", null },
                    { 4, "Academic Block A", 40, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "2nd Floor", null, null, "Computer Lab 2", "A-205", "LAB", "AVAILABLE", null },
                    { 5, "Academic Block B", 30, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "1st Floor", null, null, "Science Lab", "B-101", "LAB", "AVAILABLE", null },
                    { 6, "Academic Block B", 50, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "2nd Floor", null, null, "Conference Room", "B-201", "MEETING_ROOM", "AVAILABLE", null }
                });

            migrationBuilder.InsertData(
                table: "semesters",
                columns: new[] { "id", "created_at", "end_date", "is_current", "name", "start_date" },
                values: new object[] { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 31, 0, 0, 0, 0, DateTimeKind.Utc), true, "Fall 2024/2025", new DateTime(2024, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.InsertData(
                table: "locations",
                columns: new[] { "id", "code", "created_at", "name", "parent_id", "type" },
                values: new object[,]
                {
                    { 6, "GSB", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Gasabo", 1, "DISTRICT" },
                    { 7, "KCK", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Kicukiro", 1, "DISTRICT" },
                    { 8, "NYR", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Nyarugenge", 1, "DISTRICT" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_lecture_schedules_lecturer_id",
                table: "lecture_schedules",
                column: "lecturer_id");

            migrationBuilder.CreateIndex(
                name: "IX_lecture_schedules_semester_id",
                table: "lecture_schedules",
                column: "semester_id");

            migrationBuilder.CreateIndex(
                name: "IX_lecturer_statuses_lecturer_id",
                table: "lecturer_statuses",
                column: "lecturer_id");

            migrationBuilder.CreateIndex(
                name: "IX_locations_code",
                table: "locations",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_locations_parent_id",
                table: "locations",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_user_id",
                table: "notifications",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_offices_office_number",
                table: "offices",
                column: "office_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_offices_staff_user_id",
                table: "offices",
                column: "staff_user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_password_reset_requests_token",
                table: "password_reset_requests",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_password_reset_requests_user_id",
                table: "password_reset_requests",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_roles_rolename",
                table: "roles",
                column: "rolename",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rooms_current_lecturer_id",
                table: "rooms",
                column: "current_lecturer_id");

            migrationBuilder.CreateIndex(
                name: "IX_rooms_room_number",
                table: "rooms",
                column: "room_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_identification_number",
                table: "users",
                column: "identification_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_location_id",
                table: "users",
                column: "location_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_role_id",
                table: "users",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_verification_requests_user_id",
                table: "verification_requests",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "lecture_schedules");

            migrationBuilder.DropTable(
                name: "lecturer_statuses");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "offices");

            migrationBuilder.DropTable(
                name: "password_reset_requests");

            migrationBuilder.DropTable(
                name: "rooms");

            migrationBuilder.DropTable(
                name: "verification_requests");

            migrationBuilder.DropTable(
                name: "semesters");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "locations");

            migrationBuilder.DropTable(
                name: "roles");
        }
    }
}
