using GitViewer.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace GitViewer.MigrationService;

public class Worker(
    IServiceProvider serviceProvider,
    IHostApplicationLifetime lifetime,
    ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Applying EF Core migrations ");
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<GitViewerServiceContext>();

        await db.Database.MigrateAsync(cancellationToken);

        logger.LogInformation("Migrations complete!");
        lifetime.StopApplication();
    }
}