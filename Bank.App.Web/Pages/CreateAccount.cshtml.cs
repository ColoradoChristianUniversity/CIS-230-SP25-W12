using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Bank.App.Shared;
using System.Threading.Tasks;

namespace Bank.App.Web.Pages
{
    public class CreateAccountModel : PageModel
    {
        private readonly IApiClient _apiClient;

        public CreateAccountModel(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _apiClient.CreateAccountAsync();
            return RedirectToPage("Index");
        }
    }
}
