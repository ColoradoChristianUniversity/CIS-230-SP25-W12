public interface IApiClient
{
    Task<List<AccountDto>> GetAccountsAsync();
    Task<AccountDto> GetAccountAsync(int id);
    Task CreateAccountAsync();
    Task DepositAsync(int accountId, double amount);
    Task WithdrawAsync(int accountId, double amount);
}


