using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LostUAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPostCloseAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ClosedAtUtc",
                table: "Posts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClosedByUserId",
                table: "Posts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClosedReason",
                table: "Posts",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClosedAtUtc",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "ClosedByUserId",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "ClosedReason",
                table: "Posts");
        }
    }
}
