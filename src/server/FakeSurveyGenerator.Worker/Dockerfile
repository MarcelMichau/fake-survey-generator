#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:9.0-noble-chiseled-extra AS base
USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/server/Directory.Build.props", "src/server/"]
COPY ["src/server/FakeSurveyGenerator.Worker/FakeSurveyGenerator.Worker.csproj", "src/server/FakeSurveyGenerator.Worker/"]
COPY ["src/server/FakeSurveyGenerator.Application/FakeSurveyGenerator.Application.csproj", "src/server/FakeSurveyGenerator.Application/"]
RUN dotnet restore "./src/server/FakeSurveyGenerator.Worker/./FakeSurveyGenerator.Worker.csproj"
COPY . .
WORKDIR "/src/src/server/FakeSurveyGenerator.Worker"
RUN dotnet build "./FakeSurveyGenerator.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./FakeSurveyGenerator.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "FakeSurveyGenerator.Worker.dll"]