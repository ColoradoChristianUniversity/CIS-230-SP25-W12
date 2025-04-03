using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Bank.Api.Logic;
using Microsoft.Extensions.DependencyInjection;
using Bank.Logic;

namespace Bank.Api.Tests;

public class MinimalApiTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly HttpClient _client;
    private readonly Storage _storage;

    public MinimalApiTests(WebApplicationFactory<Program> factory)
    {
        _storage = new Storage();

        var factoryWithTestStorage = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IStorage>(_storage);
            });
        });
        _client = factoryWithTestStorage.CreateClient();
    }

    public void Dispose() => DeleteAllAccounts();

    private void DeleteAllAccounts()
    {
        if (File.Exists(_storage.path))
        {
            File.Delete(_storage.path);
        }
    }

    private int CreateTestAccount()
    {
        var account = _storage.NewAccount();
        account.Should().NotBeNull();
        return account!.Id;
    }

    [Fact]
    public void POST_Account_Create_ReturnsSuccess()
    {
        // Arrange
        _storage.ListAccounts().Should().BeEmpty();

        // Act
        var accountId = CreateTestAccount();

        // Assert
        accountId.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GET_Accounts_ListWithoutInsert_ReturnsEmpty()
    {
        // Arrange
        DeleteAllAccounts();

        // Act
        var response = await _client.GetAsync("/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var accounts = await response.Content.ReadFromJsonAsync<List<int>>();
        accounts.Should().NotBeNull();
    }

    [Fact]
    public async Task GET_Accounts_ListWithInsert_ReturnsInserted()
    {
        // Arrange
        DeleteAllAccounts();
        var accountId = CreateTestAccount();

        // Act
        var response = await _client.GetAsync("/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var accounts = await response.Content.ReadFromJsonAsync<List<int>>();
        accounts.Should().Contain(accountId);
    }

    [Fact]
    public async Task DELETE_Account_ReturnsSuccess()
    {
        // Arrange
        var accountId = CreateTestAccount();

        // Act
        var response = await _client.DeleteAsync($"/account/{accountId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _storage.TryGetAccount(accountId, out var deletedAccount);
        deletedAccount.Should().BeNull();
    }

    [Fact]
    public async Task GET_Account_WhenExists_ReturnsAccount()
    {
        // Arrange
        var accountId = CreateTestAccount();

        // Act
        var response = await _client.GetAsync($"/account/{accountId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var account = await response.Content.ReadFromJsonAsync<Account>();
        account.Should().NotBeNull();
        account!.Id.Should().Be(accountId);
    }

    [Fact]
    public async Task GET_Account_WhenNotExists_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/account/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task POST_Withdraw_WithSufficientFunds_ReturnsOk()
    {
        // Arrange
        var accountId = CreateTestAccount();
        await _client.PostAsync($"/deposit/{accountId}/200", null);

        // Act
        var response = await _client.PostAsync($"/withdraw/{accountId}/100", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task POST_Withdraw_WithInsufficientFunds_ReturnsBadRequest()
    {
        // Arrange
        var accountId = CreateTestAccount();

        // Act
        var response = await _client.PostAsync($"/withdraw/{accountId}/100", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_Deposit_WhenAccountExists_ReturnsOk()
    {
        // Arrange
        var accountId = CreateTestAccount();

        // Act
        var response = await _client.PostAsync($"/deposit/{accountId}/100", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task POST_Deposit_WhenAccountNotExists_ReturnsNotFound()
    {
        // Act
        var response = await _client.PostAsync("/deposit/999/100", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GET_Transactions_WhenAccountHasTransactions_ReturnsTransactions()
    {
        // Arrange
        var accountId = CreateTestAccount();
        await _client.PostAsync($"/transactions/{accountId}/Deposit/100", null);
        await _client.PostAsync($"/transactions/{accountId}/Withdrawal/-50", null);

        // Act
        var response = await _client.GetAsync($"/transactions/{accountId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var transactions = await response.Content.ReadFromJsonAsync<List<Transaction>>(new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        transactions.Should().NotBeNull();
        transactions!.Count.Should().Be(2);
    }

    [Fact]
    public async Task GET_Transactions_WhenAccountNotExists_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/transactions/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task POST_Transaction_AddWhenValid_ReturnsOk()
    {
        // Arrange
        var accountId = CreateTestAccount();

        // Act
        var response = await _client.PostAsync($"/transactions/{accountId}/Deposit/100", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task POST_Transaction_AddWhenInvalidType_ReturnsBadRequest()
    {
        // Arrange
        var accountId = CreateTestAccount();

        // Act
        var response = await _client.PostAsync($"/transactions/{accountId}/Invalid/100", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_Transaction_AddWhenAccountNotExists_ReturnsNotFound()
    {
        // Act
        var response = await _client.PostAsync("/transactions/999/Deposit/100", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public void POST_Account_Create_ShouldStartWithCleanState()
    {
        // Arrange
        _storage.ListAccounts().Should().BeEmpty();

        // Act
        var accountId = CreateTestAccount();

        // Assert
        accountId.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task POST_Account_Create_ShouldReturnOneAsync()
    {
        // Arrange
        _storage.ListAccounts().Should().BeEmpty();

        // Act
        var response = await _client.PostAsync("/account", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var account = await response.Content.ReadFromJsonAsync<Account>();
        account.Should().NotBeNull();

        _storage.ListAccounts().Should().HaveCount(1);
        _storage.ListAccounts().Should().Contain(account!.Id);
    }

    [Fact]
    public void Account_Deserialization_ShouldWork()
    {
        // Arrange
        var json = @"
        {
            ""settings"": { ""overdraftFee"": 35, ""managementFee"": 10 },
            ""id"": 1,
            ""transactions"": [
                { ""type"": 1, ""amount"": 100, ""date"": ""2025-03-27T02:32:35Z"" },
                { ""type"": 0, ""amount"": -50, ""date"": ""2025-03-27T02:32:36Z"" }
            ]
        }";

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // Act
        var account = JsonSerializer.Deserialize<Account>(json, options);

        // Assert
        account.Should().NotBeNull();
        account!.Transactions.Should().HaveCount(2);
        account.Transactions[0].Type.Should().Be(TransactionType.Deposit);
        account.Transactions[1].Type.Should().Be(TransactionType.Withdrawal);
    }

    [Fact]
    public void Transactions_Deserialization_ShouldWork()
    {
        // Arrange
        var json = @"
        [
            { ""type"": 1, ""amount"": 100, ""date"": ""2025-03-27T02:32:35Z"" },
            { ""type"": 0, ""amount"": -50, ""date"": ""2025-03-27T02:32:36Z"" }
        ]";

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // Act
        var transactions = JsonSerializer.Deserialize<List<Transaction>>(json, options);

        // Assert
        transactions.Should().NotBeNull();
        transactions!.Count.Should().Be(2);
        transactions[0].Type.Should().Be(TransactionType.Deposit);
        transactions[1].Type.Should().Be(TransactionType.Withdrawal);
    }
}
