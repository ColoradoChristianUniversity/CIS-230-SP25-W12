using FluentAssertions;
using Bank.Logic;
using Bank.Api.Logic;

namespace Bank.Api.Tests;

public class StorageTests : IDisposable
{
    private readonly string testFilePath = Path.Combine(AppContext.BaseDirectory, "test_store.json");
    private readonly Storage storage;

    public StorageTests()
    {
        if (File.Exists(testFilePath))
        {
            File.Delete(testFilePath);
        }

        storage = new Storage(testFilePath);
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
        storage.Should().NotBeNull();
        storage.ListAccounts().Should().BeEmpty();
    }

    [Fact]
    public void NewAccount_AssignsUniqueIds()
    {
        var account1 = storage.NewAccount();
        var account2 = storage.NewAccount();

        account1.Id.Should().NotBe(account2.Id);
    }

    [Fact]
    public void TryGetAccount_ReturnsTrue_WhenAccountExists()
    {
        var account = storage.NewAccount();

        var result = storage.TryGetAccount(account.Id, out var retrieved);

        result.Should().BeTrue();
        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(account.Id);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(9999)]
    public void TryGetAccount_ReturnsFalse_WhenAccountDoesNotExist(int id)
    {
        var result = storage.TryGetAccount(id, out var account);
        result.Should().BeFalse();
        account.Should().BeNull();
    }

    [Fact]
    public void RemoveAccount_DeletesAccount()
    {
        var account = storage.NewAccount();
        storage.RemoveAccount(account.Id);

        storage.TryGetAccount(account.Id, out _).Should().BeFalse();
    }

    [Fact]
    public void RemoveAccount_DoesNothing_WhenAccountDoesNotExist()
    {
        var maxId = storage.ListAccounts().DefaultIfEmpty(0).Max();
        var nonExistentId = maxId + 1;

        storage.RemoveAccount(nonExistentId);
        storage.TryGetAccount(nonExistentId, out _).Should().BeFalse();
    }

    [Fact]
    public void Storage_PersistsDataBetweenInstances()
    {
        var account = storage.NewAccount();
        var reopened = new Storage(testFilePath);

        var result = reopened.TryGetAccount(account.Id, out var loaded);
        result.Should().BeTrue();
        loaded.Should().NotBeNull();
        loaded!.Id.Should().Be(account.Id);
    }

    [Fact]
    public void ListAccounts_ReturnsAllAccountIds()
    {
        var a1 = storage.NewAccount();
        var a2 = storage.NewAccount();

        var list = storage.ListAccounts();

        list.Should().Contain(a1.Id);
        list.Should().Contain(a2.Id);
        list.Should().HaveCount(2);
    }

    [Fact]
    public void AddTransactionToAccount_ShouldUpdateAccount()
    {
        var account = storage.NewAccount();
        account.TryAddTransaction(100.0, TransactionType.Deposit).Should().BeTrue();

        var updated = storage.UpdateAccount(account);

        updated.GetTransactions().Should().ContainSingle(t =>
            t.Amount == 100.0 && t.Type == TransactionType.Deposit);
    }

    [Fact]
    public void GetTransactions_ReturnsEmptyList_WhenAccountMissing()
    {
        int fakeId = storage.ListAccounts().DefaultIfEmpty(0).Max() + 1;

        var transactions = storage.GetTransactions(fakeId);

        transactions.Should().BeEmpty();
    }

    [Fact]
    public void GetTransactions_ReturnsAllTransactions()
    {
        var account = storage.NewAccount();
        account.TryAddTransaction(200.0, TransactionType.Deposit).Should().BeTrue();
        account.TryAddTransaction(-50.0, TransactionType.Withdrawal).Should().BeTrue();

        storage.UpdateAccount(account);

        var transactions = storage.GetTransactions(account.Id);

        transactions.Should().HaveCount(2);
        transactions.Should().Contain(t => t.Type == TransactionType.Deposit && t.Amount == 200.0);
        transactions.Should().Contain(t => t.Type == TransactionType.Withdrawal && t.Amount == -50.0);
    }

    [Fact]
    public void Constructor_HandlesInvalidJson()
    {
        File.WriteAllText(testFilePath, "not valid json");

        var reloaded = new Storage(testFilePath);
        reloaded.ListAccounts().Should().BeEmpty();
    }
}
