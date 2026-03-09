using GitViewer.DataAccess.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GitViewer.DataAccess.configs
{
    public static class DataAccessConfig
    {
        private const int MaxRetryCount = 3;

        //public static IServiceCollection RegisterDataAccessDependencies(this IServiceCollection services, string connectionString)
        //{
        //    services.AddDbContext<GitViewerServiceContext>(options =>
        //    //options.UseSqlServer(connectionString, opt => opt.EnableRetryOnFailure(MaxRetryCount)));
        //    options.UseNpgsql(connectionString));
        //    return services;
        //}

        public static IHost DataAccessMigrateDatabase(this IHost host)
        {
            using var scope = host.Services.CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<GitViewerServiceContext>();

            //if (context.Database.ProviderName is not "Microsoft.EntityFrameworkCore.InMemory")
            //    context.Database.Migrate();

            return host;
        }
    }
}
