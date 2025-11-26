using OneMonthFlow.Domain.Features.Project;
using OneMonthFlow.Domain.Features.ProjectTechStack;
using Microsoft.Extensions.Logging;

namespace OneMonthFlow.Domain.Features.GitHub;

public class GitHubImportService
{
    private readonly GitHubService _gitHubService;
    private readonly ProjectService _projectService;
    private readonly TechStackService _techStackService;
    private readonly ProjectTechStackService _projectTechStackService;
    private readonly ILogger<GitHubImportService> _logger;

    // Mapping of GitHub languages to TechStack codes
    private readonly Dictionary<string, string> _languageToTechStackMap = new()
    {
        { "C#", "TS001" }, // C# .NET
        { "CSharp", "TS001" },
        { "JavaScript", "TS002" },
        { "TypeScript", "TS002" },
        { "Python", "TS003" },
        { "Java", "TS004" },
        { "React", "TS005" },
        { "Angular", "TS006" },
        { "SQL", "TS007" },
        { "Node.js", "TS008" },
        { "PHP", "TS009" },
        { "Dart", "TS010" },
        { "Go", "TS011" },
        { "Vue", "TS012" },
        { "Svelte", "TS013" }
    };

    public GitHubImportService(
        GitHubService gitHubService,
        ProjectService projectService,
        TechStackService techStackService,
        ProjectTechStackService projectTechStackService,
        ILogger<GitHubImportService> logger)
    {
        _gitHubService = gitHubService;
        _projectService = projectService;
        _techStackService = techStackService;
        _projectTechStackService = projectTechStackService;
        _logger = logger;
    }

    public async Task<ImportResult> ImportOrganizationRepositoriesAsync(string organization, string? gitHubToken = null)
    {
        var result = new ImportResult();
        
        try
        {
            if (!string.IsNullOrWhiteSpace(gitHubToken))
            {
                _gitHubService.SetAuthToken(gitHubToken);
            }

            _logger.LogInformation("Starting import of repositories from organization: {Organization}", organization);
            
            var repositories = await _gitHubService.GetOrganizationRepositoriesAsync(organization);
            result.TotalRepositories = repositories.Count;

            _logger.LogInformation("Found {Count} repositories to import", repositories.Count);

            foreach (var repo in repositories)
            {
                try
                {
                    // Skip archived or private repositories if needed
                    if (repo.Archived)
                    {
                        result.SkippedArchived++;
                        _logger.LogInformation("Skipping archived repository: {Name}", repo.Name);
                        continue;
                    }

                    // Generate project code from repository name
                    var projectCode = GenerateProjectCode(repo.Name);
                    
                    // Check if project already exists
                    var existingProjects = await _projectService.GetProjectsAsync(1, 1000);
                    if (existingProjects.IsSuccess && existingProjects.Data?.Projects.Any(p => 
                        p.ProjectCode.Equals(projectCode, StringComparison.OrdinalIgnoreCase)) == true)
                    {
                        result.SkippedExisting++;
                        _logger.LogInformation("Project {Code} already exists, skipping", projectCode);
                        continue;
                    }

                    // Create project request
                    var projectRequest = new ProjectRequestModel
                    {
                        ProjectCode = projectCode,
                        ProjectName = FormatProjectName(repo.Name),
                        RepoUrl = repo.HtmlUrl,
                        ProjectDescription = repo.Description ?? $"Repository: {repo.FullName}",
                        StartDate = repo.CreatedAt?.Date,
                        EndDate = repo.UpdatedAt?.Date,
                        Teams = new List<ProjectTeamRequestModel>()
                    };

                    // Determine status based on repository activity
                    var status = DetermineStatus(repo);
                    projectRequest.Status = status;

                    // Create the project
                    var createResult = await _projectService.CreateProjectAsync(projectRequest);
                    
                    if (createResult.IsSuccess)
                    {
                        result.Imported++;
                        _logger.LogInformation("Successfully imported project: {Name} ({Code})", 
                            projectRequest.ProjectName, projectCode);

                        // Associate tech stacks based on language
                        if (!string.IsNullOrWhiteSpace(repo.Language))
                        {
                            await AssociateTechStacksAsync(projectCode, repo.Language, repo.Topics);
                        }
                    }
                    else
                    {
                        result.Failed++;
                        result.Errors.Add($"{repo.Name}: {createResult.Message}");
                        _logger.LogError("Failed to import project {Name}: {Error}", 
                            repo.Name, createResult.Message);
                    }

                    // Small delay to avoid overwhelming the database
                    await Task.Delay(100);
                }
                catch (Exception ex)
                {
                    result.Failed++;
                    result.Errors.Add($"{repo.Name}: {ex.Message}");
                    _logger.LogError(ex, "Error importing repository: {Name}", repo.Name);
                }
            }

            _logger.LogInformation("Import completed. Imported: {Imported}, Failed: {Failed}, Skipped: {Skipped}", 
                result.Imported, result.Failed, result.SkippedExisting + result.SkippedArchived);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing organization repositories");
            result.Errors.Add($"Import failed: {ex.Message}");
            return result;
        }
    }

