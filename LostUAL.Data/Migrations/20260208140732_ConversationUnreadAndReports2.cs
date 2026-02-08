using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LostUAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class ConversationUnreadAndReports2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ClaimantLastReadAtUtc",
                table: "Conversations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastMessageAtUtc",
                table: "Conversations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastMessageByUserId",
                table: "Conversations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OwnerLastReadAtUtc",
                table: "Conversations",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClaimantLastReadAtUtc",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "LastMessageAtUtc",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "LastMessageByUserId",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "OwnerLastReadAtUtc",
                table: "Conversations");
        }
    }
}
