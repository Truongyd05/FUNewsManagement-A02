using BusinessObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using PhungDangTruongRazorPages.Models;
using Services;

namespace PhungDangTruongRazorPages.Pages.Accounts;

/// <summary>Admin: manage account information.</summary>
[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private const string FormView = "/Pages/Accounts/_Form.cshtml";
    private const string PagePath = "/Accounts/Index";

    private readonly ISystemAccountService _accounts;
    private readonly AdminSettings _admin;

    public IndexModel(ISystemAccountService accounts, IOptions<AdminSettings> admin)
    {
        _accounts = accounts;
        _admin = admin.Value;
    }

    [BindProperty(SupportsGet = true)]
    public string? Keyword { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? Role { get; set; }

    public List<SystemAccount> Accounts { get; set; } = new();

    public void OnGet()
    {
        Accounts = _accounts.Search(Keyword, Role);
    }

    public IActionResult OnGetForm(short? id)
    {
        if (id == null)
        {
            return this.ModalPartial(FormView, new SystemAccount { AccountRole = AccountRole.Staff });
        }
        var account = _accounts.GetById(id.Value);
        return account == null ? NotFound() : this.ModalPartial(FormView, account);
    }

    public IActionResult OnPostSave(SystemAccount model)
    {
        bool isNew = model.AccountId == 0;
        if (isNew)
        {
            ModelState.Remove(nameof(SystemAccount.AccountId));
        }
        if (string.Equals(model.AccountEmail?.Trim(), _admin.Email, StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(nameof(SystemAccount.AccountEmail), "This email is reserved.");
        }
        if (ModelState.IsValid)
        {
            try
            {
                if (isNew) _accounts.Add(model); else _accounts.Update(model);
                return new JsonResult(new { success = true });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
        }
        return this.ModalPartial(FormView, model);
    }

    public IActionResult OnGetDeleteForm(short id)
    {
        var account = _accounts.GetById(id);
        if (account == null)
        {
            return NotFound();
        }
        return this.ConfirmDelete(new ConfirmDeleteViewModel
        {
            Page = PagePath,
            Id = id.ToString(),
            Message = $"Are you sure you want to delete the account \"{account.AccountName}\" ({account.AccountEmail})?"
        });
    }

    public IActionResult OnPostDelete(short id)
    {
        try
        {
            _accounts.Delete(id);
            return new JsonResult(new { success = true });
        }
        catch (InvalidOperationException ex)
        {
            return this.ConfirmDelete(new ConfirmDeleteViewModel
            {
                Page = PagePath,
                Id = id.ToString(),
                Message = "Delete this account?",
                Error = ex.Message
            });
        }
    }
}
