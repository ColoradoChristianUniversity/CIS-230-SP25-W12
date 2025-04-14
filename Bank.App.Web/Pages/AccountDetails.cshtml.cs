using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Bank.Logic.Models;
using Bank.App.Shared;
using System.Threading.Tasks;

namespace Bank.App.Web.Pages
{
    public class AccountDetailsModel : PageModel
    {
        private readonly IApiClient _apiClient;

        [BindProperty(SupportsGet = true)]
        public int id { get; set; }

        // These properties will capture form input.
        [BindProperty]
        public double DepositAmount { get; set; }
        
        [BindProperty]
        public double WithdrawAmount { get; set; }

        public Account Account { get; set; } = new Account();

        public AccountDetailsModel(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                Account = await _apiClient.GetAccountAsync(id);
                return Page();
            }
            catch
            {
                return RedirectToPage("Error");
            }
        }

        public async Task<IActionResult> OnPostDepositAsync()
        {
            await _apiClient.DepositAsync(id, DepositAmount);
            return RedirectToPage(new { id = id });
        }

        public async Task<IActionResult> OnPostWithdrawAsync()
        {
            await _apiClient.WithdrawAsync(id, WithdrawAmount);
            return RedirectToPage(new { id = id });
        }

        public async Task<IActionResult> OnPostDeleteAsync()
        {
            await _apiClient.DeleteAccountAsync(id);
            return RedirectToPage("Index");
        }
    }
}
