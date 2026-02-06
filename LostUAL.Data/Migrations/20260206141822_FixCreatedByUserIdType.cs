using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LostUAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixCreatedByUserIdType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId1",
                table: "Posts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Posts_CreatedByUserId1",
                table: "Posts",
                column: "CreatedByUserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_AspNetUsers_CreatedByUserId1",
                table: "Posts",
                column: "CreatedByUserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Posts_AspNetUsers_CreatedByUserId1",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Posts_CreatedByUserId1",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId1",
                table: "Posts");
        }
    }
}
