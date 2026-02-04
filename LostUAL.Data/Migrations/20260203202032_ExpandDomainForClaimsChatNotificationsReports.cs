using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LostUAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class ExpandDomainForClaimsChatNotificationsReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.AddColumn<DateTime>(
                name: "OnClaimAtUtc",
                table: "Posts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhotoPath",
                table: "Posts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WinningClaimId",
                table: "Posts",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Claims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PostId = table.Column<int>(type: "INTEGER", nullable: false),
                    ClaimantUserId = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ResolvedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Claims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Claims_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: true),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    PostId = table.Column<int>(type: "INTEGER", nullable: true),
                    ClaimId = table.Column<int>(type: "INTEGER", nullable: true),
                    ConversationId = table.Column<int>(type: "INTEGER", nullable: true),
                    Text = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ReadAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PostId = table.Column<int>(type: "INTEGER", nullable: false),
                    ReporterUserId = table.Column<string>(type: "TEXT", nullable: true),
                    ReportedUserId = table.Column<string>(type: "TEXT", nullable: true),
                    Reason = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reports_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Conversations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClaimId = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conversations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Conversations_Claims_ClaimId",
                        column: x => x.ClaimId,
                        principalTable: "Claims",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ConversationId = table.Column<int>(type: "INTEGER", nullable: false),
                    SenderUserId = table.Column<string>(type: "TEXT", nullable: true),
                    Body = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Messages_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Posts_Status_CreatedAtUtc",
                table: "Posts",
                columns: new[] { "Status", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Posts_WinningClaimId",
                table: "Posts",
                column: "WinningClaimId");

            migrationBuilder.CreateIndex(
                name: "IX_Claims_PostId_Status",
                table: "Claims",
                columns: new[] { "PostId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_ClaimId",
                table: "Conversations",
                column: "ClaimId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ConversationId_CreatedAtUtc",
                table: "Messages",
                columns: new[] { "ConversationId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_ReadAtUtc",
                table: "Notifications",
                columns: new[] { "UserId", "ReadAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Reports_PostId_CreatedAtUtc",
                table: "Reports",
                columns: new[] { "PostId", "CreatedAtUtc" });

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Claims_WinningClaimId",
                table: "Posts",
                column: "WinningClaimId",
                principalTable: "Claims",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Claims_WinningClaimId",
                table: "Posts");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Reports");

            migrationBuilder.DropTable(
                name: "Conversations");

            migrationBuilder.DropTable(
                name: "Claims");

            migrationBuilder.DropIndex(
                name: "IX_Posts_Status_CreatedAtUtc",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Posts_WinningClaimId",
                table: "Posts");

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

            migrationBuilder.DropColumn(
                name: "OnClaimAtUtc",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "PhotoPath",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "WinningClaimId",
                table: "Posts");
        }
    }
}
