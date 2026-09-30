using System.Security.Claims;
using BusinessObjects;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using PhungDangTruongRazorPages.Models;
using Services;

namespace PhungDangTruongRazorPages.Pages.Account;

/// <summary>Staff: manage own profile.</summary>
[Authorize(Roles = "Staff")]
public class ProfileModel : PageModel
{
    private const string FormView = "/Pages/Account/_ProfileForm.cshtml";

    private readonly ISystemAccountService _accounts;
    private readonly AdminSettings _admin;

    public ProfileModel(ISystemAccountService accounts, IOptions<AdminSettings> admin)
    {
        _accounts = accounts;
        _admin = admin.Value;
    }

    public SystemAccount? Account { get; set; }

    public IActionResult OnGet()
    {
        Account = _accounts.GetById(User.GetAccountId());
        return Account == null ? NotFound() : Page();
    }

    public IActionResult OnGetForm()
    {
        var account = _accounts.GetById(User.GetAccountId());
        return account == null ? NotFound() : this.ModalPartial(FormView, account);
    }

    public async Task<IActionResult> OnPostSaveAsync(SystemAccount model)
    {
        var current = _accounts.GetById(User.GetAccountId());
        if (current == null)
        {
            return NotFound();
        }
        // A staff member cannot change id or role.
        model.AccountId = current.AccountId;
        model.AccountRole = current.AccountRole;
        ModelState.Remove(nameof(SystemAccount.AccountRole));

        if (string.Equals(model.AccountEmail?.Trim(), _admin.Email, StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(nameof(SystemAccount.AccountEmail), "This email is reserved.");
        }
        if (ModelState.IsValid)
        {
            try
            {
                _accounts.Update(model);
                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, model.AccountId.ToString()),
                    new(ClaimTypes.Name, model.AccountName ?? ""),
                    new(ClaimTypes.Email, model.AccountEmail ?? ""),
                    new(ClaimTypes.Role, "Staff")
                };
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));
                return new JsonResult(new { success = true });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
        }
        return this.ModalPartial(FormView, model);
    }
}
