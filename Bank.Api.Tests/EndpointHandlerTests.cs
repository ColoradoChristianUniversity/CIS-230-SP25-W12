using Bank.Api.Logic;
using Bank.Logic;
using Microsoft.AspNetCore.Http.HttpResults;
using Xunit;

namespace Bank.Api.Tests;

public class EndpointHandlerTests : IDisposable
{
    private readonly EndpointHandler _handler = new("Test.json");

    public void Dispose()
    {
        if (File.Exists("Test.json"))
        {
            File.Delete("Test.json");
        }
    }

    [Fact]
    public async Task CreateAccountAsync_ReturnsOkResult()
    {
        var result = await _handler.CreateAccountAsync();
        var typed = Assert.IsType<Ok<Account>>(result);
        Assert.NotNull(typed.Value);
    }

    [Fact]
    public async Task GetAccountAsync_ReturnsAccount()
    {
        var createResult = await _handler.CreateAccountAsync();
        var accountId = Assert.IsType<Ok<Account>>(createResult).Value!.Id;
        var result = await _handler.GetAccountAsync(accountId);
        var okResult = Assert.IsType<Ok<Account>>(result);
        Assert.Equal(accountId, okResult.Value!.Id);
    }

    [Fact]
    public async Task GetAccountAsync_NonExistent_ReturnsBadRequest()
    {
        var result = await _handler.GetAccountAsync(999);
        Assert.IsType<BadRequest<string>>(result);
    }

    [Fact]
    public async Task DepositAsync_AddsDeposit()
    {
        var createResult = await _handler.CreateAccountAsync();
        var accountId = Assert.IsType<Ok<Account>>(createResult).Value!.Id;
        var result = await _handler.DepositAsync(accountId, 100.00);
        var okResult = Assert.IsType<Ok<Transaction>>(result);
        Assert.Equal(TransactionType.Deposit, okResult.Value!.Type);
        Assert.Equal(100.00, okResult.Value!.Amount);
    }

    [Fact]
    public async Task WithdrawAsync_ReducesBalance()
    {
        var createResult = await _handler.CreateAccountAsync();
        var accountId = Assert.IsType<Ok<Account>>(createResult).Value!.Id;
        await _handler.DepositAsync(accountId, 100.00);
        var result = await _handler.WithdrawAsync(accountId, 50.00);
        var okResult = Assert.IsType<Ok<Transaction>>(result);
        Assert.Equal(TransactionType.Withdraw, okResult.Value!.Type);
        Assert.Equal(-50.00, okResult.Value!.Amount);
    }

    [Fact]
    public async Task ListAccountsAsync_ReturnsAccounts()
    {
        await _handler.CreateAccountAsync();
        var result = await _handler.ListAccountsAsync();
        var okResult = Assert.IsType<Ok<int[]>>(result);
        Assert.Single(okResult.Value!);
    }

    [Fact]
    public async Task DeleteAccountAsync_RemovesAccount()
    {
        var createResult = await _handler.CreateAccountAsync();
        var accountId = Assert.IsType<Ok<Account>>(createResult).Value!.Id;
        var result = await _handler.DeleteAccountAsync(accountId);
        Assert.IsType<Ok>(result);
        Assert.Null(_handler.Storage.GetAccount(accountId));
    }

    [Fact]
    public async Task AddTransactionAsync_AddsFee()
    {
        var createResult = await _handler.CreateAccountAsync();
        var accountId = Assert.IsType<Ok<Account>>(createResult).Value!.Id;
        var result = await _handler.AddTransactionAsync(accountId, "Fee_Management", -10.00);
        var okResult = Assert.IsType<Ok<Transaction>>(result);
        Assert.Equal(TransactionType.Fee_Management, okResult.Value!.Type);
        Assert.Equal(-10.00, okResult.Value!.Amount);
    }

    [Fact]
    public async Task GetDefaultSettingsAsync_ReturnsSettings()
    {
        var result = await _handler.GetDefaultSettingsAsync();
        var okResult = Assert.IsType<Ok<AccountSettings>>(result);
        Assert.Equal(35.00, okResult.Value!.OverdraftFee);
        Assert.Equal(10.00, okResult.Value!.ManagementFee);
    }

    [Fact]
    public async Task GetTransactionHistoryAsync_ReturnsTransactions()
    {
        var createResult = await _handler.CreateAccountAsync();
        var accountId = Assert.IsType<Ok<Account>>(createResult).Value!.Id;
        await _handler.DepositAsync(accountId, 100.00);
        await _handler.WithdrawAsync(accountId, 50.00);
        var result = await _handler.GetTransactionHistoryAsync(accountId);
        var okResult = Assert.IsType<Ok<List<Bank.Logic.Abstractions.ITransaction>>>(result);
        Assert.Equal(2, okResult.Value!.Count);
    }
}