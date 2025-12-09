using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// GitHub.Services/PortfolioRepoDto.cs
using Octokit;
using System.Collections.Generic;

namespace GitHub.Services
{
    public class PortfolioRepoDto
    {
        public string? Name { get; set; }
        public string? HtmlUrl { get; set; } // קישור ל-Repo ב-GitHub
        public string? Homepage { get; set; } // קישור לאתר ה-Repo (אם יש)
        public DateTimeOffset? LastCommitDate { get; set; }
        public int StargazersCount { get; set; } // מס' כוכבים
        public int PullRequestsCount { get; set; }
        public Dictionary<string, long>? Languages { get; set; } // שפה: בייטים

        // שיטה ליצירת DTO מ-Octokit.Repository
        public static PortfolioRepoDto FromRepository(Repository repo)
        {
            return new PortfolioRepoDto
            {
                Name = repo.Name,
                HtmlUrl = repo.HtmlUrl,
                Homepage = repo.Homepage,
                StargazersCount = repo.StargazersCount,
                // LastCommitDate והנתונים המורכבים ימולאו בהמשך ב-API
            };
        }
    }
}
