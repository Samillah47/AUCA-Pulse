using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AUCAPulse.Migrations
{
    /// <inheritdoc />
    public partial class AddClassCancellationAndNotificationLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // notifications.link — where to navigate when the notification is clicked
            migrationBuilder.AddColumn<string>(
                name: "link",
                table: "notifications",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            // lecture_schedules.cancelled_on — the date today's occurrence of
            // this weekly slot was cancelled by the lecturer. NULL = not cancelled.
            migrationBuilder.AddColumn<DateTime>(
                name: "cancelled_on",
                table: "lecture_schedules",
                type: "timestamp with time zone",
                nullable: true);

            // lecture_schedules.cancellation_reason — optional note from the lecturer
            migrationBuilder.AddColumn<string>(
                name: "cancellation_reason",
                table: "lecture_schedules",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "link",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "cancelled_on",
                table: "lecture_schedules");

            migrationBuilder.DropColumn(
                name: "cancellation_reason",
                table: "lecture_schedules");
        }
    }
}
