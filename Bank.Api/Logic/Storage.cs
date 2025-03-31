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
            File.WriteAllText(path, "[]");
        }

        try
        {
            accounts = JsonSerializer.Deserialize<List<Account>>(File.ReadAllText(path)) ?? [];
        }
        catch (JsonException)
        {
            accounts = [];
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

    public void SaveChanges() // Changed from private to public
    {
        var json = JsonSerializer.Serialize(accounts);
        File.WriteAllText(path, json);
    }
}