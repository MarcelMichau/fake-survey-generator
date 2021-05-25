FROM mcr.microsoft.com/dotnet/runtime:6.0.0-preview.4-bullseye-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0.100-preview.4-bullseye-slim AS build
WORKDIR /src/server
COPY src/server/FakeSurveyGenerator.Domain/FakeSurveyGenerator.Domain.csproj FakeSurveyGenerator.Domain/
COPY src/server/FakeSurveyGenerator.Infrastructure/FakeSurveyGenerator.Infrastructure.csproj FakeSurveyGenerator.Infrastructure/
COPY src/server/FakeSurveyGenerator.Worker/FakeSurveyGenerator.Worker.csproj FakeSurveyGenerator.Worker/
COPY src/server/FakeSurveyGenerator.Worker/packages.lock.json FakeSurveyGenerator.Worker/
RUN dotnet restore src/server/FakeSurveyGenerator.Worker/FakeSurveyGenerator.Worker.csproj
COPY src/server/. .
WORKDIR /src/server/FakeSurveyGenerator.Worker
RUN dotnet build FakeSurveyGenerator.Worker.csproj --no-restore -c Release -o /app/build

FROM build AS publish
RUN dotnet publish FakeSurveyGenerator.Worker.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FakeSurveyGenerator.Worker.dll"]