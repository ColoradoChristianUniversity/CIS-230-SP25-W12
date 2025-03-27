using Bank.Api.Logic;
using Bank.Logic;
using Bank.Logic.Abstractions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http;
using FluentAssertions;

namespace Bank.Api.Tests;

public class EndpointHandlerTests : IDisposable
{
    private readonly EndpointHandler handler = new("Test.json");

    public void Dispose()
    {
        if (File.Exists("Test.json"))
        {
            File.Delete("Test.json");
        }
    }

    [Fact]
    public async Task CreateAccountAsync_ShouldReturnOkResultWithValidAccount()
    {
        // Act
        var result = await handler.CreateAccountAsync();

        // Assert
        result.Should().BeOfType<Ok<Account>>("because the account creation should succeed");
        var account = (result as Ok<Account>)!.Value;
        account.Should().NotBeNull("because a valid account should be created");
    }

    [Fact]
    public async Task DeleteAccountAsync_ShouldDeleteAccountSuccessfully()
    {
        // Arrange
        var account = await handler.CreateAccountAsync();
        var accountId = (account as Ok<Account>)!.Value.Id;

        // Act
        var result = await handler.DeleteAccountAsync(accountId);

        // Assert
        result.Should().BeOfType<Ok>("because the account should be deleted successfully");
        handler.Storage.GetAccount(accountId).Should().BeNull("because the account should no longer exist in storage");
    }

    [Fact]
    public async Task GetAccountAsync_ShouldReturnAccount_WhenAccountExists()
    {
        // Arrange
        var account = await handler.CreateAccountAsync();
        var accountId = (account as Ok<Account>)!.Value.Id;

        // Act
        var result = await handler.GetAccountAsync(accountId);

        // Assert
        result.Should().BeOfType<Ok<Account>>("because the account exists");
        var retrievedAccount = (result as Ok<Account>)!.Value;
        retrievedAccount.Should().NotBeNull("because the account should be retrievable");
        retrievedAccount.Id.Should().Be(accountId, "because the retrieved account ID should match the created account ID");
    }

    [Fact]
    public async Task GetAccountAsync_ShouldReturnNotFound_WhenAccountDoesNotExist()
    {
        // Act
        var result = await handler.GetAccountAsync(999);

        // Assert
        result.Should().BeOfType<NotFound<string>>("because the account does not exist");
    }

    [Fact]
    public async Task ListAccountsAsync_ShouldReturnAllAccounts()
    {
        // Arrange
        await handler.CreateAccountAsync();
        await handler.CreateAccountAsync();

        // Act
        var result = await handler.ListAccountsAsync();

        // Assert
        result.Should().BeOfType<Ok<int[]>>("because the accounts should be listed successfully");
        var accounts = (result as Ok<int[]>)!.Value;
        accounts.Should().NotBeNull("because there should be accounts in the list");
        accounts.Should().HaveCount(2, "because two accounts were created");
    }

    [Fact]
    public async Task AddTransactionAsync_ShouldReturnOk_WhenTransactionIsValid()
    {
        // Arrange
        var account = await handler.CreateAccountAsync();
        var accountId = (account as Ok<Account>)!.Value.Id;

        // Act
        var result = await handler.AddTransactionAsync(accountId, "Deposit", 100);

        // Assert
        result.Should().BeOfType<Ok>("because the transaction is valid and should succeed");
    }

    [Fact]
    public async Task AddTransactionAsync_ShouldReturnBadRequest_WhenTransactionTypeIsInvalid()
    {
        // Arrange
        var account = await handler.CreateAccountAsync();
        var accountId = (account as Ok<Account>)!.Value.Id;

        // Act
        var result = await handler.AddTransactionAsync(accountId, "InvalidType", 100);

        // Assert
        result.Should().BeOfType<BadRequest<string>>("because the transaction type is invalid");
    }

    [Fact]
    public async Task AddTransactionAsync_ShouldReturnNotFound_WhenAccountDoesNotExist()
    {
        // Act
        var result = await handler.AddTransactionAsync(999, "Deposit", 100);

        // Assert
        result.Should().BeOfType<NotFound<string>>("because the account does not exist");
    }

        [Fact]
    public async Task WithdrawAsync_ShouldReturnNotFound_WhenAccountDoesNoteExist()
    {
        // Act
        var result = await handler.WithdrawAsync(999, 100);

        // Assert
        result.Should().BeOfType<NotFound<string>>("because the account does not exist"); 
    }

    [Fact]
    public async Task WithdrawAsync_ShouldReturnBadRequest_WhenInsufficientFunds()
    {
        // Arrange
        var account = await handler.CreateAccountAsync();
        var accountId = (account as Ok<Account>)!.Value.Id;

        // Act 
        var result = await handler.WithdrawAsync(accountId, 100);

        // Assert
        result.Should().BeOfType<BadRequest<string>>("because the account does not have sufficient funds");
    }

