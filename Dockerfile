# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props global.json ./
COPY src/Fintrox.Domain/Fintrox.Domain.csproj src/Fintrox.Domain/
COPY src/Fintrox.Contracts/Fintrox.Contracts.csproj src/Fintrox.Contracts/
COPY src/Fintrox.Application/Fintrox.Application.csproj src/Fintrox.Application/
COPY src/Fintrox.Infrastructure/Fintrox.Infrastructure.csproj src/Fintrox.Infrastructure/
COPY src/Fintrox.Api/Fintrox.Api.csproj src/Fintrox.Api/

RUN dotnet restore src/Fintrox.Api/Fintrox.Api.csproj

COPY src/ src/

RUN dotnet publish src/Fintrox.Api/Fintrox.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_EnableDiagnostics=0

EXPOSE 8080

COPY --from=build /app/publish .

USER $APP_UID

ENTRYPOINT ["dotnet", "Fintrox.Api.dll"]
