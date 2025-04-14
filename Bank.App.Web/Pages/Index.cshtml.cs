using Bank.Logic.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Bank.App.Web.Pages;

public class IndexModel : PageModel
{
    
    private readonly IApiClient _apiClient;

    public IndexModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public List<Account> Accounts { get; set; } = new();
    public Account? SelectedAccount { get; set; }

    public async Task OnGetAsync()
    {
        Accounts = await _apiClient.GetAccountsAsync();
    }

    public async Task<IActionResult> OnPostCreateAccountAsync()
    {
        await _apiClient.CreateAccountAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSelectAccountAsync(int id)
    {
        SelectedAccount = await _apiClient.GetAccountAsync(id);
        return Page();
    }

    public async Task<IActionResult> OnPostDepositAsync(int selectedAccountId, double amount)
    {
        SelectedAccount = await _apiClient.GetAccountAsync(selectedAccountId);
        if (SelectedAccount != null)
        {
            await _apiClient.DepositAsync(SelectedAccount.Id, amount);
            SelectedAccount = await _apiClient.GetAccountAsync(SelectedAccount.Id); // Refresh account data
        }
        return Page();
    }

    public async Task<IActionResult> OnPostWithdrawAsync(int selectedAccountId, double amount)
    {
        SelectedAccount = await _apiClient.GetAccountAsync(selectedAccountId);
        if (SelectedAccount != null)
        {
            await _apiClient.WithdrawAsync(SelectedAccount.Id, amount);
            SelectedAccount = await _apiClient.GetAccountAsync(SelectedAccount.Id); // Refresh account data
        }
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAccountAsync(int selectedAccountId)
    {
        SelectedAccount = await _apiClient.GetAccountAsync(selectedAccountId);
        if (SelectedAccount != null)
        {
            await _apiClient.DeleteAccountAsync(SelectedAccount.Id);
            SelectedAccount = null;
        }

        // Refresh the account list
        Accounts = await _apiClient.GetAccountsAsync();
        return RedirectToPage();
    }

    public IActionResult OnPostBack()
    {
        SelectedAccount = null;
        return RedirectToPage();
    }
}