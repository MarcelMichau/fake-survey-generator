FROM mcr.microsoft.com/dotnet/aspnet:6.0.6-bullseye-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0.301-bullseye-slim AS build
WORKDIR /src/server
COPY src/server/Directory.Build.props .
COPY src/server/version.json .
COPY src/server/FakeSurveyGenerator.API/FakeSurveyGenerator.API.csproj FakeSurveyGenerator.API/
COPY src/server/FakeSurveyGenerator.Application/FakeSurveyGenerator.Application.csproj FakeSurveyGenerator.Application/
COPY src/server/FakeSurveyGenerator.Domain/FakeSurveyGenerator.Domain.csproj FakeSurveyGenerator.Domain/
COPY src/server/FakeSurveyGenerator.Infrastructure/FakeSurveyGenerator.Infrastructure.csproj FakeSurveyGenerator.Infrastructure/
COPY src/server/FakeSurveyGenerator.Shared/FakeSurveyGenerator.Shared.csproj FakeSurveyGenerator.Shared/
COPY src/server/FakeSurveyGenerator.API/packages.lock.json FakeSurveyGenerator.API/
RUN dotnet restore FakeSurveyGenerator.API/FakeSurveyGenerator.API.csproj
COPY src/server/. .
WORKDIR /
COPY .git .git
WORKDIR /src/server/FakeSurveyGenerator.API
RUN dotnet build FakeSurveyGenerator.API.csproj --no-restore -c Release -o /app/build

FROM build AS domaintest
WORKDIR /src/server/FakeSurveyGenerator.Domain.Tests
RUN dotnet restore FakeSurveyGenerator.Domain.Tests.csproj

FROM build AS applicationtest
WORKDIR /src/server/FakeSurveyGenerator.Application.Tests
RUN dotnet restore FakeSurveyGenerator.Application.Tests.csproj

FROM build AS integrationtest
WORKDIR /src/server/FakeSurveyGenerator.API.Tests.Integration
RUN dotnet restore FakeSurveyGenerator.API.Tests.Integration.csproj

FROM build AS publish
RUN dotnet publish FakeSurveyGenerator.API.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FakeSurveyGenerator.API.dll"]
# ENV COMPlus_EnableDiagnostics=0 <-- Use this to allow .NET Core to run in a read-only filesystem
