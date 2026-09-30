using System.ComponentModel.DataAnnotations;
using BusinessObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services;

namespace PhungDangTruongRazorPages.Pages.Reports;

/// <summary>Admin: statistic report by created-date period, newest first.</summary>
[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly INewsArticleService _news;

    public IndexModel(INewsArticleService news)
    {
        _news = news;
    }

    [BindProperty(SupportsGet = true)]
    [Display(Name = "Start Date")]
    public DateTime? StartDate { get; set; }

    [BindProperty(SupportsGet = true)]
    [Display(Name = "End Date")]
    public DateTime? EndDate { get; set; }

    public List<NewsArticle>? Articles { get; set; }

    public void OnGet()
    {
        var hasPeriod = StartDate.HasValue && EndDate.HasValue;
        var today = DateTime.Today;
        StartDate ??= new DateTime(today.Year, today.Month, 1);
        EndDate ??= today;

        if (hasPeriod)
        {
            try
            {
                Articles = _news.GetReport(StartDate.Value, EndDate.Value);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
        }
    }
}
