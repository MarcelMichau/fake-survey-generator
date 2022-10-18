using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FakeSurveyGenerator.Infrastructure.Migrations
{
    public partial class AddAuditFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                schema: "Survey",
                table: "User",
                maxLength: 250,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedOn",
                schema: "Survey",
                table: "User",
                nullable: false,
                defaultValue: new DateTimeOffset(new System.DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                schema: "Survey",
                table: "User",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ModifiedOn",
                schema: "Survey",
                table: "User",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                schema: "Survey",
                table: "SurveyOption",
                maxLength: 250,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedOn",
                schema: "Survey",
                table: "SurveyOption",
                nullable: false,
                defaultValue: new DateTimeOffset(new System.DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                schema: "Survey",
                table: "SurveyOption",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ModifiedOn",
                schema: "Survey",
                table: "SurveyOption",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedOn",
                schema: "Survey",
                table: "Survey",
                nullable: false,
                oldClrType: typeof(System.DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                schema: "Survey",
                table: "Survey",
                maxLength: 250,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                schema: "Survey",
                table: "Survey",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ModifiedOn",
                schema: "Survey",
                table: "Survey",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "Survey",
                table: "User");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                schema: "Survey",
                table: "User");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                schema: "Survey",
                table: "User");

            migrationBuilder.DropColumn(
                name: "ModifiedOn",
                schema: "Survey",
                table: "User");

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

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "Survey",
                table: "Survey");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                schema: "Survey",
                table: "Survey");

            migrationBuilder.DropColumn(
                name: "ModifiedOn",
                schema: "Survey",
                table: "Survey");

            migrationBuilder.AlterColumn<System.DateTime>(
                name: "CreatedOn",
                schema: "Survey",
                table: "Survey",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset));
        }
    }
}
