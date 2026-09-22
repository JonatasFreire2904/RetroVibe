FROM node:22-alpine AS frontend-build
WORKDIR /frontend
COPY front-end/package.json front-end/package-lock.json ./
RUN npm ci
COPY front-end/ ./
ENV VITE_API_URL=/api
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend-build
WORKDIR /src
COPY back-end-dotnet/RetroVibe.slnx ./
COPY back-end-dotnet/src/RetroVibe.Domain/*.csproj src/RetroVibe.Domain/
COPY back-end-dotnet/src/RetroVibe.Application/*.csproj src/RetroVibe.Application/
COPY back-end-dotnet/src/RetroVibe.Infrastructure/*.csproj src/RetroVibe.Infrastructure/
COPY back-end-dotnet/src/RetroVibe.Api/*.csproj src/RetroVibe.Api/
COPY back-end-dotnet/tests/RetroVibe.Api.IntegrationTests/*.csproj tests/RetroVibe.Api.IntegrationTests/
RUN dotnet restore RetroVibe.slnx
COPY back-end-dotnet/ ./
RUN dotnet publish src/RetroVibe.Api/RetroVibe.Api.csproj -c Release -o /app/publish --no-restore

FROM backend-build AS test
RUN dotnet test RetroVibe.slnx -c Release --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=backend-build /app/publish ./
COPY --from=frontend-build /frontend/dist ./wwwroot/
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DATABASE_PATH=/app/data/retrovibe.db
EXPOSE 3333
ENTRYPOINT ["dotnet", "RetroVibe.Api.dll"]
