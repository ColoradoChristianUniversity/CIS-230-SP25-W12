using System.Text.Json;
using Bank.Logic;

namespace Bank.Api.Logic;

public class Storage
{
    private readonly string _filePath;
    private readonly Dictionary<int, Account> _accounts;

    public Storage(string filePath)
    {
        _filePath = filePath;
        _accounts = File.Exists(filePath)
            ? JsonSerializer.Deserialize<Dictionary<int, Account>>(File.ReadAllText(filePath))
            ?? []
            : [];
    }

    public Account? GetAccount(int accountId)
    {
        _accounts.TryGetValue(accountId, out var account);
        return account;
    }

    public int[] ListAccounts()
    {
        return [.. _accounts.Keys];
    }

    public void AddAccount(Account account)
    {
        _accounts.Add(account.Id, account);
        SaveChanges();
    }

    public void RemoveAccount(int accountId)
    {
        _accounts.Remove(accountId);
        SaveChanges();
    }

    private void SaveChanges()
    {
        File.WriteAllText(_filePath, System.Text.Json.JsonSerializer.Serialize(_accounts));
    }
}