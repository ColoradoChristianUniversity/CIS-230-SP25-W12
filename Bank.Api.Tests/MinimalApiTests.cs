using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Bank.Logic;
using Bank.Api.Logic;
using Xunit;

namespace Bank.Api.Tests;

public class MinimalApiTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly HttpClient _client;
    private readonly Storage _storage;

    public MinimalApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IEndpointHandler>(new EndpointHandler("test_store.json"));
            });
        }).CreateClient();
        _storage = new Storage("test_store.json");
    }

    private async Task<int> CreateTestAccountAsync()
    {
        var response = await _client.PostAsync("/account", null);
        response.EnsureSuccessStatusCode();
        var account = await response.Content.ReadFromJsonAsync<Account>();
        return account!.Id;
    }

    public void Dispose()
    {
        DeleteAllAccounts();
    }

    private void DeleteAllAccounts()
    {
        foreach (var id in _storage.ListAccounts())
        {
            _storage.RemoveAccount(id);
        }
    }

    [Fact]
    public async Task CreateAccount_ReturnsSuccess()
    {
        var accountId = await CreateTestAccountAsync();
        accountId.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ListAccounts_WithInsert_ReturnsInserted()
    {
        DeleteAllAccounts();
        var accountId = await CreateTestAccountAsync();
        var response = await _client.GetAsync("/");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var accounts = await response.Content.ReadFromJsonAsync<int[]>();
        accounts.Should().Contain(accountId);
    }

    [Fact]
    public async Task GetAccount_ReturnsAccount()
    {
        var accountId = await CreateTestAccountAsync();
        var response = await _client.GetAsync($"/account/{accountId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var account = await response.Content.ReadFromJsonAsync<Account>();
        account!.Id.Should().Be(accountId);
    }

    [Fact]
    public async Task GetAccount_NonExistent_ReturnsBadRequest()
    {
        var response = await _client.GetAsync("/account/999");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Deposit_IncreasesBalance()
    {
        var accountId = await CreateTestAccountAsync();
        var response = await _client.PostAsync($"/deposit/{accountId}/100.00", null);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var transaction = await response.Content.ReadFromJsonAsync<Transaction>();
        transaction!.Amount.Should().Be(100.00);
    }

    [Fact]
    public async Task Deposit_NegativeAmount_ReturnsBadRequest()
    {
        var accountId = await CreateTestAccountAsync();
        var response = await _client.PostAsync($"/deposit/{accountId}/-100.00", null);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Withdraw_DecreasesBalance()
    {
        var accountId = await CreateTestAccountAsync();
        await _client.PostAsync($"/deposit/{accountId}/100.00", null);
        var response = await _client.PostAsync($"/withdraw/{accountId}/50.00", null);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var transaction = await response.Content.ReadFromJsonAsync<Transaction>();
        transaction!.Amount.Should().Be(-50.00);
    }

    [Fact]
    public async Task Withdraw_InsufficientFunds_ReturnsBadRequest()
    {
        var accountId = await CreateTestAccountAsync();
        var response = await _client.PostAsync($"/withdraw/{accountId}/50.00", null);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetTransactionHistory_ReturnsTransactions()
    {
        var accountId = await CreateTestAccountAsync();
        await _client.PostAsync($"/deposit/{accountId}/100.00", null);
        await _client.PostAsync($"/withdraw/{accountId}/50.00", null);
        var response = await _client.GetAsync($"/transaction/{accountId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var transactions = await response.Content.ReadFromJsonAsync<List<Bank.Logic.Abstractions.ITransaction>>();
        transactions!.Count.Should().Be(2);
    }
}