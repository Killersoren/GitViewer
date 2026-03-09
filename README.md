# GitViewer
This project is part of the GitViewer system:

- GitViewerClient (React frontend)
- GitViewer (ASP.NET Core API)
- GitViewerLogging (Logging microservice)

# GitViewer API

Backend API for the GitViewer system.

Related repositories:

* **[GitViewerLogging](https://github.com/Killersoren/GitViewerLogging)** – Logging microservice
* **[GitViewerClient](https://github.com/Killersoren/GitViewerClient)** – React frontend

## Description

GitViewer API is the main backend service responsible for user management and repository data.

It exposes REST endpoints used by the GitViewerClient frontend and produces logging events that are consumed by the GitViewerLogging service.

The service is built using **ASP.NET Core** and **Entity Framework Core**.

## Getting Started

### Requirements

* .NET 9 SDK
* Docker

### Running the program locally

1. Ensure **Docker is running**.
2. (Optional) Start a **RabbitMQ container** if you want to enable logging.
3. Run the application:

```
dotnet run --project AppHost
```

The API will start locally and expose REST endpoints for repository and user operations.

A **PostgreSQL Docker container** will automatically be started for the local database.
Database data is stored in a persistent Docker volume.
