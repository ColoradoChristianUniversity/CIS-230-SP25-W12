using Bank.Logic.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Bank.App.Web.Pages;

public class AccountModel : PageModel
{
    [BindProperty]
    public string Nickname { get; set; } = string.Empty;
    
    public Account? CurrentAccount { get; private set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        try
        {
            var apiClient = new BankApiClient(default, validateConnection: true);
            CurrentAccount = await apiClient.GetAccountAsync(id);
            Nickname = CurrentAccount.Nickname;
            return Page();
        }
        catch
        {
            return RedirectToPage("/Error");
        }
    }

    public async Task<IActionResult> OnPostUpdateNicknameAsync(int id)
    {
        try
        {
            var apiClient = new BankApiClient(default, validateConnection: true);
            await apiClient.UpdateNicknameAsync(id, Nickname);
            return RedirectToPage("/Account", new { id });
        }
        catch
        {
            return RedirectToPage("/Error");
        }
    }

    public async Task<IActionResult> OnPostDepositAsync(int id, double amount)
    {
        try
        {
            var apiClient = new BankApiClient(default, validateConnection: true);
            await apiClient.DepositAsync(id, amount);
            return RedirectToPage("/Account", new { id });
        }
        catch
        {
            return RedirectToPage("/Error");
        }
    }

    public async Task<IActionResult> OnPostWithdrawAsync(int id, double amount)
    {
        try
        {
            var apiClient = new BankApiClient(default, validateConnection: true);
            await apiClient.WithdrawAsync(id, amount);
            return RedirectToPage("/Account", new { id });
        }
        catch
        {
            return RedirectToPage("/Error");
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        try
        {
            var apiClient = new BankApiClient(default, validateConnection: true);
            await apiClient.DeleteAccountAsync(id);
            return RedirectToPage("/Index");
        }
        catch
        {
            return RedirectToPage("/Error");
        }
    }
}