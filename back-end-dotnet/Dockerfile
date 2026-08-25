FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY RetroVibe.slnx .
COPY src/RetroVibe.Domain/*.csproj src/RetroVibe.Domain/
COPY src/RetroVibe.Application/*.csproj src/RetroVibe.Application/
COPY src/RetroVibe.Infrastructure/*.csproj src/RetroVibe.Infrastructure/
COPY src/RetroVibe.Api/*.csproj src/RetroVibe.Api/
COPY tests/RetroVibe.Api.IntegrationTests/*.csproj tests/RetroVibe.Api.IntegrationTests/
RUN dotnet restore src/RetroVibe.Api/RetroVibe.Api.csproj

COPY . .
RUN dotnet publish src/RetroVibe.Api/RetroVibe.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# The aspnet runtime image has no curl by default — needed for the docker-compose healthcheck.
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

EXPOSE 3333
ENTRYPOINT ["dotnet", "RetroVibe.Api.dll"]
