using BusinessObjects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services;

namespace PhungDangTruongRazorPages.Pages.News;

public class DetailsModel : PageModel
{
    private readonly INewsArticleService _news;

    public DetailsModel(INewsArticleService news)
    {
        _news = news;
    }

    public NewsArticle Article { get; set; } = null!;

    public IActionResult OnGet(string id)
    {
        var article = _news.GetById(id);
        if (article == null || article.NewsStatus != true)
        {
            return NotFound();
        }
        Article = article;
        return Page();
    }
}
