using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LostUAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPostCreatedByUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "Posts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Posts_CreatedByUserId",
                table: "Posts",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_AspNetUsers_CreatedByUserId",
                table: "Posts",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Posts_AspNetUsers_CreatedByUserId",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Posts_CreatedByUserId",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Posts");
        }
    }
}
