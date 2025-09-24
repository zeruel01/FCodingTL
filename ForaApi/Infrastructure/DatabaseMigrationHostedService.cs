namespace ForaApi.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Fora.Infrastructure;
using Fora.Application;

public class DatabaseMigrationHostedService(IServiceProvider services, IHostApplicationLifetime lifetime, ILogger<DatabaseMigrationHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.MigrateAsync(cancellationToken);

            var envArgs = Environment.GetCommandLineArgs();
            if (envArgs.Contains("--import"))
            {
                var importer = scope.ServiceProvider.GetRequiredService<IImporter>();
                await importer.ImportAsync(CikSeed.All, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error applying migrations on startup");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
