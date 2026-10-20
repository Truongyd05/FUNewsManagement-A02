using BusinessObjects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services;

namespace PhungDangTruongRazorPages.Pages.News;

/// <summary>Public news view: no authentication required, only active news is shown.</summary>
public class IndexModel : PageModel
{
    private readonly INewsArticleService _news;
    private readonly ICategoryService _categories;

    public IndexModel(INewsArticleService news, ICategoryService categories)
    {
        _news = news;
        _categories = categories;
    }

    [BindProperty(SupportsGet = true)]
    public string? Keyword { get; set; }

    [BindProperty(SupportsGet = true)]
    public short? CategoryId { get; set; }

    public List<NewsArticle> Articles { get; set; } = new();

    public List<Category> Categories { get; set; } = new();

    /// <summary>Article count per category, based on the full active feed (used by the sidebar).</summary>
    public Dictionary<short, int> CategoryCounts { get; set; } = new();

    /// <summary>Total number of active articles.</summary>
    public int TotalCount { get; set; }

    /// <summary>Top stories for the sidebar, always based on the full active feed.</summary>
    public List<NewsArticle> TopStories { get; set; } = new();

    /// <summary>Most frequent tags for the sidebar, always based on the full active feed.</summary>
    public List<string> TrendingTags { get; set; } = new();

    public void OnGet()
    {
        Categories = _categories.GetActive();
        Articles = _news.Search(Keyword, CategoryId, null, activeOnly: true, createdById: null);

        // Counts for the sidebar are always based on the full active feed,
        // so filtering by one category does not zero-out the other counters.
        var activeArticles = _news.Search(null, null, null, activeOnly: true, createdById: null);
        CategoryCounts = activeArticles
            .Where(n => n.CategoryId.HasValue)
            .GroupBy(n => n.CategoryId!.Value)
            .ToDictionary(g => g.Key, g => g.Count());
        TotalCount = activeArticles.Count;

        // Sidebar rails read from the full active feed too, so they stay
        // populated even when the current filter returns nothing.
        TopStories = activeArticles.Take(5).ToList();
        TrendingTags = activeArticles
            .SelectMany(n => n.Tags)
            .Where(t => !string.IsNullOrWhiteSpace(t.TagName))
            .GroupBy(t => t.TagName!)
            .OrderByDescending(g => g.Count())
            .ThenBy(g => g.Key)
            .Take(12)
            .Select(g => g.Key)
            .ToList();
    }
}
