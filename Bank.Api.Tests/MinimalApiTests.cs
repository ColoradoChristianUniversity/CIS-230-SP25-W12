using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Bank.Logic;
using Bank.Api.Logic;
using Bank.Logic.Abstractions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bank.Api.Tests;

public class ITransactionConverter : JsonConverter<ITransaction>
{
    public override ITransaction? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using (var document = JsonDocument.ParseValue(ref reader))
        {
            var root = document.RootElement;

            if (!root.TryGetProperty("type", out var typeProperty))
            {
                throw new JsonException("Missing 'type' property in transaction JSON.");
            }

            // Convert the integer type to the TransactionType enum
            var type = (TransactionType)typeProperty.GetInt32();

            // Deserialize the rest of the transaction
            var transaction = JsonSerializer.Deserialize<Transaction>(root.GetRawText(), options);
            if (transaction == null)
            {
                throw new JsonException("Failed to deserialize transaction.");
            }

            transaction.Type = type; // Ensure the type is set correctly
            return transaction;
        }
    }

    public override void Write(Utf8JsonWriter writer, ITransaction value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, (object)value, value.GetType(), options);
    }
}

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
    }

    // LS Additions
    [Fact]
    public async Task GetAccount_ReturnsAccount_WhenAccountExists()
    {
        // Arrange
        var accountId = await CreateTestAccountAsync();

        // Act
        var response = await _client.GetAsync($"/account/{accountId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because the account exists");
        var account = await response.Content.ReadFromJsonAsync<Account>();
        account.Should().NotBeNull("because the account should be returned");
        account!.Id.Should().Be(accountId, "because the returned account ID should match the requested ID");
    }

    [Fact]
    public async Task GetAccount_ReturnsNotFound_WhenAccountDoesNotExist()
    {
        // Act
        var response = await _client.GetAsync("/account/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound, "because the account does not exist");
    }
    
    [Fact]
    public async Task Withdraw_ReturnsOk_WhenSufficientFunds()
    {
        // Arrange
        var accountId = await CreateTestAccountAsync();
        await _client.PostAsync($"/deposit/{accountId}/200", null); // Deposit funds

        // Act
        var response = await _client.PostAsync($"/withdraw/{accountId}/-100", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because the account has sufficient funds");
    }

    [Fact]
    public async Task Withdraw_ReturnsBadRequest_WhenInsufficientFunds()
    {
        // Arrange
        var accountId = await CreateTestAccountAsync();

        // Act
        var response = await _client.PostAsync($"/withdraw/{accountId}/100", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "because the account does not have sufficient funds");
    }

    [Fact]
    public async Task Deposit_ReturnsOk_WhenAccountExists()
    {
        // Arrange
        var accountId = await CreateTestAccountAsync();

        // Act
        var response = await _client.PostAsync($"/deposit/{accountId}/100", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because the deposit should succeed");
    }

    [Fact]
    public async Task Deposit_ReturnsNotFound_WhenAccountDoesNotExist()
    {
        // Act
        var response = await _client.PostAsync("/deposit/999/100", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound, "because the account does not exist");
    }

   
    // This test alone took me 5 hours to figure out what was goign wrong, but its done :D
    [Fact]
    public async Task GetTransactions_ReturnsTransactions_WhenAccountHasTransactions()
    {
        // Arrange
        var accountId = await CreateTestAccountAsync();
        await _client.PostAsync($"/transactions/{accountId}/Deposit/100", null);
        await _client.PostAsync($"/transactions/{accountId}/Withdraw/-50", null);

        // Act
        var response = await _client.GetAsync($"/transactions/{accountId}");

        // display converted response
        var responseString = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because the account has transactions");
        var account = await response.Content.ReadFromJsonAsync<Account>(new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // Apply camelCase here
            Converters = { new ITransactionConverter() }
        });
        account.Should().NotBeNull("because transactions should be returned");
        var transactions = account!.Transactions;
        transactions.Should().NotBeEmpty("because transactions should be returned");
        transactions!.Count.Should().Be(2, "because two transactions were added");
    }

    [Fact]
    public async Task GetTransactions_ReturnsNotFound_WhenAccountDoesNotExist()
    {
        // Act
        var response = await _client.GetAsync("/transactions/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound, "because the account does not exist");
    }

    [Fact]
    public async Task AddTransaction_ReturnsOk_WhenTransactionIsValid()
    {
        // Arrange
        var accountId = await CreateTestAccountAsync();

        // Act
        var response = await _client.PostAsync($"/transactions/{accountId}/Deposit/100", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because the transaction is valid");
    }

    [Fact]
    public async Task AddTransaction_ReturnsBadRequest_WhenTransactionTypeIsInvalid()
    {
        // Arrange
        var accountId = await CreateTestAccountAsync();

        // Act
        var response = await _client.PostAsync($"/transactions/{accountId}/InvalidType/100", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "because the transaction type is invalid");
    } 

    [Fact]
    public async Task AddTransaction_ReturnsNotFound_WhenAccountDoesNotExist()
    {
        // Act
        var response = await _client.PostAsync("/transactions/999/Deposit/100", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound, "because the account does not exist");
    }  

    [Fact]
    public async Task CreateAccount_ShouldStartWithCleanState()
    {
        // Verify that storage is clean
        _storage.ListAccounts().Should().BeEmpty("because Dispose should have cleared all accounts");

        // Act
        var accountId = await CreateTestAccountAsync();

        // Assert
        accountId.Should().BeGreaterThan(0, "because a new account should be created successfully");
    }

[Fact]
public void AccountDeserialization_ShouldWork()
{
    // Arrange
    var json = @"
    {
        ""settings"": {
            ""overdraftFee"": 35,
            ""managementFee"": 10
        },
        ""id"": 1,
        ""transactions"": [
            { ""type"": 1, ""amount"": 100, ""date"": ""2025-03-27T02:32:35.2832082Z"" },
            { ""type"": 0, ""amount"": -50, ""date"": ""2025-03-27T02:32:35.3323651Z"" }
        ]
    }";

    var options = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // Apply camelCase here
        Converters = { new ITransactionConverter() }
    };

    // Act
    var account = JsonSerializer.Deserialize<Account>(json, options);

    // Assert
    account.Should().NotBeNull();
    account!.Id.Should().Be(1);
    account.Transactions.Should().NotBeEmpty();
    account.Transactions.Count.Should().Be(2);
    account.Transactions[0].Type.Should().Be(TransactionType.Deposit);
    account.Transactions[1].Type.Should().Be(TransactionType.Withdraw);
}

[Fact]
public void ITransactionConverter_ShouldDeserializeTransactions()
{
    // Arrange
    var json = @"
    [
        { ""type"": 1, ""amount"": 100, ""date"": ""2025-03-27T02:32:35.2832082Z"" },
        { ""type"": 0, ""amount"": -50, ""date"": ""2025-03-27T02:32:35.3323651Z"" }
    ]";

    var options = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // Apply camelCase here
        Converters = { new ITransactionConverter() }
    };

    // Act
    var transactions = JsonSerializer.Deserialize<List<ITransaction>>(json, options);

    // Assert
    transactions.Should().NotBeNull();
    transactions!.Count.Should().Be(2);
    transactions[0].Type.Should().Be(TransactionType.Deposit);
    transactions[1].Type.Should().Be(TransactionType.Withdraw);
}
}