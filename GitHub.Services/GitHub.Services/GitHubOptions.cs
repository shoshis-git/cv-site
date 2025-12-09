using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// GitHub.Services/GitHubOptions.cs
namespace GitHub.Services
{
    public class GitHubOptions
    {
        // השם של סעיף הקונפיגורציה ב-appsettings.json או secrets.json
        public const string GitHub = "GitHub";

        public string? Username { get; set; }
        public string? PersonalAccessToken { get; set; }
    }
}
