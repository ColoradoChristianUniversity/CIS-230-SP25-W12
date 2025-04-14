using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Bank.Logic.Models;
using System;

public class AccountMenuModel : PageModel
{
    private readonly IApiClient _apiClient;

    public AccountMenuModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public Account Account { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        Account = await _apiClient.GetAccountAsync(Id);
        if (Account == null)
        {
            return NotFound();
        }

        return Page();
    }

    // LS ADDITIONS

    // public async Task<IActionResult> OnPostDepositAsync()
    // {
    //     Console.WriteLine("Deposit button clicked.");
    //     // Logic for handling deposits
    // }

    // public async Task<IActionResult> OnPostWithdrawAsync()
    // {
    //     Console.WriteLine("Withdraw button clicked.");
    //     // Logic for handling withdrawals
    // }
}