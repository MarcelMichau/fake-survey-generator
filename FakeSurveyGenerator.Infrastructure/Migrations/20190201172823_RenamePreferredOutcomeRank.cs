using Microsoft.EntityFrameworkCore.Migrations;

namespace FakeSurveyGenerator.Infrastructure.Migrations
{
    public partial class RenamePreferredOutcomeRank : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PreferredOutcomeRank",
                schema: "Survey",
                table: "SurveyOption",
                newName: "PreferredNumberOfVotes");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PreferredNumberOfVotes",
                schema: "Survey",
                table: "SurveyOption",
                newName: "PreferredOutcomeRank");
        }
    }
}
