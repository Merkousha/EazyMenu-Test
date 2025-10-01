using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace EazyMenu.Infrastructure.Persistence;

public sealed class EazyMenuDbContextFactory : IDesignTimeDbContextFactory<EazyMenuDbContext>
{
    public EazyMenuDbContext CreateDbContext(string[] args)
    {
    var currentDirectory = Directory.GetCurrentDirectory();
    var solutionRoot = FindSolutionRoot(currentDirectory) ?? currentDirectory;
    var webProjectPath = Path.Combine(solutionRoot, "src", "Presentation", "Web");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(webProjectPath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=.;Database=EazyMenu;Trusted_Connection=True;TrustServerCertificate=True;";

        var optionsBuilder = new DbContextOptionsBuilder<EazyMenuDbContext>();
        optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.MigrationsAssembly(typeof(EazyMenuDbContext).Assembly.FullName);
            sqlOptions.EnableRetryOnFailure();
        });

        return new EazyMenuDbContext(optionsBuilder.Options);
    }
    private static string? FindSolutionRoot(string startPath)
    {
        var directory = new DirectoryInfo(startPath);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "src", "Presentation", "Web");
            if (Directory.Exists(candidate))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return null;
    }
}
