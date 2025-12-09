using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// GitHub.Services/GitHubService.cs
using Microsoft.Extensions.Options;
using Octokit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitHub.Services
{
    public class GitHubService : IGitHubService
    {
        private readonly GitHubClient _client;
        private readonly string _username;

        public GitHubService(IOptions<GitHubOptions> options)
        {
            _username = options.Value.Username ?? throw new ArgumentNullException(nameof(options.Value.Username));
            string token = options.Value.PersonalAccessToken ?? throw new ArgumentNullException(nameof(options.Value.PersonalAccessToken));

            // יצירת ה-Client עם הזדהות באמצעות Personal Access Token
            _client = new GitHubClient(new ProductHeaderValue("Developer-Portfolio-App"))
            {
                Credentials = new Credentials(token)
            };
        }

        // 1. שליפת רשימת ה-Repositories של המשתמש והאיסוף מידע נוסף
        public async Task<IReadOnlyList<Repository>> GetPortfolioRepositoriesAsync()
        {
            // שליפת כל ה-Repositories של המשתמש המזוהה
            // ה-API של Octokit משתמש אוטומטית בפרטי המשתמש מה-Credentials
            var repos = await _client.Repository.GetAllForCurrent();
            return repos;
        }

        // 2. חיפוש Repositories ציבוריים
        public async Task<SearchRepositoryResult> SearchRepositoriesAsync(string? repoName, string? language, string? username)
        {
            // תיקון: מעבירים את מילת החיפוש (repoName) ישירות בתוך הסוגריים של ה-new
            // אם repoName הוא null או ריק, נשלח מחרוזת ריקה או כוכבית (*) כדי לא לשבור את הבקשה
            var term = !string.IsNullOrWhiteSpace(repoName) ? repoName : "*";

            var request = new SearchRepositoriesRequest(term);

            // הוספת קריטריונים נוספים דרך המאפיינים הקיימים
            if (!string.IsNullOrWhiteSpace(language))
            {
                // המרה מ-String ל-Enum של Octokit
                if (Enum.TryParse(typeof(Language), language, true, out var langEnum))
                {
                    request.Language = (Language)langEnum;
                }
            }

            if (!string.IsNullOrWhiteSpace(username))
            {
                request.User = username;
            }

            // הגדרות מיון (אופציונלי)
            request.SortField = RepoSearchSort.Stars;
            request.Order = SortDirection.Descending;

            return await _client.Search.SearchRepo(request);
        }

        // 3. שליפת שפות קוד ל-Repository ספציפי
        public async Task<IReadOnlyList<RepositoryLanguage>> GetRepositoryLanguagesAsync(string owner, string repoName)
        {
            // Octokit משתמש ב-long כדי לייצג את כמות הבתים
            var languages = await _client.Repository.GetAllLanguages(owner, repoName);
            return languages;
        }

        // 4. שליפת מספר ה-Pull Requests הפתוחים/סגורים ל-Repository ספציפי
        public async Task<int> GetRepositoryPullRequestCountAsync(string owner, string repoName)
        {
            // ה-API של GitHub מאפשר לסנן לפי מצב (Open, Closed, All). נשלוף את כולם
            var request = new PullRequestRequest { State = ItemStateFilter.All };
            var prs = await _client.PullRequest.GetAllForRepository(owner, repoName, request);
            return prs.Count;
        }
        public async Task<DateTimeOffset> GetLastUserActivityTimeAsync()
        {
            // הגדרות שליפה: עמוד 1, פריט 1 בלבד (אנחנו רוצים רק את האחרון)
            var options = new ApiOptions { PageSize = 1, PageCount = 1 };

            // שליפת אירועים שהמשתמש ביצע (Push, Create Repo, etc.)
            var events = await _client.Activity.Events.GetAllUserPerformed(_username, options);

            var lastEvent = events.FirstOrDefault();

            // אם אין אירועים בכלל, נחזיר תאריך ישן מאוד
            return lastEvent?.CreatedAt ?? DateTimeOffset.MinValue;
        }
    }
}
