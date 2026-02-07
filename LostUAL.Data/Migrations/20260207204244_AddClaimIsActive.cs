using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LostUAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddClaimIsActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Claims_PostId_ClaimantUserId",
                table: "Claims");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Claims",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
				
			migrationBuilder.Sql(@"
			UPDATE Claims
			SET IsActive = 0
			WHERE Status IN (3,4,5,6);
			");

            migrationBuilder.CreateIndex(
                name: "IX_Claims_PostId_ClaimantUserId",
                table: "Claims",
                columns: new[] { "PostId", "ClaimantUserId" },
                unique: true,
                filter: "\"IsActive\" = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Claims_PostId_ClaimantUserId",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Claims");

            migrationBuilder.CreateIndex(
                name: "IX_Claims_PostId_ClaimantUserId",
                table: "Claims",
                columns: new[] { "PostId", "ClaimantUserId" },
                unique: true);
        }
    }
}
