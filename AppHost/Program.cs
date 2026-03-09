using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var password = builder.AddParameter("postgresPassword", "1234");

// Add a PostgreSQL container
var postgres = builder.AddPostgres("postgresViewer")
    .WithDataVolume()
    .WithPassword(password)
    .WithEnvironment("POSTGRES_PASSWORD", "1234")
    .WithHostPort(5432)
    .WithPgAdmin();

var migrations = builder.AddProject<GitViewer_MigrationService>("migrations")
        .WithReference(postgres)
        .WaitFor(postgres);

var gitViewer = builder.AddProject<GitViewer_Api>("GitViewerAPI")
    .WithReference(postgres)
    .WithReference(migrations)
    .WaitFor(migrations);

await builder.Build().RunAsync();
