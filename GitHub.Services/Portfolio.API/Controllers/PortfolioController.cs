// Portfolio.API/Controllers/PortfolioController.cs
using GitHub.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Octokit;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Portfolio.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PortfolioController : ControllerBase
    {
        private readonly IGitHubService _gitHubService;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _configuration;

        // מפתח ל-Cache
        private const string CacheKey = "UserPortfolioCache";

        public PortfolioController(IGitHubService gitHubService,
                                   IMemoryCache cache,
                                   IConfiguration configuration)
        {
            _gitHubService = gitHubService;
            _cache = cache;
            _configuration = configuration;
        }
        [HttpGet("GetPortfolio")]
        public async Task<ActionResult<IEnumerable<PortfolioRepoDto>>> GetPortfolio()
        {
            // 1. בדיקה מול GitHub: מתי קרה השינוי האחרון בחשבון?
            // זו קריאה קלה ומהירה יחסית
            var lastActivityOnGitHub = await _gitHubService.GetLastUserActivityTimeAsync();

            // 2. בדיקת המטמון
            if (_cache.TryGetValue(CacheKey, out CachedPortfolioData? cachedData))
            {
                // האם המידע במטמון עדכני ביחס לפעילות האחרונה ב-GitHub?
                // אם הפעילות האחרונה קרתה לפני או בדיוק בזמן שיצרנו את המטמון - המידע תקף
                if (cachedData != null && cachedData.LastUpdated >= lastActivityOnGitHub)
                {
                    return Ok(cachedData.Portfolio);
                }
            }

            // --- אם הגענו לפה, סימן שהמידע לא ב-Cache או שהוא לא מעודכן ---

            // 3. שליפת המידע המלא (התהליך הכבד)
            var repos = await _gitHubService.GetPortfolioRepositoriesAsync();
            var portfolio = new List<PortfolioRepoDto>();
            string owner = _configuration.GetValue<string>("GitHub:Username") ?? "";

            foreach (var repo in repos)
            {
                var dto = PortfolioRepoDto.FromRepository(repo);
                dto.Languages = (await _gitHubService.GetRepositoryLanguagesAsync(owner, repo.Name))
                                .ToDictionary(l => l.Name, l => l.NumberOfBytes);
                dto.PullRequestsCount = await _gitHubService.GetRepositoryPullRequestCountAsync(owner, repo.Name);
                dto.LastCommitDate = repo.UpdatedAt;
                portfolio.Add(dto);
            }

            // 4. שמירה ב-Cache עם תאריך העדכון הנוכחי (שהוא האירוע האחרון שמצאנו)
            var dataToCache = new CachedPortfolioData
            {
                Portfolio = portfolio,
                LastUpdated = lastActivityOnGitHub // שומרים את החותמת של האירוע האחרון
            };

            // אופציונלי: עדיין כדאי לשמור Expiration אבסולוטי ארוך (למשל שעה) כדי לרענן זיכרון
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(1));

            _cache.Set(CacheKey, dataToCache, cacheEntryOptions);

            return Ok(portfolio);
        }
        

        /// <summary>
        /// חיפוש Repositories ציבוריים ב-GitHub עם סינון.
        /// </summary>
        [HttpGet("SearchRepositories")]
        public async Task<ActionResult<SearchRepositoryResult>> SearchRepositories(
            [FromQuery] string? repoName,
            [FromQuery] string? language,
            [FromQuery] string? username)
        {
            var result = await _gitHubService.SearchRepositoriesAsync(repoName, language, username);
            return Ok(result);
        }
        

        
    

        
    }
}
