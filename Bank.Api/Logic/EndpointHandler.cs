using Bank.Logic; // For TransactionType, Transaction, AccountSettings
using Bank.Logic.Abstractions; // For ITransaction (implemented by Transaction)

namespace Bank.Api.Logic;

public class EndpointHandler : IEndpointHandler
{
    public Storage Storage { get; }

    public EndpointHandler(string? filePath = null)
    {
        Storage = new Storage(filePath ?? "store.json");
    }

    public async Task<IResult> AddTransactionAsync(int accountId, string type, double amount)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult> CreateAccountAsync()
    {
        return await WrapperAsync(Do);

        IResult Do()
        {
            return Results.Ok(Storage.AddAccount());
        }
    }

    public async Task<IResult> DeleteAccountAsync(int accountId)
    {
        return await WrapperAsync(Do);

        IResult Do()
        {
            var account = Storage.GetAccount(accountId);
            if (account == null)
            {
                return Results.NotFound();
            }

            Storage.RemoveAccount(accountId);
            return Results.Ok();
        }
    }

    public async Task<IResult> DepositAsync(int accountId, double amount)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult> GetAccountAsync(int accountId)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult> GetDefaultSettingsAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<IResult> GetTransactionHistoryAsync(int accountId)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult> ListAccountsAsync()
    {
        return await WrapperAsync(Do);

        IResult Do()
        {
            var accounts = Storage.GetAllAccounts();
            return Results.Ok(accounts);
        }
    }

    public async Task<IResult> WithdrawAsync(int accountId, double amount)
    {
        throw new NotImplementedException();
    }

    private static async Task<IResult> WrapperAsync(Func<IResult> action)
    {
        try
        {
            return await Task.Run(action);
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Results.Problem($"An error occurred: {ex.Message}");
        }
    }
    private static async Task<IResult> WrapperAsync(Func<Task<IResult>> action)
    {
        try
        {
            return await action();
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Results.Problem($"An error occurred: {ex.Message}");
        }
    }
}