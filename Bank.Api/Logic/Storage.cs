using System.Text.Json;
using Bank.Logic;

namespace Bank.Api.Logic;

public class Storage
{
    private const int firstId = 1;
    private readonly string path;
    private readonly List<Account> accounts;

    public Storage(string? fileName = "store.json")
    {
        path = Path.Combine(AppContext.BaseDirectory, fileName ?? "store.json");

        if (!File.Exists(path))
        {
            File.WriteAllText(path, "[]"); // Ensures an empty JSON array instead of just creating the file
        }

        try
        {
            accounts = JsonSerializer.Deserialize<List<Account>>(File.ReadAllText(path)) ?? new List<Account>();
        }
        catch (JsonException)
        {
            accounts = new List<Account>();
        }
    }

    public int[] ListAccounts()
    {
        return accounts.Select(a => a.Id).ToArray();
    }

    public Account AddAccount()
    {
        var newAccount = new Account
        {
            Id = GenerateNewAccountId(),
            Settings = new()
        };

        accounts.Add(newAccount);
        SaveChanges();
        return newAccount;

        int GenerateNewAccountId()
        {
            if (accounts.Count == 0)
            {
                return firstId;
            }

            return accounts.Max(a => a.Id) + 1;
        }
    }

    public Account? GetAccount(int id) => accounts.FirstOrDefault(a => a.Id == id);

    public void RemoveAccount(int id)
    {
        var account = GetAccount(id);
        if (account == null)
        {
            return;
        }

        accounts.Remove(account);
        SaveChanges();
    }

    public void AddTransaction(int accountId, string type, double amount)
    {
        var account = GetAccount(accountId);
        if (account == null)
        {
            throw new ArgumentException("Account not found.");
        }

        // Ensure the Transactions list is initialized
        if (account.Transactions == null)
        {
            account.Transactions = new List<Bank.Logic.Abstractions.ITransaction>();
        }

        var transaction = new Transaction
        {
            Date = DateTime.Now
        };
        transaction.Type = Enum.Parse<TransactionType>(type, true);
        transaction.Amount = Utilities.InidicatesNegativeAmount(transaction.Type) ? -Math.Abs(amount) : Math.Abs(amount);

        account.Transactions.Add(transaction);
        account.Balance = account.GetBalance();
        SaveChanges();
    }

    private void SaveChanges()
    {
        var json = JsonSerializer.Serialize(accounts);
        File.WriteAllText(path, json);
    }

    public void ClearAllAccounts()
    {
        accounts.Clear();
        SaveChanges();
    }

    public object GetDefaultSettings()
    {
        return new { Setting1 = "Default1", Setting2 = "Default2" };
    }

    public IEnumerable<Transaction> GetTransactionHistory(int accountId)
    {
        var account = GetAccount(accountId);
        if (account == null || account.Transactions == null)
        {
            return Enumerable.Empty<Transaction>();
        }

        return account.Transactions.OfType<Transaction>();
    }
    public IEnumerable<int> GetAllAccounts()
    {
        return accounts.Select(a => a.Id);
    }
}

