using Bank.Logic;

namespace Bank.Api.Logic;

public class EndpointHandler : IEndpointHandler
{
    public Storage Storage { get; }

    public EndpointHandler(string? filePath = null)
    {
        Storage = new Storage(filePath ?? "store.json");
    }
    // TODO: Added async tag, will this mess everything up?
    public async Task<IResult> AddTransactionAsync(int accountId, string type, double amount)
    {
        return await WrapperAsync(Do);

        IResult Do()
        {
            var account = Storage.GetAccount(accountId);

            if (account == null){
                return Results.NotFound($"Account {accountId} not found");
            }

            // Validate the transaction type
            if (!Enum.TryParse<TransactionType>(type, true, out var transactionType) || transactionType == TransactionType.Unknown)
            {
                return Results.BadRequest($"Invalid transaction type: {type}");
            }

            Storage.AddTransaction(account, amount, transactionType);

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
            Storage.RemoveAccount(accountId);
            return Results.Ok();
        }
    }

    public async Task<IResult> DepositAsync(int accountId, double amount)
    {
        return await WrapperAsync(Do);

        IResult Do()
        {
            var account = Storage.GetAccount(accountId);
            
            if (account == null){
                return Results.NotFound($"Account {accountId} not found");
            }

            double balance = account.GetBalance();

            Storage.AddTransaction(account, amount, TransactionType.Deposit);

            return Results.Ok();
        }
    }

    public async Task<IResult> GetAccountAsync(int accountId)
    {
        return await WrapperAsync(Do);

        IResult Do()
        {
        var account = Storage.GetAccount(accountId);

        if (account == null){
            return Results.NotFound($"Account {accountId} not found");
        }

        return Results.Ok(account);
        }
    }

    public async Task<IResult> GetDefaultSettingsAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<IResult> GetTransactionHistoryAsync(int accountId)
    {
        return await WrapperAsync(Do);

        IResult Do()
        {
            var account = Storage.GetAccount(accountId);

            if (account == null){
                return Results.NotFound($"Account {accountId} not found");
            }

            var transactions = Storage.GetTransactions(accountId);

            return Results.Json(transactions);
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
            var account = Storage.GetAccount(accountId);
            
            if (account == null){
                return Results.NotFound($"Account {accountId} not found");
            }

            double balance = account.GetBalance();

            // Check sufficient funds
            if (balance < amount)
            {
                return Results.BadRequest("Insufficient funds");
            }

            Storage.AddTransaction(account, amount, TransactionType.Withdraw);

            return Results.Ok();
        }

    }

    public static async Task<IResult> WrapperAsync(Func<IResult> action)
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
