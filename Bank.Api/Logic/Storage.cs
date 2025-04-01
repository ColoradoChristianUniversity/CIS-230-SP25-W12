using System.Text.Json;
using Bank.Logic;

namespace Bank.Api.Logic;

public class Storage : IStorage
{
    private readonly string path;
    private readonly List<Account> accounts;

    public Storage(string? fileName = "store.json")
    {
        path = Path.Combine(AppContext.BaseDirectory, fileName ?? "store.json");

        try
        {
            accounts = JsonSerializer.Deserialize<List<Account>>(ReadAllText()) ?? [];
        }
        catch (JsonException)
        {
            accounts = [];
        }

        string ReadAllText()
        {
            if (!File.Exists(path))
            {
                File.WriteAllText(path, "[]");
            }

            return File.ReadAllText(path);
        }
    }

    public int[] ListAccounts() => accounts.Select(a => a.Id).ToArray();

    public Account NewAccount()
    {
        Account newAccount = new()
        {
            Id = accounts.Count == 0 ? 1 : accounts.Max(a => a.Id) + 1,
            Settings = new()
        };

        accounts.Add(newAccount);
        SaveChanges();
        return newAccount;
    }

    public bool TryGetAccount(int id, out Account account)
    {
        account = accounts.FirstOrDefault(a => a.Id == id)!;
        return account is not null;
    }

    public void RemoveAccount(int id)
    {
        if (TryGetAccount(id, out var account))
        {
            accounts.Remove(account);
            SaveChanges();
        }
    }

    public Account UpdateAccount(Account account)
    {
        if (TryGetAccount(account.Id, out var existingAccount))
        {
            accounts.Remove(existingAccount);
        }

        accounts.Add(account);
        SaveChanges();

        if (!TryGetAccount(account.Id, out var updatedAccount))
        {
            throw new InvalidOperationException($"Account {account.Id} not found after update.");
        }
        return updatedAccount;
    }

    private void SaveChanges() => File.WriteAllText(path, JsonSerializer.Serialize(accounts));

    public IReadOnlyList<Transaction> GetTransactions(int accountId)
    {
        if (TryGetAccount(accountId, out var account))
        {
            return account.GetTransactions();
        }
        return [];
    }
}
