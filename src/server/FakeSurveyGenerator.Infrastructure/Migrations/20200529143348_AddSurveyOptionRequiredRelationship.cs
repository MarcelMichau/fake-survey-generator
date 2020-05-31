using Microsoft.EntityFrameworkCore.Migrations;

namespace FakeSurveyGenerator.Infrastructure.Migrations
{
    public partial class AddSurveyOptionRequiredRelationship : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SurveyOption_Survey_SurveyId",
                schema: "Survey",
                table: "SurveyOption");

            migrationBuilder.AlterColumn<int>(
                name: "SurveyId",
                schema: "Survey",
                table: "SurveyOption",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SurveyOption_Survey_SurveyId",
                schema: "Survey",
                table: "SurveyOption",
                column: "SurveyId",
                principalSchema: "Survey",
                principalTable: "Survey",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SurveyOption_Survey_SurveyId",
                schema: "Survey",
                table: "SurveyOption");

            migrationBuilder.AlterColumn<int>(
                name: "SurveyId",
                schema: "Survey",
                table: "SurveyOption",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddForeignKey(
                name: "FK_SurveyOption_Survey_SurveyId",
                schema: "Survey",
                table: "SurveyOption",
                column: "SurveyId",
                principalSchema: "Survey",
                principalTable: "Survey",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
