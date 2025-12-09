using GitHub.Services;

namespace Portfolio.API
{
    // נגדיר מחלקה פנימית או חיצונית לעטיפת המידע ב-Cache
    public class CachedPortfolioData
    {
        public List<PortfolioRepoDto> Portfolio { get; set; }
        public DateTimeOffset LastUpdated { get; set; }
    }
}
