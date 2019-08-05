FROM mcr.microsoft.com/dotnet/core/aspnet:3.0.0-preview7-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.0.100-preview7-buster AS build
WORKDIR /src
COPY FakeSurveyGenerator.API/FakeSurveyGenerator.API.csproj FakeSurveyGenerator.API/
COPY FakeSurveyGenerator.Domain/FakeSurveyGenerator.Domain.csproj FakeSurveyGenerator.Domain/
COPY FakeSurveyGenerator.Infrastructure/FakeSurveyGenerator.Infrastructure.csproj FakeSurveyGenerator.Infrastructure/
RUN dotnet restore FakeSurveyGenerator.API/FakeSurveyGenerator.API.csproj
COPY . .
WORKDIR /src/FakeSurveyGenerator.API
RUN dotnet build FakeSurveyGenerator.API.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish FakeSurveyGenerator.API.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "FakeSurveyGenerator.API.dll"]
