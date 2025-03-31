using Bank.Logic;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Bank.Api.Logic;

public class EndpointHandler : IEndpointHandler
{
    public Storage Storage { get; }

    public EndpointHandler(string filePath)
    {
        Storage = new Storage(filePath);
    }

    public async Task<IResult> CreateAccountAsync()
    {
        return await WrapperAsync(() =>
        {
            var account = new Account
            {
                Id = Storage.ListAccounts().Length > 0 ? Storage.ListAccounts().Max() + 1 : 1,
                Settings = new AccountSettings()
            };
            Storage.AddAccount(account);
            return Task.FromResult<IResult>(TypedResults.Ok(account));
        });
    }

    public async Task<IResult> GetAccountAsync(int accountId)
    {
        return await WrapperAsync(() =>
        {
            var account = Storage.GetAccount(accountId);
            if (account == null)
            {
                return Task.FromResult<IResult>(TypedResults.BadRequest("Account not found."));
            }
            return Task.FromResult<IResult>(TypedResults.Ok(account));
        });
    }

    public async Task<IResult> DepositAsync(int accountId, double amount)
    {
        return await WrapperAsync(() =>
        {
            if (amount <= 0)
            {
                return Task.FromResult<IResult>(TypedResults.BadRequest("Deposit amount must be positive."));
            }
            var account = Storage.GetAccount(accountId);
            if (account == null)
            {
                return Task.FromResult<IResult>(TypedResults.BadRequest("Account not found."));
            }
            var transaction = new Transaction
            {
                Type = TransactionType.Deposit,
                Amount = amount,
                Date = DateTime.Now
            };
            if (account.Transactions == null)
            {
                account.Transactions = new List<Bank.Logic.Abstractions.ITransaction>();
            }
            if (!account.TryAddTransaction(transaction))
            {
                return Task.FromResult<IResult>(TypedResults.BadRequest("Deposit rejected."));
            }
            Storage.AddAccount(account);
            return Task.FromResult<IResult>(TypedResults.Ok(transaction));
        });
            }
            Storage.AddAccount(account);
            return Task.FromResult<IResult>(TypedResults.Ok(transaction));
        });
    }

    public async Task<IResult> WithdrawAsync(int accountId, double amount)
    {
        return await WrapperAsync(() =>
        {
            var account = Storage.GetAccount(accountId);
            if (account == null)
            {
                return Task.FromResult<IResult>(TypedResults.BadRequest("Account not found."));
            }
            var transaction = new Transaction
            {
                Type = TransactionType.Withdraw,
                Amount = -amount,
                Date = DateTime.Now
            };
            if (account.Transactions == null)
            {
                account.Transactions = new List<Bank.Logic.Abstractions.ITransaction>();
            }
            if (!account.TryAddTransaction(transaction))
            {
                return Task.FromResult<IResult>(TypedResults.BadRequest("Withdrawal rejected."));
            }
            Storage.AddAccount(account);
            return Task.FromResult<IResult>(TypedResults.Ok(transaction));
        });
    }

    public async Task<IResult> ListAccountsAsync()
    {
        return await WrapperAsync(() =>
        {
            return Task.FromResult<IResult>(TypedResults.Ok(Storage.ListAccounts()));
        });
    }

    public async Task<IResult> DeleteAccountAsync(int accountId)
    {
        return await WrapperAsync(() =>
        {
            Storage.RemoveAccount(accountId);
            return Task.FromResult<IResult>(TypedResults.Ok());
        });
    }

    public async Task<IResult> AddTransactionAsync(int accountId, string type, double amount)
    {
        return await WrapperAsync(() =>
        {
            if (!Enum.TryParse<TransactionType>(type, true, out var transactionType))
            {
                return Task.FromResult<IResult>(TypedResults.BadRequest("Invalid transaction type."));
            }
            var account = Storage.GetAccount(accountId);
            if (account == null)
            {
                return Task.FromResult<IResult>(TypedResults.BadRequest("Account not found."));
            }
            var transaction = new Transaction
            {
                Type = transactionType,
                Amount = amount,
                Date = DateTime.Now
            };
            if (account.Transactions == null)
            {
                account.Transactions = new List<Bank.Logic.Abstractions.ITransaction>();
            }
            if (!account.TryAddTransaction(transaction))
            {
                return Task.FromResult<IResult>(TypedResults.BadRequest("Transaction rejected."));
            }
            Storage.AddAccount(account);
            return Task.FromResult<IResult>(TypedResults.Ok(transaction));
        });
    }

    public async Task<IResult> GetDefaultSettingsAsync()
    {
        return await WrapperAsync(() =>
        {
            return Task.FromResult<IResult>(TypedResults.Ok(new AccountSettings()));
        });
    }

    public async Task<IResult> GetTransactionHistoryAsync(int accountId)
    {
        return await WrapperAsync(() =>
        {
            var account = Storage.GetAccount(accountId);
            if (account == null)
            {
                return Task.FromResult<IResult>(TypedResults.BadRequest("Account not found."));
            }
            if (account.Transactions == null)
            {
                account.Transactions = new List<Bank.Logic.Abstractions.ITransaction>();
            }
            return Task.FromResult<IResult>(TypedResults.Ok(account.Transactions));
        });
    }

    private static async Task<IResult> WrapperAsync(Func<Task<IResult>> func)
    {
        try
        {
            return await func();
        }
        catch (ArgumentException ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return TypedResults.BadRequest($"An unexpected error occurred: {ex.Message}");
        }
    }
}