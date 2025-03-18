using Bank.Api.Logic;
using Bank.Logic;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Bank.Api.Tests;

public class EndpointHandlerTests: IDisposable
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
    public async Task CreateAccountAsync_ReturnsOkResult()
    {
        // Act
        var result = await handler.CreateAccountAsync();

        // Assert
        var typed = Assert.IsType<Ok<Account>>(result);
        var account = Assert.IsType<Account>(typed.Value);
        Assert.NotNull(account);
    }

    [Fact]
    public async Task DeleteAccountAsync_DeletesAccount()
    {
        // Arrange
        var account = await handler.CreateAccountAsync();
        var typed = Assert.IsType<Ok<Account>>(account);
        var accountId = Assert.IsType<Account>(typed.Value).Id;

        // Act
        var result = await handler.DeleteAccountAsync(accountId);

        // Assert
        Assert.IsType<Ok>(result);
        Assert.Null(handler.Storage.GetAccount(accountId));
    }
}