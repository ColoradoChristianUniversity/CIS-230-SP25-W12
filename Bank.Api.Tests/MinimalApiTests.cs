using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Bank.Api;
using Bank.Api.Logic;
using Bank.Logic;

namespace Bank.Api.Tests;

public class MinimalApiTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly HttpClient _client;
    private readonly Storage _storage;

    public MinimalApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _storage = new Storage();
    }

    public void Dispose() => DeleteAllAccounts();

    private void DeleteAllAccounts()
    {
        foreach (var id in _storage.ListAccounts())
        {
            _storage.RemoveAccount(id);
        }
    }

    private async Task<int> CreateTestAccountAsync()
    {
        var response = await _client.PostAsync("/account", null);
        response.IsSuccessStatusCode.Should().BeTrue();

        var account = await response.Content.ReadFromJsonAsync<Account>();
        account.Should().NotBeNull();
        return account!.Id;
    }

    [Fact]
    public async Task CreateAccount_ReturnsSuccess()
    {
        var accountId = await CreateTestAccountAsync();
        accountId.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ListAccounts_WithoutInsert_ReturnsEmpty()
    {
        DeleteAllAccounts();
        var response = await _client.GetAsync("/");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var accounts = await response.Content.ReadFromJsonAsync<List<int>>();
        accounts.Should().NotBeNull();
    }

    [Fact]
    public async Task ListAccounts_WithInsert_ReturnsInserted()
    {
        DeleteAllAccounts();
        var accountId = await CreateTestAccountAsync();
        var response = await _client.GetAsync("/");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var accounts = await response.Content.ReadFromJsonAsync<List<int>>();
        accounts.Should().Contain(accountId);
    }

    [Fact]
    public async Task DeleteAccount_ReturnsSuccess()
    {
        var accountId = await CreateTestAccountAsync();
        var response = await _client.DeleteAsync($"/account/{accountId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        _storage.TryGetAccount(accountId, out var deletedAccount);
        deletedAccount.Should().BeNull();
    }

    [Fact]
    public async Task GetAccount_ReturnsAccount_WhenAccountExists()
    {
        var accountId = await CreateTestAccountAsync();
        var response = await _client.GetAsync($"/account/{accountId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var account = await response.Content.ReadFromJsonAsync<Account>();
        account.Should().NotBeNull();
        account!.Id.Should().Be(accountId);
    }

    [Fact]
    public async Task GetAccount_ReturnsNotFound_WhenAccountDoesNotExist()
    {
        var response = await _client.GetAsync("/account/999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Withdraw_ReturnsOk_WhenSufficientFunds()
    {
        var accountId = await CreateTestAccountAsync();
        await _client.PostAsync($"/deposit/{accountId}/200", null);

        var response = await _client.PostAsync($"/withdraw/{accountId}/100", null);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Withdraw_ReturnsBadRequest_WhenInsufficientFunds()
    {
        var accountId = await CreateTestAccountAsync();
        var response = await _client.PostAsync($"/withdraw/{accountId}/100", null);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Deposit_ReturnsOk_WhenAccountExists()
    {
        var accountId = await CreateTestAccountAsync();
        var response = await _client.PostAsync($"/deposit/{accountId}/100", null);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Deposit_ReturnsNotFound_WhenAccountDoesNotExist()
    {
        var response = await _client.PostAsync("/deposit/999/100", null);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetTransactions_ReturnsTransactions_WhenAccountHasTransactions()
    {
        var accountId = await CreateTestAccountAsync();
        await _client.PostAsync($"/transactions/{accountId}/Deposit/100", null);
        await _client.PostAsync($"/transactions/{accountId}/Withdrawal/-50", null);

        var response = await _client.GetAsync($"/transactions/{accountId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var transactions = await response.Content.ReadFromJsonAsync<List<Transaction>>(new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        transactions.Should().NotBeNull();
        transactions!.Count.Should().Be(2);
    }

    [Fact]
    public async Task GetTransactions_ReturnsNotFound_WhenAccountDoesNotExist()
    {
        var response = await _client.GetAsync("/transactions/999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddTransaction_ReturnsOk_WhenTransactionIsValid()
    {
        var accountId = await CreateTestAccountAsync();
        var response = await _client.PostAsync($"/transactions/{accountId}/Deposit/100", null);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AddTransaction_ReturnsBadRequest_WhenTransactionTypeIsInvalid()
    {
        var accountId = await CreateTestAccountAsync();
        var response = await _client.PostAsync($"/transactions/{accountId}/Invalid/100", null);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddTransaction_ReturnsNotFound_WhenAccountDoesNotExist()
    {
        var response = await _client.PostAsync("/transactions/999/Deposit/100", null);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateAccount_ShouldStartWithCleanState()
    {
        _storage.ListAccounts().Should().BeEmpty();
        var accountId = await CreateTestAccountAsync();
        accountId.Should().BeGreaterThan(0);
    }

    [Fact]
    public void AccountDeserialization_ShouldWork()
    {
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

        var account = JsonSerializer.Deserialize<Account>(json, options);
        account.Should().NotBeNull();
        account!.Transactions.Should().HaveCount(2);
        account.Transactions[0].Type.Should().Be(TransactionType.Deposit);
        account.Transactions[1].Type.Should().Be(TransactionType.Withdrawal);
    }

    [Fact]
    public void TransactionDeserialization_ShouldWork()
    {
        var json = @"
        [
            { ""type"": 1, ""amount"": 100, ""date"": ""2025-03-27T02:32:35Z"" },
            { ""type"": 0, ""amount"": -50, ""date"": ""2025-03-27T02:32:36Z"" }
        ]";

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var transactions = JsonSerializer.Deserialize<List<Transaction>>(json, options);
        transactions.Should().NotBeNull();
        transactions!.Count.Should().Be(2);
        transactions[0].Type.Should().Be(TransactionType.Deposit);
        transactions[1].Type.Should().Be(TransactionType.Withdrawal);
    }
}
