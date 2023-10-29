dotnet tool restore
Set-Location src/server/FakeSurveyGenerator.Application
dotnet dotnet-ef migrations script -o DbMigrationScript.sql -i