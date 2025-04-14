using Microsoft.AspNetCore.Mvc.RazorPages;
using Bank.Logic.Models;
using Bank.App.Shared;  // The namespace for IApiClient
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bank.App.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IApiClient _apiClient;
        public IEnumerable<Account> Accounts { get; set; } = new List<Account>();

        public IndexModel(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task OnGetAsync()
        {
            Accounts = await _apiClient.GetAccountsAsync();
        }
    }
}
