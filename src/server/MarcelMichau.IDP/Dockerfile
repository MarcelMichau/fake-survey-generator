FROM mcr.microsoft.com/dotnet/core/aspnet:3.1.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1.101-buster AS build
WORKDIR /src
COPY src/server/MarcelMichau.IDP/MarcelMichau.IDP.csproj MarcelMichau.IDP/
RUN dotnet restore MarcelMichau.IDP/MarcelMichau.IDP.csproj
COPY src/server/. .
WORKDIR /src/MarcelMichau.IDP
RUN dotnet build MarcelMichau.IDP.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish MarcelMichau.IDP.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "MarcelMichau.IDP.dll"]