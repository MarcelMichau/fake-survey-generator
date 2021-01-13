FROM mcr.microsoft.com/dotnet/runtime:5.0.2-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0.102-buster-slim AS build
WORKDIR /src
COPY ["src/server/FakeSurveyGenerator.Worker/FakeSurveyGenerator.Worker.csproj", "src/server/FakeSurveyGenerator.Worker/"]
RUN dotnet restore "src/server/FakeSurveyGenerator.Worker/FakeSurveyGenerator.Worker.csproj"
COPY . .
WORKDIR "/src/src/server/FakeSurveyGenerator.Worker"
RUN dotnet build "FakeSurveyGenerator.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FakeSurveyGenerator.Worker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FakeSurveyGenerator.Worker.dll"]