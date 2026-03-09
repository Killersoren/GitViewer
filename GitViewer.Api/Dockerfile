# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution file and restore
COPY ../GitViewer.sln ./
COPY ../GitViewer.Api/GitViewer.Api.csproj GitViewer.Api/
COPY ../GitViewer.DataAccess/GitViewer.DataAccess.csproj GitViewer.DataAccess/
COPY ../GitViewer.MigrationService/GitViewer.MigrationService.csproj GitViewer.MigrationService/

RUN dotnet restore GitViewer.Api/GitViewer.Api.csproj

# Copy rest of the source
COPY .. .

# Publish API
WORKDIR /src/GitViewer.Api
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# ssh-keygen install
RUN apt-get update && apt-get install -y --no-install-recommends openssh-client git \
    && rm -rf /var/lib/apt/lists/*

EXPOSE 8080
ENTRYPOINT ["dotnet", "GitViewer.Api.dll"]
