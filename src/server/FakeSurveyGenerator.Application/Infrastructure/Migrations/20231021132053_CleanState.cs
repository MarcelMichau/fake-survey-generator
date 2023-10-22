using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FakeSurveyGenerator.Application.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CleanState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Survey");

            migrationBuilder.CreateSequence(
                name: "SurveySeq",
                schema: "Survey",
                incrementBy: 10);

            migrationBuilder.CreateSequence(
                name: "UserSeq",
                schema: "Survey",
                incrementBy: 10);

            migrationBuilder.CreateTable(
                name: "User",
                schema: "Survey",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    EmailAddress = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    ExternalUserId = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    ModifiedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Survey",
                schema: "Survey",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    OwnerId = table.Column<int>(type: "int", nullable: false),
                    Topic = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    RespondentType = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    NumberOfRespondents = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    ModifiedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Survey", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Survey_User_OwnerId",
                        column: x => x.OwnerId,
                        principalSchema: "Survey",
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SurveyOption",
                schema: "Survey",
                columns: table => new
                {
                    SurveyId = table.Column<int>(type: "int", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OptionText = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    NumberOfVotes = table.Column<int>(type: "int", nullable: false),
                    PreferredNumberOfVotes = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SurveyOption", x => new { x.SurveyId, x.Id });
                    table.ForeignKey(
                        name: "FK_SurveyOption_Survey_SurveyId",
                        column: x => x.SurveyId,
                        principalSchema: "Survey",
                        principalTable: "Survey",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Survey_OwnerId",
                schema: "Survey",
                table: "Survey",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_User_ExternalUserId",
                schema: "Survey",
                table: "User",
                column: "ExternalUserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SurveyOption",
                schema: "Survey");

            migrationBuilder.DropTable(
                name: "Survey",
                schema: "Survey");

            migrationBuilder.DropTable(
                name: "User",
                schema: "Survey");

            migrationBuilder.DropSequence(
                name: "SurveySeq",
                schema: "Survey");

            migrationBuilder.DropSequence(
                name: "UserSeq",
                schema: "Survey");
        }
    }
}
