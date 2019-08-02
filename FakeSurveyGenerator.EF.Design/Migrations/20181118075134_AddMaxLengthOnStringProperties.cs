using Microsoft.EntityFrameworkCore.Migrations;

namespace FakeSurveyGenerator.EF.Design.Migrations
{
    public partial class AddMaxLengthOnStringProperties : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "OptionText",
                schema: "Survey",
                table: "SurveyOption",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "Topic",
                schema: "Survey",
                table: "Survey",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "RespondentType",
                schema: "Survey",
                table: "Survey",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "OptionText",
                schema: "Survey",
                table: "SurveyOption",
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 250);

            migrationBuilder.AlterColumn<string>(
                name: "Topic",
                schema: "Survey",
                table: "Survey",
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 250);

            migrationBuilder.AlterColumn<string>(
                name: "RespondentType",
                schema: "Survey",
                table: "Survey",
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 250);
        }
    }
}
