using Bank.Api.Logic;
using Bank.Logic;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Bank.Api.Tests;

public class EndpointHandlerTestsTest : IDisposable
{
    private readonly EndpointHandler handler;

    public EndpointHandlerTestsTest()
    {
        // Mock or initialize the EndpointHandler with proper dependencies
        handler = new EndpointHandler("Test.json");
    }

    public void Dispose()
    {
        if (File.Exists("Test.json"))
        {
            File.Delete("Test.json");
        }
    }

    [Fact]
    public async Task GetAccountAsync_ReturnsAccount()
    {
        // Arrange
        var accountResult = await handler.CreateAccountAsync();
        var account = Assert.IsType<Ok<Account>>(accountResult).Value;

        // Act
        var result = await handler.GetAccountAsync(account.Id);

        // Assert
        var okResult = Assert.IsType<Ok<Account>>(result);
        Assert.Equal(account.Id, okResult.Value.Id);
    }

    [Fact]
    public async Task GetAccountAsync_AccountNotFound_ReturnsNotFound()
    {
        // Act
        var result = await handler.GetAccountAsync(999);

        // Assert
        Assert.IsType<NotFound>(result);
    }

[Fact]
public async Task CreateAccountAsync_CreatesNewAccount()
{
    // Act
    var result = await handler.CreateAccountAsync();

    // Assert
    var account = Assert.IsType<Ok<Account>>(result).Value;
    Assert.NotNull(account);
    Assert.True(account.Id > 0);
    Assert.Equal(0, account.Balance);
}

    [Fact]
    public async Task DepositAsync_ValidAmount_UpdatesBalance()
    {
        // Arrange
        var accountResult = await handler.CreateAccountAsync();
        var account = Assert.IsType<Ok<Account>>(accountResult).Value;

        // Act
        var result = await handler.DepositAsync(account.Id, 100);

        // Assert
        Assert.IsType<Ok>(result);
        var updatedAccount = Assert.IsType<Ok<Account>>(await handler.GetAccountAsync(account.Id)).Value;
        Assert.Equal(100, updatedAccount.Balance);
    }

    [Fact]
    public async Task DepositAsync_InvalidAmount_ThrowsArgumentException()
    {
        // Arrange
        var accountResult = await handler.CreateAccountAsync();
        var account = Assert.IsType<Ok<Account>>(accountResult).Value;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => handler.DepositAsync(account.Id, -50));
        Assert.Equal("Transaction amount must be greater than zero.", exception.Message);
    }

    [Fact]
    public async Task WithdrawAsync_ValidAmount_UpdatesBalanceCorrectly()
    {
        // Arrange
        var accountResult = await handler.CreateAccountAsync();
        var account = Assert.IsType<Ok<Account>>(accountResult).Value;
        await handler.DepositAsync(account.Id, 200);

        // Act
        var withdrawResult = await handler.WithdrawAsync(account.Id, 75);

        // Assert
        Assert.IsType<Ok>(withdrawResult);
        var updatedAccount = Assert.IsType<Ok<Account>>(await handler.GetAccountAsync(account.Id)).Value;
        Assert.Equal(125, updatedAccount.Balance);
    }

    [Fact]
    public async Task WithdrawAsync_InsufficientFunds_ThrowsInvalidOperationException()
    {
        // Arrange
        var accountResult = await handler.CreateAccountAsync();
        var account = Assert.IsType<Ok<Account>>(accountResult).Value;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.WithdrawAsync(account.Id, 100));
        Assert.Equal("Insufficient funds.", exception.Message);
    }

    [Fact]
    public async Task GetTransactionHistoryAsync_ReturnsAllTransactions()
    {
        // Arrange
        var accountResult = await handler.CreateAccountAsync();
        var account = Assert.IsType<Ok<Account>>(accountResult).Value;
        await handler.DepositAsync(account.Id, 100);
        await handler.WithdrawAsync(account.Id, 50);

        // Act
        var result = await handler.GetTransactionHistoryAsync(account.Id);

        // Assert
        var transactions = Assert.IsType<Ok<IEnumerable<Transaction>>>(result).Value;
        Assert.Equal(2, transactions.Count());
    }

    [Fact]
    public async Task GetTransactionHistoryAsync_AccountNotFound_ReturnsNotFound()
    {
        // Act
        var result = await handler.GetTransactionHistoryAsync(999);

        // Assert
        Assert.IsType<NotFound>(result);
    }

    [Fact]
    public async Task DeleteAccountAsync_RemovesAccount()
    {
        // Arrange
        var accountResult = await handler.CreateAccountAsync();
        var account = Assert.IsType<Ok<Account>>(accountResult).Value;

        // Act
        var result = await handler.DeleteAccountAsync(account.Id);

        // Assert
        Assert.IsType<Ok>(result);
        var getResult = await handler.GetAccountAsync(account.Id);
        Assert.IsType<NotFound>(getResult);
    }

    [Fact]
    public async Task DeleteAccountAsync_AccountNotFound_ReturnsNotFound()
    {
        // Act
        var result = await handler.DeleteAccountAsync(999);

        // Assert
        Assert.IsType<NotFound>(result);
    }
}
