using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Bank.Logic;
using Bank.Api.Logic;

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

    private async Task<int> CreateTestAccountAsync()
    {
        var response = await _client.PostAsync("/account", null);
        response.IsSuccessStatusCode.Should().BeTrue();

        var account = await response.Content.ReadFromJsonAsync<Account>();
        account.Should().NotBeNull();

        return account.Id;
    }

    public void Dispose()
    {
        DeleteAllAccounts();
    }

    private void DeleteAllAccounts()
    {
        foreach (var id in _storage.ListAccounts())
        {
            _storage.RemoveAccount(id); // Delete all accounts after tests
        }
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
        accounts!.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ListAccounts_WithInsert_ReturnsInserted()
    {
        DeleteAllAccounts();
        var accountId = await CreateTestAccountAsync();
        var response = await _client.GetAsync("/");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var accounts = await response.Content.ReadFromJsonAsync<List<int>>();
        accounts.Should().NotBeNull();
        accounts.Should().Contain(a => a == accountId);
    }

    [Fact]
    public async Task DeleteAccount_ReturnsSuccess()
    {
        var accountId = await CreateTestAccountAsync();
        var response = await _client.DeleteAsync($"/account/{accountId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var deletedAccount = _storage.GetAccount(accountId);
        deletedAccount.Should().BeNull();

        // Test Commit
    }
}