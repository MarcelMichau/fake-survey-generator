FROM mcr.microsoft.com/dotnet/aspnet:5.0.0-preview.5-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0.100-preview.5-buster-slim AS build
COPY ./.git /.git
COPY Directory.Build.props /Directory.Build.props
RUN dotnet tool install --tool-path . nbgv --version 3.2.7-beta
WORKDIR /src
COPY src/server/FakeSurveyGenerator.API/FakeSurveyGenerator.API.csproj FakeSurveyGenerator.API/
COPY src/server/FakeSurveyGenerator.Application/FakeSurveyGenerator.Application.csproj FakeSurveyGenerator.Application/
COPY src/server/FakeSurveyGenerator.Domain/FakeSurveyGenerator.Domain.csproj FakeSurveyGenerator.Domain/
COPY src/server/FakeSurveyGenerator.Infrastructure/FakeSurveyGenerator.Infrastructure.csproj FakeSurveyGenerator.Infrastructure/
RUN dotnet restore FakeSurveyGenerator.API/FakeSurveyGenerator.API.csproj
COPY src/server/. .
WORKDIR /src/FakeSurveyGenerator.API
RUN dotnet build FakeSurveyGenerator.API.csproj -c Release -o /app

FROM build AS unittest
WORKDIR /src/FakeSurveyGenerator.Domain.Tests

FROM build AS integrationtest
WORKDIR /src/FakeSurveyGenerator.API.Tests.Integration

FROM build AS publish
RUN dotnet publish FakeSurveyGenerator.API.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "FakeSurveyGenerator.API.dll"]
# ENV COMPlus_EnableDiagnostics=0 <-- Use this to allow .NET Core to run in a read-only filesystem
