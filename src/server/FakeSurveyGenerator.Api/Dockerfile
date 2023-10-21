#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/server/Directory.Build.props", "src/server/"]
COPY ["src/server/version.json", "src/server/"]
COPY ["src/server/FakeSurveyGenerator.Api/FakeSurveyGenerator.Api.csproj", "src/server/FakeSurveyGenerator.Api/"]
COPY ["src/server/FakeSurveyGenerator.Application/FakeSurveyGenerator.Application.csproj", "src/server/FakeSurveyGenerator.Application/"]
RUN dotnet restore "./src/server/FakeSurveyGenerator.Api/./FakeSurveyGenerator.Api.csproj"
COPY . .
WORKDIR /
COPY .git .git
WORKDIR "/src/src/server/FakeSurveyGenerator.Api"
RUN dotnet build "./FakeSurveyGenerator.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./FakeSurveyGenerator.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "FakeSurveyGenerator.Api.dll"]