    [Fact]
    public async Task WithdrawAsync_ShouldReturnOk_WhenSufficientFunds()
    {
        // Arrange
        var account = await handler.CreateAccountAsync();
        var accountId = (account as Ok<Account>)!.Value.Id;
        await handler.AddTransactionAsync(accountId, "Deposit", 200);

        // Act
        var result = await handler.WithdrawAsync(accountId, -100);

        // Assert
        result.Should().BeOfType<Ok>("because the account has sufficient funds");
    }

    [Fact]
    public async Task DepositAsync_ShouldReturnNotFound_WhenAccountDoesNotExist()
    {
        // Act
        var result = await handler.DepositAsync(999, 100);

        // Assert
        result.Should().BeOfType<NotFound<string>>("because the account does not exist"); 
    }

    [Fact]
    public async Task DepositAsync_ShouldReturnOK_WhenAccountExists()
    {
        // Arrange
        var account = await handler.CreateAccountAsync();
        var accountId = (account as Ok<Account>)!.Value.Id;

        // Act
        var result = await handler.DepositAsync(accountId, 100);

        // Assert
        result.Should().BeOfType<Ok>("because the account exists");
    }

    [Fact]
    public async Task GetTransactionHistoryAsync_ShouldReturnTransactions_WhenAccountExists()
    {
        // Arrange
        var account = await handler.CreateAccountAsync();
        var accountId = (account as Ok<Account>)!.Value.Id;
        await handler.AddTransactionAsync(accountId, "Deposit", 100);

        // Act
        var result = await handler.GetTransactionHistoryAsync(accountId);

        // Assert
        result.Should().BeOfType<JsonHttpResult<IReadOnlyList<ITransaction>>>("because the account exists and has transactions");
        var transactions = (result as JsonHttpResult<IReadOnlyList<ITransaction>>)!.Value;
        transactions.Should().NotBeNull("because there should be transactions for the account");
        transactions.Should().HaveCount(1, "because one transaction was added");
    }

    [Fact]
    public async Task GetTransactionHistoryAsync_ShouldReturnNotFound_WhenAccountDoesNotExist()
    {
        // Act
        var result = await handler.GetTransactionHistoryAsync(999);

        // Assert
        result.Should().BeOfType<NotFound<string>>("because the account does not exist");
    }

    [Fact]
    public async Task GetDefaultSettingsAsync_ShouldThrowNotImplementedException()
    {
        // Act
        Func<Task> act = async () => await handler.GetDefaultSettingsAsync();

        // Assert
        await act.Should().ThrowAsync<NotImplementedException>("because the method is not implemented");
    }
    
    [Fact]
    public async Task WrapperAsync_ShouldReturnOk_WhenActionSucceeds()
    {
        // Arrange
        Func<IResult> action = () => Results.Ok("Success");

        // Act
        var result = await EndpointHandler.WrapperAsync(action);

        // Assert
        result.Should().BeOfType<Ok<string>>("because the action succeeded");
        var value = (result as Ok<string>)!.Value;
        value.Should().Be("Success", "because the action returned 'Success'");
    }

    [Fact]
    public async Task WrapperAsync_ShouldReturnBadRequest_WhenArgumentExceptionIsThrown()
    {
        // Arrange
        Func<IResult> action = () => throw new ArgumentException("Invalid argument");

        // Act
        var result = await EndpointHandler.WrapperAsync(action);

        // Assert
        result.Should().BeOfType<BadRequest<string>>("because an ArgumentException was thrown");
        var value = (result as BadRequest<string>)!.Value;
        value.Should().Be("Invalid argument", "because the exception message should be returned");
    }

    [Fact]
    public async Task WrapperAsync_ShouldReturnConflict_WhenInvalidOperationExceptionIsThrown()
    {
        // Arrange
        Func<IResult> action = () => throw new InvalidOperationException("Conflict occurred");

        // Act
        var result = await EndpointHandler.WrapperAsync(action);

        // Assert
        result.Should().BeOfType<Conflict<string>>("because an InvalidOperationException was thrown");
        var value = (result as Conflict<string>)!.Value;
        value.Should().Be("Conflict occurred", "because the exception message should be returned");
    }

    [Fact]
    public async Task WrapperAsync_ShouldReturnProblem_WhenGenericExceptionIsThrown()
    {
        // Arrange
        Func<IResult> action = () => throw new Exception("Unexpected error");

        // Act
        var result = await EndpointHandler.WrapperAsync(action);

        // Assert
        result.Should().BeOfType<ProblemHttpResult>("because a generic exception was thrown");
        var problemDetails = (result as ProblemHttpResult)!.ProblemDetails;
        problemDetails.Detail.Should().Be("An error occurred: Unexpected error", "because the exception message should be included in the problem details");
    }
}