    private string GenerateProjectCode(string repositoryName)
    {
        // Convert repository name to project code format (e.g., "pos_csharp" -> "PROJ_POS_CSHARP")
        var code = repositoryName
            .Replace("-", "_")
            .Replace(".", "_")
            .ToUpper();
        
        // Ensure it starts with PROJ_ if it doesn't already
        if (!code.StartsWith("PROJ_"))
        {
            code = "PROJ_" + code;
        }

        // Limit length to 50 characters (database constraint)
        if (code.Length > 50)
        {
            code = code.Substring(0, 50);
        }

        return code;
    }

    private string FormatProjectName(string repositoryName)
    {
        // Convert repository name to a more readable format
        // e.g., "pos_csharp" -> "Pos Csharp" or "pos-csharp" -> "Pos Csharp"
        return repositoryName
            .Replace("-", " ")
            .Replace("_", " ")
            .Split(' ')
            .Select(word => char.ToUpper(word[0]) + word.Substring(1).ToLower())
            .Aggregate((a, b) => a + " " + b);
    }

    private string DetermineStatus(GitHubRepositoryModel repo)
    {
        // Determine project status based on repository activity
        if (repo.Archived)
            return "Archived";
        
        var daysSinceUpdate = repo.PushedAt.HasValue 
            ? (DateTime.UtcNow - repo.PushedAt.Value).TotalDays 
            : double.MaxValue;

        if (daysSinceUpdate < 30)
            return "Active";
        else if (daysSinceUpdate < 90)
            return "In Progress";
        else
            return "Completed";
    }

    private async Task AssociateTechStacksAsync(string projectCode, string language, List<string> topics)
    {
        try
        {
            var techStackCodes = new HashSet<string>();

            // Get tech stack code from language
            if (_languageToTechStackMap.TryGetValue(language, out var techStackCode))
            {
                techStackCodes.Add(techStackCode);
            }

            // Also check topics for additional tech stack hints
            foreach (var topic in topics)
            {
                var topicLower = topic.ToLower();
                if (topicLower.Contains("react") && _languageToTechStackMap.TryGetValue("React", out var reactCode))
                {
                    techStackCodes.Add(reactCode);
                }
                else if (topicLower.Contains("angular") && _languageToTechStackMap.TryGetValue("Angular", out var angularCode))
                {
                    techStackCodes.Add(angularCode);
                }
                else if (topicLower.Contains("vue") && _languageToTechStackMap.TryGetValue("Vue", out var vueCode))
                {
                    techStackCodes.Add(vueCode);
                }
                else if (topicLower.Contains("svelte") && _languageToTechStackMap.TryGetValue("Svelte", out var svelteCode))
                {
                    techStackCodes.Add(svelteCode);
                }
                else if (topicLower.Contains("node") && _languageToTechStackMap.TryGetValue("Node.js", out var nodeCode))
                {
                    techStackCodes.Add(nodeCode);
                }
            }

            // Verify tech stacks exist before associating
            if (techStackCodes.Any())
            {
                var techStacks = await _techStackService.GetTechStacksAsync();
                if (techStacks.IsSuccess && techStacks.Data?.TechStacks != null)
                {
                    var existingTechStackCodes = techStacks.Data.TechStacks
                        .Select(ts => ts.TechStackCode)
                        .ToHashSet();

                    var validTechStackCodes = techStackCodes
                        .Where(code => existingTechStackCodes.Contains(code))
                        .ToList();

                    if (validTechStackCodes.Any())
                    {
                        var request = new ProjectTechStackRequestModel
                        {
                            ProjectCode = projectCode,
                            TechStackCodes = validTechStackCodes
                        };

                        var result = await _projectTechStackService.CreateProjectTechStacksAsync(request);
                        if (result.IsSuccess)
                        {
                            _logger.LogInformation("Associated {Count} tech stack(s) with project {ProjectCode}", 
                                validTechStackCodes.Count, projectCode);
                        }
                        else
                        {
                            _logger.LogWarning("Failed to associate tech stacks with project {ProjectCode}: {Error}", 
                                projectCode, result.Message);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error associating tech stacks for project {ProjectCode}", projectCode);
        }
    }
}

public class ImportResult
{
    public int TotalRepositories { get; set; }
    public int Imported { get; set; }
    public int Failed { get; set; }
    public int SkippedExisting { get; set; }
    public int SkippedArchived { get; set; }
    public List<string> Errors { get; set; } = new();
}

