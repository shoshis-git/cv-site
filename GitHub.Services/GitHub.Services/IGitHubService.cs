using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// GitHub.Services/IGitHubService.cs
using Octokit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitHub.Services
{
    public interface IGitHubService
    {
        Task<IReadOnlyList<Repository>> GetPortfolioRepositoriesAsync();
        Task<SearchRepositoryResult> SearchRepositoriesAsync(string? repoName, string? language, string? username);
        Task<IReadOnlyList<RepositoryLanguage>> GetRepositoryLanguagesAsync(string owner, string repoName);
        Task<DateTimeOffset> GetLastUserActivityTimeAsync();
        Task<int> GetRepositoryPullRequestCountAsync(string owner, string repoName);
    }
}
