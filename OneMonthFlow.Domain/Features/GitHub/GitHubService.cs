using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace OneMonthFlow.Domain.Features.GitHub;

public class GitHubService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GitHubService> _logger;
    private const string GitHubApiBaseUrl = "https://api.github.com";

    public GitHubService(HttpClient httpClient, ILogger<GitHubService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        
        // Set default headers for GitHub API
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "OneMonthFlow/1.0");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
    }

    public void SetAuthToken(string token)
    {
        if (!string.IsNullOrWhiteSpace(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
    }

    public async Task<List<GitHubRepositoryModel>> GetOrganizationRepositoriesAsync(string organization)
    {
        try
        {
            _logger.LogInformation("Fetching repositories for organization: {Organization}", organization);
            
            var repositories = new List<GitHubRepositoryModel>();
            var page = 1;
            const int perPage = 100;
            bool hasMore = true;

            while (hasMore)
            {
                var url = $"{GitHubApiBaseUrl}/orgs/{organization}/repos?per_page={perPage}&page={page}&sort=updated&direction=desc";
                _logger.LogInformation("Fetching page {Page} from: {Url}", page, url);

                var response = await _httpClient.GetAsync(url);
                
                if (response.IsSuccessStatusCode)
                {
                    var pageRepos = await response.Content.ReadFromJsonAsync<List<GitHubRepositoryModel>>(
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = false });
                    
                    if (pageRepos != null && pageRepos.Any())
                    {
                        repositories.AddRange(pageRepos);
                        _logger.LogInformation("Fetched {Count} repositories from page {Page}", pageRepos.Count, page);
                        
                        // Check if there are more pages
                        hasMore = pageRepos.Count == perPage;
                        page++;
                        
                        // Rate limiting: GitHub allows 60 requests per hour for unauthenticated requests
                        // Add a small delay to avoid hitting rate limits
                        await Task.Delay(1000);
                    }
                    else
                    {
                        hasMore = false;
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Organization {Organization} not found", organization);
                    hasMore = false;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    _logger.LogError("Access forbidden. Rate limit may have been exceeded or token is invalid.");
                    var rateLimitRemaining = response.Headers.Contains("X-RateLimit-Remaining") 
                        ? response.Headers.GetValues("X-RateLimit-Remaining").FirstOrDefault() 
                        : "unknown";
                    _logger.LogError("Rate limit remaining: {Remaining}", rateLimitRemaining);
                    hasMore = false;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to fetch repositories. Status: {Status}, Error: {Error}", 
                        response.StatusCode, errorContent);
                    hasMore = false;
                }
            }

            _logger.LogInformation("Total repositories fetched: {Count}", repositories.Count);
            return repositories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching repositories for organization: {Organization}", organization);
            throw;
        }
    }

    public async Task<GitHubRepositoryModel?> GetRepositoryAsync(string owner, string repo)
    {
        try
        {
            var url = $"{GitHubApiBaseUrl}/repos/{owner}/{repo}";
            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<GitHubRepositoryModel>(
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = false });
            }
            
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching repository: {Owner}/{Repo}", owner, repo);
            return null;
        }
    }
}

