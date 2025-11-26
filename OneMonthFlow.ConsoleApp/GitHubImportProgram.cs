using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using OneMonthFlow.Domain.Features.GitHub;
using OneMonthFlow.Domain.Features.Project;
using OneMonthFlow.Domain.Features.TechStack;

namespace OneMonthFlow.ConsoleApp;

public class GitHubImportProgram
{
    private static ILogger<GitHubService> _gitHubLogger;
    private static ILogger<GitHubImportService> _importLogger;
    private static ILogger<ProjectService> _projectLogger;
    private static ILogger<TechStackService> _techStackLogger;

    public static async Task Main(string[] args)
    {
        Console.WriteLine("=== GitHub Repository Import Tool ===");
        Console.WriteLine();

        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddCommandLine(args)
            .Build();

        // Setup logging
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .SetMinimumLevel(LogLevel.Information)
                .AddSerilog();
        });

        _gitHubLogger = loggerFactory.CreateLogger<GitHubService>();
        _importLogger = loggerFactory.CreateLogger<GitHubImportService>();
        _projectLogger = loggerFactory.CreateLogger<ProjectService>();
        _techStackLogger = loggerFactory.CreateLogger<TechStackService>();

        try
        {
            // Get organization name from args or config
            var organization = args.Length > 0 ? args[0] : configuration["GitHub:Organization"] ?? "one-project-one-month";
            var gitHubToken = configuration["GitHub:Token"] ?? Environment.GetEnvironmentVariable("GITHUB_TOKEN");

            Console.WriteLine($"Organization: {organization}");
            if (!string.IsNullOrWhiteSpace(gitHubToken))
            {
                Console.WriteLine("GitHub token: [CONFIGURED]");
            }
            else
            {
                Console.WriteLine("GitHub token: [NOT SET - Using unauthenticated requests (60 req/hour limit)]");
            }
            Console.WriteLine();

            // Initialize services
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            var dbConnectionFactory = new OneMonthFlow.Databases.MssqlConnectionFactory(connectionString);
            var sqlService = new OneMonthFlow.Databases.MssqlService(dbConnectionFactory, 
                loggerFactory.CreateLogger<OneMonthFlow.Databases.MssqlService>());

            var projectService = new ProjectService(sqlService);
            var techStackService = new TechStackService(sqlService);
            var projectTechStackService = new OneMonthFlow.Domain.Features.ProjectTechStack.ProjectTechStackService(sqlService);

            var httpClient = new HttpClient();
            var gitHubService = new GitHubService(httpClient, _gitHubLogger);
            var importService = new GitHubImportService(gitHubService, projectService, techStackService, projectTechStackService, _importLogger);

            // Confirm before proceeding
            Console.Write("Do you want to proceed with importing repositories? (y/n): ");
            var response = Console.ReadLine();
            if (response?.ToLower() != "y")
            {
                Console.WriteLine("Import cancelled.");
                return;
            }

            Console.WriteLine();
            Console.WriteLine("Starting import...");
            Console.WriteLine();

            // Import repositories
            var result = await importService.ImportOrganizationRepositoriesAsync(organization, gitHubToken);

            // Display results
            Console.WriteLine();
            Console.WriteLine("=== Import Results ===");
            Console.WriteLine($"Total repositories found: {result.TotalRepositories}");
            Console.WriteLine($"Successfully imported: {result.Imported}");
            Console.WriteLine($"Failed: {result.Failed}");
            Console.WriteLine($"Skipped (existing): {result.SkippedExisting}");
            Console.WriteLine($"Skipped (archived): {result.SkippedArchived}");
            Console.WriteLine();

            if (result.Errors.Any())
            {
                Console.WriteLine("Errors:");
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"  - {error}");
                }
            }

            Console.WriteLine();
            Console.WriteLine("Import completed!");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            Console.ResetColor();
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}

