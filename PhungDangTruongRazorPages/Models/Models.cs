using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace PhungDangTruongRazorPages.Models;

public class AdminSettings
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}

public class ConfirmDeleteViewModel
{
    public string Page { get; set; } = "";
    public string Id { get; set; } = "";
    public string Message { get; set; } = "";
    public string? Error { get; set; }
}

public static class UserExtensions
{
    public static short GetAccountId(this ClaimsPrincipal user)
        => short.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : (short)0;

    public static string? GetRole(this ClaimsPrincipal user) => user.FindFirstValue(ClaimTypes.Role);
}

public static class PageModelExtensions
{
    /// <summary>Renders a partial view (used for popup dialogs), keeping ModelState for validation messages.</summary>
    public static PartialViewResult ModalPartial<T>(this PageModel page, string viewName, T model)
        => new()
        {
            ViewName = viewName,
            ViewData = new ViewDataDictionary<T>(page.ViewData, model)
        };

    public static PartialViewResult ConfirmDelete(this PageModel page, ConfirmDeleteViewModel model)
        => page.ModalPartial("/Pages/Shared/_ConfirmDelete.cshtml", model);
}
