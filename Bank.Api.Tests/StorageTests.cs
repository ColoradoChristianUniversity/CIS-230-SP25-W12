using System.Text.Json;
using Bank.Logic;
using Bank.Logic.Abstractions;

namespace Bank.Api.Tests;

public class StorageTests : IDisposable
{
    private readonly string testFilePath = Path.Combine(AppContext.BaseDirectory, "test_store.json");
    private readonly Bank.Api.Logic.Storage storage; // Fully qualify Storage

    public StorageTests()
    {
        if (File.Exists(testFilePath))
        {
            File.Delete(testFilePath);
        }
        storage = new Bank.Api.Logic.Storage(testFilePath); // Fully qualify Storage
    }

    public void Dispose()
    {
        if (File.Exists(testFilePath))
        {
            File.Delete(testFilePath);
        }
    }

    [Fact]
    public void Constructor_InitializesEmptyStorage()
    {
        Assert.NotNull(storage);
    }

    [Fact]
    public void AddAccount_AssignsUniqueIds()
    {
        var account1 = storage.AddAccount();
        var account2 = storage.AddAccount();
        Assert.NotEqual(account1.Id, account2.Id);
    }

    [Fact]
    public void GetAccount_ReturnsCorrectAccount()
    {
        var account = storage.AddAccount();
        var retrievedAccount = storage.GetAccount(account.Id);
        Assert.NotNull(retrievedAccount);
        Assert.Equal(account.Id, retrievedAccount!.Id);
    }

    [Fact]
    public void GetAccount_ReturnsNull_WhenAccountDoesNotExist()
    {
        Assert.Null(storage.GetAccount(999));
    }

    [Fact]
    public void RemoveAccount_DeletesAccount()
    {
        var account = storage.AddAccount();
        storage.RemoveAccount(account.Id);
        Assert.Null(storage.GetAccount(account.Id));
    }

    [Fact]
    public void RemoveAccount_DoesNothing_WhenAccountDoesNotExist()
    {
        storage.RemoveAccount(999);
        Assert.Null(storage.GetAccount(999));
    }

    [Fact]
    public void Storage_PersistsDataBetweenInstances()
    {
        var account = storage.AddAccount();
        var newStorage = new Bank.Api.Logic.Storage(testFilePath); // Fully qualify Storage
        Assert.NotNull(newStorage.GetAccount(account.Id));
    }
}