FROM mcr.microsoft.com/dotnet/aspnet:6.0.6-bullseye-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0.301-bullseye-slim AS build
WORKDIR /src/server
COPY src/server/FakeSurveyGenerator.Application/FakeSurveyGenerator.Application.csproj FakeSurveyGenerator.Application/
COPY src/server/FakeSurveyGenerator.Domain/FakeSurveyGenerator.Domain.csproj FakeSurveyGenerator.Domain/
COPY src/server/FakeSurveyGenerator.Infrastructure/FakeSurveyGenerator.Infrastructure.csproj FakeSurveyGenerator.Infrastructure/
COPY src/server/FakeSurveyGenerator.Shared/FakeSurveyGenerator.Shared.csproj FakeSurveyGenerator.Shared/
COPY src/server/FakeSurveyGenerator.Worker/FakeSurveyGenerator.Worker.csproj FakeSurveyGenerator.Worker/
COPY src/server/FakeSurveyGenerator.Worker/packages.lock.json FakeSurveyGenerator.Worker/
RUN dotnet restore FakeSurveyGenerator.Worker/FakeSurveyGenerator.Worker.csproj
COPY src/server/. .
WORKDIR /src/server/FakeSurveyGenerator.Worker
RUN dotnet build FakeSurveyGenerator.Worker.csproj --no-restore -c Release -o /app/build

FROM build AS publish
RUN dotnet publish FakeSurveyGenerator.Worker.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FakeSurveyGenerator.Worker.dll"]