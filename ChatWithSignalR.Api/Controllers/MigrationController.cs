using ChatWithSignalR.UsersDb;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System.Diagnostics;
using System.Threading;

namespace ChatWithSignalR.Api.Controllers
{
    public class MigrationController(IServiceProvider serviceProvider) : ControllerBase
    {


        // POST /migration
        [HttpPost("migration")]
        public async Task<IActionResult> Migrate()
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();

                await EnsureDatabaseAsync(dbContext);
                await RunMigrationAsync(dbContext);
            }
            catch (Exception ex)
            {
                throw;
            }


            return Ok();
        }

        private static async Task EnsureDatabaseAsync(UsersDbContext dbContext)
        {
            var dbCreator = dbContext.Database.GetService<IRelationalDatabaseCreator>();

            var strategy = dbContext.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                // Create the database if it does not exist.
                // Do this first so there is then a database to start a transaction against.
                if (!await dbCreator.ExistsAsync())
                {
                    await dbCreator.CreateAsync();
                }
            });
        }

        private static async Task RunMigrationAsync(UsersDbContext dbContext)
        {
            var strategy = dbContext.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await dbContext.Database.MigrateAsync();
            });
        }
    }
}
