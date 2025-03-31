namespace Bank.Api.Logic;

public class EndpointHandler : IEndpointHandler
{
    public Storage Storage { get; }

    public EndpointHandler(string? filePath = null)
    {
        Storage = new Storage(filePath ?? "store.json");
    }

    public Task<IResult> AddTransactionAsync(int accountId, string type, double amount)
    {
        return WrapperAsync(Do);

        IResult Do()
        {
            if (amount <= 0)
            {
                throw new ArgumentException("Transaction amount must be greater than zero.");
            }

            if (type != "deposit" && type != "withdraw")
            {
                throw new ArgumentException("Transaction type must be either 'deposit' or 'withdraw'.");
            }

            var account = Storage.GetAccount(accountId);
            if (account == null)
            {
                throw new InvalidOperationException("Account not found.");
            }

            Storage.AddTransaction(accountId, type, amount);
            return Results.Ok();
        }
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
        return await WrapperAsync(Do);

        IResult Do()
        {
            if (amount <= 0)
            {
                throw new ArgumentException("Transaction amount must be greater than zero.");
            }

            var account = Storage.GetAccount(accountId);
            if (account == null)
            {
                throw new ArgumentException("Account not found.");
            }

            Storage.AddTransaction(accountId, "deposit", amount);
            return Results.Ok();
        }
    }

    public async Task<IResult> GetAccountAsync(int accountId)
    {
        return await WrapperAsync(Do);

        IResult Do()
        {
            var account = Storage.GetAccount(accountId);
            if (account == null)
            {
                return Results.NotFound();
            }
            return Results.Ok(account);
        }
    }

    public async Task<IResult> GetDefaultSettingsAsync()
    {
        return await WrapperAsync(Do);

        IResult Do()
        {
            var settings = Storage.GetDefaultSettings();
            return Results.Ok(settings);
        }
    }

    public async Task<IResult> GetTransactionHistoryAsync(int accountId)
    {
        return await WrapperAsync(Do);

        IResult Do()
        {
            var transactions = Storage.GetTransactionHistory(accountId);
            if (transactions == null || !transactions.Any())
            {
                return Results.NotFound();
            }
            return Results.Ok(transactions);
        }
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
        return await WrapperAsync(Do);

        IResult Do()
        {
            if (amount <= 0)
            {
                throw new ArgumentException("Withdrawal amount must be greater than zero.");
            }

            var account = Storage.GetAccount(accountId);
            if (account == null)
            {
                return Results.NotFound();
            }

            if (account.Balance < amount)
            {
                throw new InvalidOperationException("Insufficient funds.");
            }

            account.Balance -= amount;
            Storage.AddTransaction(accountId, "withdraw", amount);
            return Results.Ok();
        }
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
