namespace Bank.App.Shared; 

using Bank.Logic.Models;

public interface IApiClient
{
    Task<List<Account>> GetAccountsAsync();
    Task<Account> GetAccountAsync(int id);
    Task CreateAccountAsync();
    Task DepositAsync(int accountId, double amount);
    Task WithdrawAsync(int accountId, double amount);
    Task DeleteAccountAsync(int accountId); // ← added
}
