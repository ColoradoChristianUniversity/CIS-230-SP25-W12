using Bank.Api.Logic;
using Bank.Logic;
using Microsoft.AspNetCore.Http.HttpResults;
using FluentAssertions;
using Microsoft.AspNetCore.Http;

namespace Bank.Api.Tests;

public class EndpointHandlerTests : IDisposable
{
    private readonly EndpointHandler handler;
    private readonly string testFile = "Test.json";

    public EndpointHandlerTests()
    {
        if (File.Exists(testFile)) File.Delete(testFile);
        handler = new EndpointHandler(new Storage(testFile));
    }

    public void Dispose()
    {
        if (File.Exists(testFile)) File.Delete(testFile);
    }

    [Fact]
    public async Task CreateAccountAsync_ShouldReturnOk()
    {
        var result = await handler.CreateAccountAsync();
        result.Should().BeOfType<Ok<Account>>();

        var account = (result as Ok<Account>)!.Value;
        account.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteAccountAsync_ShouldRemoveAccount()
    {
        var result = await handler.CreateAccountAsync();
        result.Should().BeOfType<Ok<Account>>();
        var accountId = ((Ok<Account>)result).Value!.Id;

        var deleteResult = await handler.DeleteAccountAsync(accountId);
        deleteResult.Should().BeOfType<Ok>();

        var notFound = await handler.GetAccountAsync(accountId);
        notFound.Should().BeOfType<NotFound<string>>();
    }

    [Fact]
    public async Task GetAccountAsync_ShouldReturnOkIfExists()
    {
        var result = await handler.CreateAccountAsync();
        result.Should().BeOfType<Ok<Account>>();
        var accountId = ((Ok<Account>)result).Value!.Id;

        var getResult = await handler.GetAccountAsync(accountId);
        getResult.Should().BeOfType<Ok<Account>>();

        var account = ((Ok<Account>)getResult).Value;
        account.Should().NotBeNull();
        account.Id.Should().Be(accountId);
    }

    [Fact]
    public async Task GetAccountAsync_ShouldReturnNotFound_IfMissing()
    {
        var result = await handler.GetAccountAsync(999);
        result.Should().BeOfType<NotFound<string>>();
    }

    [Fact]
    public async Task ListAccountsAsync_ShouldReturnAllAccounts()
    {
        await handler.CreateAccountAsync();
        await handler.CreateAccountAsync();

        var result = await handler.ListAccountsAsync();
        result.Should().BeOfType<Ok<int[]>>();

        var ids = ((Ok<int[]>)result).Value;
        ids.Should().HaveCount(2);
    }

    [Fact]
    public async Task AddTransactionAsync_ShouldReturnOk()
    {
        var result = await handler.CreateAccountAsync();
        result.Should().BeOfType<Ok<Account>>();
        var accountId = ((Ok<Account>)result).Value!.Id;

        var txResult = await handler.AddTransactionAsync(accountId, "Deposit", 100);
        txResult.Should().BeOfType<Ok>();
    }

    [Fact]
    public async Task AddTransactionAsync_ShouldReturnBadRequest_IfInvalidType()
    {
        var result = await handler.CreateAccountAsync();
        result.Should().BeOfType<Ok<Account>>();
        var accountId = ((Ok<Account>)result).Value!.Id;

        var txResult = await handler.AddTransactionAsync(accountId, "InvalidType", 100);
        txResult.Should().BeOfType<BadRequest<string>>();
    }

    [Fact]
    public async Task AddTransactionAsync_ShouldReturnNotFound_IfAccountMissing()
    {
        var result = await handler.AddTransactionAsync(999, "Deposit", 100);
        result.Should().BeOfType<NotFound<string>>();
    }

    [Fact]
    public async Task WithdrawAsync_ShouldReturnNotFound_IfAccountMissing()
    {
        var result = await handler.WithdrawAsync(999, 100);
        result.Should().BeOfType<NotFound<string>>();
    }

    [Fact]
    public async Task WithdrawAsync_ShouldReturnBadRequest_IfInsufficientFunds()
    {
        var result = await handler.CreateAccountAsync();
        result.Should().BeOfType<Ok<Account>>();
        var accountId = ((Ok<Account>)result).Value!.Id;

        var withdrawal = await handler.WithdrawAsync(accountId, 100);
        withdrawal.Should().BeOfType<BadRequest<string>>();
    }

    [Fact]
    public async Task WithdrawAsync_ShouldReturnOk_IfSufficientFunds()
    {
        var result = await handler.CreateAccountAsync();
        result.Should().BeOfType<Ok<Account>>();
        var accountId = ((Ok<Account>)result).Value!.Id;

        await handler.DepositAsync(accountId, 200);
        var withdrawal = await handler.WithdrawAsync(accountId, 100);
        withdrawal.Should().BeOfType<Ok>();
    }

    [Fact]
    public async Task DepositAsync_ShouldReturnNotFound_IfMissing()
    {
        var result = await handler.DepositAsync(999, 100);
        result.Should().BeOfType<NotFound<string>>();
    }

    [Fact]
    public async Task DepositAsync_ShouldReturnOk_IfExists()
    {
        var result = await handler.CreateAccountAsync();
        result.Should().BeOfType<Ok<Account>>();
        var accountId = ((Ok<Account>)result).Value!.Id;

        var deposit = await handler.DepositAsync(accountId, 100);
        deposit.Should().BeOfType<Ok>();
    }

    [Fact]
    public async Task GetTransactionHistoryAsync_ShouldReturnJson_IfExists()
    {
        var result = await handler.CreateAccountAsync();
        result.Should().BeOfType<Ok<Account>>();
        var accountId = ((Ok<Account>)result).Value!.Id;

        await handler.AddTransactionAsync(accountId, "Deposit", 100);
        var history = await handler.GetTransactionHistoryAsync(accountId);

        history.Should().BeOfType<JsonHttpResult<IReadOnlyList<Transaction>>>();
        var transactions = ((JsonHttpResult<IReadOnlyList<Transaction>>)history).Value;
        transactions.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetTransactionHistoryAsync_ShouldReturnNotFound_IfMissing()
    {
        var result = await handler.GetTransactionHistoryAsync(999);
        result.Should().BeOfType<NotFound<string>>();
    }

    [Fact]
    public void GetDefaultSettings_ShouldReturnSettings()
    {
        var result = handler.GetDefaultSettings();
        result.Should().BeOfType<Ok<AccountSettings>>();

        var settings = ((Ok<AccountSettings>)result).Value;
        settings.Should().NotBeNull();
    }

    [Fact]
    public async Task WrapperAsync_ShouldReturnOk_IfSuccess()
    {
        var result = await EndpointHandler.WrapperAsync(() => Results.Ok("yay"));
        result.Should().BeOfType<Ok<string>>().Which.Value.Should().Be("yay");
    }

    [Fact]
    public async Task WrapperAsync_ShouldReturnBadRequest_IfArgumentException()
    {
        var result = await EndpointHandler.WrapperAsync(() => throw new ArgumentException("bad"));
        result.Should().BeOfType<BadRequest<string>>().Which.Value.Should().Be("bad");
    }

    [Fact]
    public async Task WrapperAsync_ShouldReturnConflict_IfInvalidOp()
    {
        var result = await EndpointHandler.WrapperAsync(() => throw new InvalidOperationException("fail"));
        result.Should().BeOfType<Conflict<string>>().Which.Value.Should().Be("fail");
    }

    [Fact]
    public async Task WrapperAsync_ShouldReturnProblem_IfUnhandled()
    {
        var result = await EndpointHandler.WrapperAsync(() => throw new Exception("boom"));
        result.Should().BeOfType<ProblemHttpResult>();
        ((ProblemHttpResult)result).ProblemDetails.Detail.Should().Contain("boom");
    }
}
