using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LostUAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddClaimConfirmationsAndAutoResolve : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutoResolveAtUtc",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "ClaimantConfirmedResolvedAtUtc",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "CreatorConfirmedResolvedAtUtc",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "IsAutoResolvePaused",
                table: "Posts");

            migrationBuilder.AddColumn<DateTime>(
                name: "AcceptedAtUtc",
                table: "Claims",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AutoResolveAtUtc",
                table: "Claims",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ClaimantConfirmedAtUtc",
                table: "Claims",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OwnerConfirmedAtUtc",
                table: "Claims",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AcceptedAtUtc",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "AutoResolveAtUtc",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "ClaimantConfirmedAtUtc",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "OwnerConfirmedAtUtc",
                table: "Claims");

            migrationBuilder.AddColumn<DateTime>(
                name: "AutoResolveAtUtc",
                table: "Posts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ClaimantConfirmedResolvedAtUtc",
                table: "Posts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatorConfirmedResolvedAtUtc",
                table: "Posts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAutoResolvePaused",
                table: "Posts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
