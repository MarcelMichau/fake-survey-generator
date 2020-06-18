using Microsoft.EntityFrameworkCore.Migrations;

namespace FakeSurveyGenerator.Infrastructure.Migrations
{
    public partial class AddUniqueConstraintToExternalUserIdIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_User_ExternalUserId",
                schema: "Survey",
                table: "User");

            migrationBuilder.CreateIndex(
                name: "IX_User_ExternalUserId",
                schema: "Survey",
                table: "User",
                column: "ExternalUserId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_User_ExternalUserId",
                schema: "Survey",
                table: "User");

            migrationBuilder.CreateIndex(
                name: "IX_User_ExternalUserId",
                schema: "Survey",
                table: "User",
                column: "ExternalUserId");
        }
    }
}
