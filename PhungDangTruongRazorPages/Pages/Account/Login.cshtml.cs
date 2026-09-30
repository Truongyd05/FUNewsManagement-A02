using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using BusinessObjects;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using PhungDangTruongRazorPages.Models;
using Services;

namespace PhungDangTruongRazorPages.Pages.Account;

public class LoginModel : PageModel
{
    private readonly ISystemAccountService _accounts;
    private readonly AdminSettings _admin;

    public LoginModel(ISystemAccountService accounts, IOptions<AdminSettings> admin)
    {
        _accounts = accounts;
        _admin = admin.Value;
    }

    [BindProperty]
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; } = "";

    [BindProperty]
    [Required(ErrorMessage = "Password is required.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    public IActionResult OnGet()
    {
        return User.Identity?.IsAuthenticated == true ? RedirectToHome(User.GetRole()) : Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        string id, name, role;
        var email = Email.Trim();
        if (string.Equals(email, _admin.Email, StringComparison.OrdinalIgnoreCase) && Password == _admin.Password)
        {
            id = "0";
            name = "Administrator";
            role = "Admin";
        }
        else
        {
            var account = _accounts.Login(email, Password);
            if (account == null)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return Page();
            }
            id = account.AccountId.ToString();
            name = account.AccountName ?? account.AccountEmail ?? "";
            role = AccountRole.GetName(account.AccountRole);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, id),
            new(ClaimTypes.Name, name),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, role)
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        return RedirectToHome(role);
    }

    public async Task<IActionResult> OnPostLogoutAsync()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToPage("/Account/Login");
    }

    private IActionResult RedirectToHome(string? role) => role switch
    {
        "Admin" => RedirectToPage("/Accounts/Index"),
        "Staff" => RedirectToPage("/Manage/Articles"),
        _ => RedirectToPage("/News/Index")
    };
}
