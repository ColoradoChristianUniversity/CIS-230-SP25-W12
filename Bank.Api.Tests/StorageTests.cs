using Bank.Api.Logic;
using Bank.Logic;
using Xunit;

namespace Bank.Api.Tests;

public class StorageTests : IDisposable
{
    private readonly Storage _storage = new("Test.json");

    public void Dispose()
    {
        if (File.Exists("Test.json"))
        {
            File.Delete("Test.json");
        }
    }

    [Fact]
    public void Constructor_WhenFileDoesNotExist_CreatesEmpty()
    {
        Assert.Empty(_storage.ListAccounts());
    }

    [Fact]
    public void AddAccount_AssignsUniqueIds()
    {
        var account1 = new Account { Id = 1, Settings = new() };
        var account2 = new Account { Id = 2, Settings = new() };
        _storage.AddAccount(account1);
        _storage.AddAccount(account2);
        Assert.NotEqual(account1.Id, account2.Id);
    }

    [Fact]
    public void GetAccount_ReturnsCorrectAccount()
    {
        var account = new Account { Id = 1, Settings = new() };
        _storage.AddAccount(account);
        var retrievedAccount = _storage.GetAccount(account.Id);
        Assert.NotNull(retrievedAccount);
        Assert.Equal(account.Id, retrievedAccount!.Id);
    }

    [Fact]
    public void GetAccount_ReturnsNull_WhenAccountDoesNotExist()
    {
        Assert.Null(_storage.GetAccount(999));
    }

    [Fact]
    public void ListAccounts_WhenEmpty_ReturnsEmpty()
    {
        Assert.Empty(_storage.ListAccounts());
    }

    [Fact]
    public void ListAccounts_AfterAdd_ReturnsIds()
    {
        _storage.AddAccount(new Account { Id = 1, Settings = new() });
        _storage.AddAccount(new Account { Id = 2, Settings = new() });
        var ids = _storage.ListAccounts();
        Assert.Equal(2, ids.Length);
        Assert.Contains(1, ids);
        Assert.Contains(2, ids);
    }

    [Fact]
    public void RemoveAccount_ThenGetAccount_ReturnsNull()
    {
        _storage.AddAccount(new Account { Id = 1, Settings = new() });
        _storage.RemoveAccount(1);
        var account = _storage.GetAccount(1);
        Assert.Null(account);
    }

    [Fact]
    public void AddAccount_ThenReload_ReturnsAccount()
    {
        _storage.AddAccount(new Account { Id = 1, Settings = new() });
        var newStorage = new Storage("Test.json");
        var account = newStorage.GetAccount(1);
        Assert.NotNull(account);
        Assert.Equal(1, account!.Id);
    }
}