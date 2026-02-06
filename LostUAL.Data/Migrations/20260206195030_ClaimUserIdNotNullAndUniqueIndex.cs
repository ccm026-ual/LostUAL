using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LostUAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class ClaimUserIdNotNullAndUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Claims_PostId_Status",
                table: "Claims");

            migrationBuilder.AlterColumn<string>(
                name: "ClaimantUserId",
                table: "Claims",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Claims_PostId_ClaimantUserId",
                table: "Claims",
                columns: new[] { "PostId", "ClaimantUserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Claims_PostId_ClaimantUserId",
                table: "Claims");

            migrationBuilder.AlterColumn<string>(
                name: "ClaimantUserId",
                table: "Claims",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.CreateIndex(
                name: "IX_Claims_PostId_Status",
                table: "Claims",
                columns: new[] { "PostId", "Status" });
        }
    }
}
