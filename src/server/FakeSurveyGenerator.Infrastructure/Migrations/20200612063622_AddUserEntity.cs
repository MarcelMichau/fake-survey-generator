using Microsoft.EntityFrameworkCore.Migrations;

namespace FakeSurveyGenerator.Infrastructure.Migrations
{
    public partial class AddUserEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "UserSeq",
                schema: "Survey",
                incrementBy: 10);

            migrationBuilder.AddColumn<int>(
                name: "OwnerId",
                schema: "Survey",
                table: "Survey",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "User",
                schema: "Survey",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    DisplayName = table.Column<string>(maxLength: 250, nullable: false),
                    EmailAddress = table.Column<string>(maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Survey_OwnerId",
                schema: "Survey",
                table: "Survey",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Survey_User_OwnerId",
                schema: "Survey",
                table: "Survey",
                column: "OwnerId",
                principalSchema: "Survey",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Survey_User_OwnerId",
                schema: "Survey",
                table: "Survey");

            migrationBuilder.DropTable(
                name: "User",
                schema: "Survey");

            migrationBuilder.DropIndex(
                name: "IX_Survey_OwnerId",
                schema: "Survey",
                table: "Survey");

            migrationBuilder.DropSequence(
                name: "UserSeq",
                schema: "Survey");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                schema: "Survey",
                table: "Survey");
        }
    }
}
