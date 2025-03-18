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
            var accounts = Storage.ListAccounts();
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
