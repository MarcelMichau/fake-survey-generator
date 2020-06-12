using Microsoft.EntityFrameworkCore.Migrations;

namespace FakeSurveyGenerator.Infrastructure.Migrations
{
    public partial class AddExternalUserIdToUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalUserId",
                schema: "Survey",
                table: "User",
                maxLength: 250,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExternalUserId",
                schema: "Survey",
                table: "User");
        }
    }
}
