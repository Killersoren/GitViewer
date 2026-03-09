using GitViewer.DataAccess.Models;

namespace GitViewer.MigrationService;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.AddServiceDefaults();

        builder.AddNpgsqlDbContext<GitViewerServiceContext>("pgdata");

        builder.Services.AddHostedService<Worker>();

        var app = builder.Build();
        app.Run();

    }
}