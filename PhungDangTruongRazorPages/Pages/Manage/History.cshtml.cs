using BusinessObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PhungDangTruongRazorPages.Models;
using Services;

namespace PhungDangTruongRazorPages.Pages.Manage;

/// <summary>Staff: news created by the signed-in staff member.</summary>
[Authorize(Roles = "Staff")]
public class HistoryModel : PageModel
{
    private readonly INewsArticleService _news;
    private readonly ICategoryService _categories;

    public HistoryModel(INewsArticleService news, ICategoryService categories)
    {
        _news = news;
        _categories = categories;
    }

    [BindProperty(SupportsGet = true)]
    public string? Keyword { get; set; }

    [BindProperty(SupportsGet = true)]
    public short? CategoryId { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool? Status { get; set; }

    public List<NewsArticle> Articles { get; set; } = new();

    public List<Category> Categories { get; set; } = new();

    public void OnGet()
    {
        Categories = _categories.GetAll();
        Articles = _news.Search(Keyword, CategoryId, Status, activeOnly: false, createdById: User.GetAccountId());
    }
}
