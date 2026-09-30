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

    public void OnGet()
    {
        Categories = _categories.GetActive();
        Articles = _news.Search(Keyword, CategoryId, null, activeOnly: true, createdById: null);
    }
}
