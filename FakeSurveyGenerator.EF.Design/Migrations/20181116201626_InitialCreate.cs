using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FakeSurveyGenerator.EF.Design.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Survey");

            migrationBuilder.CreateSequence(
                name: "SurveyOptionSeq",
                schema: "Survey",
                incrementBy: 10);

            migrationBuilder.CreateSequence(
                name: "SurveySeq",
                schema: "Survey",
                incrementBy: 10);

            migrationBuilder.CreateTable(
                name: "Survey",
                schema: "Survey",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Topic = table.Column<string>(nullable: false),
                    RespondentType = table.Column<string>(nullable: false),
                    NumberOfRespondents = table.Column<int>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Survey", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SurveyOption",
                schema: "Survey",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    OptionText = table.Column<string>(nullable: false),
                    NumberOfVotes = table.Column<int>(nullable: false),
                    SurveyId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SurveyOption", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SurveyOption_Survey_SurveyId",
                        column: x => x.SurveyId,
                        principalSchema: "Survey",
                        principalTable: "Survey",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SurveyOption_SurveyId",
                schema: "Survey",
                table: "SurveyOption",
                column: "SurveyId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SurveyOption",
                schema: "Survey");

            migrationBuilder.DropTable(
                name: "Survey",
                schema: "Survey");

            migrationBuilder.DropSequence(
                name: "SurveyOptionSeq",
                schema: "Survey");

            migrationBuilder.DropSequence(
                name: "SurveySeq",
                schema: "Survey");
        }
    }
}
