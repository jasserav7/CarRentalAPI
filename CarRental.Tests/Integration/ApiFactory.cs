using CarRental.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CarRental.Tests.Integration
{
    /// <summary>
    /// Boots the real API with SQLite in memory instead of SQL Server. The schema is created before
    /// the app starts because Program.cs seeds the database right after building the host.
    /// </summary>
    public class ApiFactory : WebApplicationFactory<Program>
    {
        private readonly SqliteConnection _connection = new("DataSource=:memory:");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            _connection.Open();

            // Program.cs fails fast without a connection string; the value is never used
            builder.UseSetting("ConnectionStrings:Database", "unused-in-tests");

            builder.ConfigureServices(services =>
            {
                var sqlServerRegistrations = services
                    .Where(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>)
                        || (d.ServiceType.Name.StartsWith("IDbContextOptionsConfiguration")
                            && d.ServiceType.GenericTypeArguments.Contains(typeof(AppDbContext))))
                    .ToList();
                foreach (var registration in sqlServerRegistrations)
                    services.Remove(registration);

                services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));

                using var provider = services.BuildServiceProvider();
                using var scope = provider.CreateScope();
                scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
                _connection.Dispose();
        }
    }
}
