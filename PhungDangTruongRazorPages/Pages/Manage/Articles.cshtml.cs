using BusinessObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using PhungDangTruongRazorPages.Hubs;
using PhungDangTruongRazorPages.Models;
using Services;

namespace PhungDangTruongRazorPages.Pages.Manage;

/// <summary>
/// Staff: manage news articles (with tags). Every create/update/delete is pushed to all
/// connected browsers through the SignalR <see cref="NewsHub"/> (real-time communication).
/// </summary>
[Authorize(Roles = "Staff")]
public class ArticlesModel : PageModel
{
    private const string FormView = "/Pages/Manage/_Form.cshtml";
    private const string PagePath = "/Manage/Articles";

    private readonly INewsArticleService _news;
    private readonly ICategoryService _categories;
    private readonly ITagService _tags;
    private readonly IHubContext<NewsHub> _hub;

    public ArticlesModel(INewsArticleService news, ICategoryService categories, ITagService tags, IHubContext<NewsHub> hub)
    {
        _news = news;
        _categories = categories;
        _tags = tags;
        _hub = hub;
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
        Articles = _news.Search(Keyword, CategoryId, Status, activeOnly: false, createdById: null);
    }

    public IActionResult OnGetDetails(string id)
    {
        var article = _news.GetById(id);
        return article == null ? NotFound() : this.ModalPartial("/Pages/Manage/_Details.cshtml", article);
    }

    public IActionResult OnGetForm(string? id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return this.ModalPartial(FormView, WithLists(new NewsArticle { NewsStatus = true }, new List<int>()));
        }
        var article = _news.GetById(id);
        if (article == null)
        {
            return NotFound();
        }
        return this.ModalPartial(FormView, WithLists(article, article.Tags.Select(t => t.TagId).ToList()));
    }

    public async Task<IActionResult> OnPostSaveAsync(NewsArticle model, List<int>? selectedTagIds)
    {
        selectedTagIds ??= new List<int>();
        bool isNew = string.IsNullOrEmpty(model.NewsArticleId);
        if (isNew)
        {
            ModelState.Remove(nameof(NewsArticle.NewsArticleId));
        }
        if (ModelState.IsValid)
        {
            try
            {
                if (isNew)
                {
                    _news.Add(model, selectedTagIds, User.GetAccountId());
                }
                else
                {
                    _news.Update(model, selectedTagIds, User.GetAccountId());
                }
                await Broadcast(isNew ? "created" : "updated", model.NewsArticleId, model.NewsTitle);
                return new JsonResult(new { success = true });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
        }
        return this.ModalPartial(FormView, WithLists(model, selectedTagIds));
    }

    public IActionResult OnGetDeleteForm(string id)
    {
        var article = _news.GetById(id);
        if (article == null)
        {
            return NotFound();
        }
        return this.ConfirmDelete(new ConfirmDeleteViewModel
        {
            Page = PagePath,
            Id = id,
            Message = $"Are you sure you want to delete the news article \"{article.NewsTitle}\"?"
        });
    }

    public async Task<IActionResult> OnPostDeleteAsync(string id)
    {
        try
        {
            var title = _news.GetById(id)?.NewsTitle;
            _news.Delete(id);
            await Broadcast("deleted", id, title);
            return new JsonResult(new { success = true });
        }
        catch (InvalidOperationException ex)
        {
            return this.ConfirmDelete(new ConfirmDeleteViewModel
            {
                Page = PagePath,
                Id = id,
                Message = "Delete this news article?",
                Error = ex.Message
            });
        }
    }

    private Task Broadcast(string action, string articleId, string? title)
        => _hub.Clients.All.SendAsync(NewsHub.NewsChanged, action, articleId, title ?? "", User.Identity?.Name ?? "");

    private NewsArticle WithLists(NewsArticle article, List<int> selectedTagIds)
    {
        var categories = _categories.GetActive();
        if (article.CategoryId.HasValue && categories.All(c => c.CategoryId != article.CategoryId))
        {
            var current = _categories.GetById(article.CategoryId.Value);
            if (current != null)
            {
                categories.Add(current);
            }
        }
        ViewData["Categories"] = categories;
        ViewData["Tags"] = _tags.GetAll();
        ViewData["SelectedTagIds"] = selectedTagIds;
        return article;
    }
}
