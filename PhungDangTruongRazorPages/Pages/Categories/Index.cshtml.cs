using BusinessObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PhungDangTruongRazorPages.Models;
using Services;

namespace PhungDangTruongRazorPages.Pages.Categories;

/// <summary>Staff: manage category information.</summary>
[Authorize(Roles = "Staff")]
public class IndexModel : PageModel
{
    private const string FormView = "/Pages/Categories/_Form.cshtml";
    private const string PagePath = "/Categories/Index";

    private readonly ICategoryService _categories;

    public IndexModel(ICategoryService categories)
    {
        _categories = categories;
    }

    [BindProperty(SupportsGet = true)]
    public string? Keyword { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool? IsActive { get; set; }

    public List<Category> Items { get; set; } = new();

    public Dictionary<short, int> Counts { get; set; } = new();

    public void OnGet()
    {
        Counts = _categories.CountArticlesPerCategory();
        Items = _categories.Search(Keyword, IsActive);
    }

    public IActionResult OnGetForm(short? id)
    {
        if (id == null)
        {
            return this.ModalPartial(FormView, WithParents(new Category { IsActive = true }));
        }
        var category = _categories.GetById(id.Value);
        return category == null ? NotFound() : this.ModalPartial(FormView, WithParents(category));
    }

    public IActionResult OnPostSave(Category model)
    {
        bool isNew = model.CategoryId == 0;
        if (isNew)
        {
            ModelState.Remove(nameof(Category.CategoryId));
        }
        if (ModelState.IsValid)
        {
            try
            {
                if (isNew) _categories.Add(model); else _categories.Update(model);
                return new JsonResult(new { success = true });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
        }
        return this.ModalPartial(FormView, WithParents(model));
    }

    public IActionResult OnGetDeleteForm(short id)
    {
        var category = _categories.GetById(id);
        if (category == null)
        {
            return NotFound();
        }
        return this.ConfirmDelete(new ConfirmDeleteViewModel
        {
            Page = PagePath,
            Id = id.ToString(),
            Message = $"Are you sure you want to delete the category \"{category.CategoryName}\"?",
            Error = _categories.IsInUse(id)
                ? "Cannot delete this category: it is already used (by news articles or as a parent category)."
                : null
        });
    }

    public IActionResult OnPostDelete(short id)
    {
        try
        {
            _categories.Delete(id);
            return new JsonResult(new { success = true });
        }
        catch (InvalidOperationException ex)
        {
            return this.ConfirmDelete(new ConfirmDeleteViewModel
            {
                Page = PagePath,
                Id = id.ToString(),
                Message = "Delete this category?",
                Error = ex.Message
            });
        }
    }

    private Category WithParents(Category category)
    {
        ViewData["Parents"] = _categories.GetAll().Where(c => c.CategoryId != category.CategoryId).ToList();
        return category;
    }
}
