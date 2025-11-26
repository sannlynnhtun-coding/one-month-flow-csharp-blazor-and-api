using System.Text.Json.Serialization;

namespace OneMonthFlow.Domain.Features.GitHub;

public class GitHubRepositoryModel
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("full_name")]
    public string FullName { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; set; } = string.Empty;

    [JsonPropertyName("clone_url")]
    public string CloneUrl { get; set; } = string.Empty;

    [JsonPropertyName("language")]
    public string? Language { get; set; }

    [JsonPropertyName("stargazers_count")]
    public int StargazersCount { get; set; }

    [JsonPropertyName("forks_count")]
    public int ForksCount { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [JsonPropertyName("pushed_at")]
    public DateTime? PushedAt { get; set; }

    [JsonPropertyName("topics")]
    public List<string> Topics { get; set; } = new();

    [JsonPropertyName("archived")]
    public bool Archived { get; set; }

    [JsonPropertyName("private")]
    public bool Private { get; set; }
}

