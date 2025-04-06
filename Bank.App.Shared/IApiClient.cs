using Bank.Logic.Models;

public interface IApiClient
{
    Task<List<Account>> GetAccountsAsync();
    Task<Account> GetAccountAsync(int accountId);
    Task CreateAccountAsync();
    Task DepositAsync(int accountId, double amount);
    Task WithdrawAsync(int accountId, double amount);
    Task DeleteAccountAsync(int accountId);
    Task UpdateNicknameAsync(int accountId, string nickname);
}
