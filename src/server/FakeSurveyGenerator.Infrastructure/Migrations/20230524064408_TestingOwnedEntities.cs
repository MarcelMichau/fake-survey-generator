using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FakeSurveyGenerator.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TestingOwnedEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SurveyOption",
                schema: "Survey",
                table: "SurveyOption");

            migrationBuilder.DropIndex(
                name: "IX_SurveyOption_SurveyId",
                schema: "Survey",
                table: "SurveyOption");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "Survey",
                table: "SurveyOption");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                schema: "Survey",
                table: "SurveyOption");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                schema: "Survey",
                table: "SurveyOption");

            migrationBuilder.DropColumn(
                name: "ModifiedOn",
                schema: "Survey",
                table: "SurveyOption");

            migrationBuilder.DropSequence(
                name: "SurveyOptionSeq",
                schema: "Survey");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                schema: "Survey",
                table: "SurveyOption",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SurveyOption",
                schema: "Survey",
                table: "SurveyOption",
                columns: new[] { "SurveyId", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SurveyOption",
                schema: "Survey",
                table: "SurveyOption");

            migrationBuilder.CreateSequence(
                name: "SurveyOptionSeq",
                schema: "Survey",
                incrementBy: 10);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                schema: "Survey",
                table: "SurveyOption",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                schema: "Survey",
                table: "SurveyOption",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedOn",
                schema: "Survey",
                table: "SurveyOption",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new System.DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                schema: "Survey",
                table: "SurveyOption",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ModifiedOn",
                schema: "Survey",
                table: "SurveyOption",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SurveyOption",
                schema: "Survey",
                table: "SurveyOption",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_SurveyOption_SurveyId",
                schema: "Survey",
                table: "SurveyOption",
                column: "SurveyId");
        }
    }
}
