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
        return await WrapperAsync(Do);

        IResult Do()
        {
            if (!Enum.TryParse<TransactionType>(type, true, out var transactionType))
                return Results.BadRequest("Invalid transaction type.");

            var account = Storage.GetAccount(accountId) ?? throw new ArgumentException("Account not found.");
            var transaction = new Transaction
            {
                Type = transactionType,
                Amount = amount,
                Date = DateTime.UtcNow
            };

            if (!account.TryAddTransaction(transaction))
                return Results.BadRequest("Transaction rejected by account rules.");

            Storage.SaveChanges(); // This will be fixed in Step 2
            return Results.Ok(transaction);
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
            Storage.RemoveAccount(accountId);
            return Results.Ok();
        }
    }

    public async Task<IResult> DepositAsync(int accountId, double amount)
    {
        return await WrapperAsync(Do);

        IResult Do()
        {
            var account = Storage.GetAccount(accountId) ?? throw new ArgumentException("Account not found.");
            var transaction = new Transaction
            {
                Type = TransactionType.Deposit,
                Amount = amount,
                Date = DateTime.UtcNow
            };

            if (!account.TryAddTransaction(transaction))
                return Results.BadRequest("Deposit rejected (e.g., zero or negative amount).");

            Storage.SaveChanges();
            return Results.Ok(transaction);
        }
    }

    public async Task<IResult> GetAccountAsync(int accountId)
    {
        return await WrapperAsync(Do);

        IResult Do()
        {
            var account = Storage.GetAccount(accountId) ?? throw new ArgumentException("Account not found.");
            return Results.Ok(account);
        }
    }

    public async Task<IResult> GetDefaultSettingsAsync()
    {
        return await WrapperAsync(Do);

        IResult Do()
        {
            return Results.Ok(new AccountSettings());
        }
    }

    public async Task<IResult> GetTransactionHistoryAsync(int accountId)
    {
        return await WrapperAsync(Do);

        IResult Do()
        {
            var account = Storage.GetAccount(accountId) ?? throw new ArgumentException("Account not found.");
            return Results.Ok(account.GetTransactions());
        }
    }

    public async Task<IResult> ListAccountsAsync()
    {
        return await WrapperAsync(Do);

        IResult Do()
        {
            var accounts = Storage.ListAccounts();
            return Results.Ok(accounts);
        }
    }

    public async Task<IResult> WithdrawAsync(int accountId, double amount)
    {
        return await WrapperAsync(Do);

        IResult Do()
        {
            var account = Storage.GetAccount(accountId) ?? throw new ArgumentException("Account not found.");
            var transaction = new Transaction
            {
                Type = TransactionType.Withdraw,
                Amount = -amount,
                Date = DateTime.UtcNow
            };

            if (!account.TryAddTransaction(transaction))
                return Results.BadRequest("Withdrawal rejected (e.g., insufficient funds or zero amount).");

            Storage.SaveChanges();
            return Results.Ok(transaction);
        }
    }

    private static async Task<IResult> WrapperAsync(Func<IResult> action)
    {
        try
        {
            return await Task.Run(action);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Results.Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            return Results.Problem($"An error occurred: {ex.Message}");
        }
    }
}