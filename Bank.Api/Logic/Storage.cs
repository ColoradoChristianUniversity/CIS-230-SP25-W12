using System.Text.Json;
using Bank.Logic;
using Bank.Logic.Abstractions;
using System.Text.Json.Serialization;

namespace Bank.Api.Logic;
public class ITransactionConverter : JsonConverter<ITransaction>
{
    public override ITransaction? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using (var document = JsonDocument.ParseValue(ref reader))
        {
            var root = document.RootElement;

            if (!root.TryGetProperty("Type", out var typeProperty))
            {
                throw new JsonException("Missing 'Type' property.");
            }

            var type = typeProperty.GetString();
            return type switch
            {
                nameof(Transaction) => JsonSerializer.Deserialize<Transaction>(root.GetRawText(), options),
                _ => throw new JsonException($"Unknown transaction type: {type}")
            };
        }
    }

    public override void Write(Utf8JsonWriter writer, ITransaction value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, (object)value, value.GetType(), options);
    }
}

public class Storage
{
    private const int firstId = 1;
    private readonly string path;
    private readonly List<Account> accounts;
    private readonly JsonSerializerOptions jsonOptions;

    public Storage(string? fileName = "store.json")
    {
        path = Path.Combine(AppContext.BaseDirectory, fileName ?? "store.json");

        jsonOptions = new JsonSerializerOptions
        {
            Converters = { new ITransactionConverter() },
            WriteIndented = true
        };

        if (!File.Exists(path))
        {
            File.WriteAllText(path, "[]"); // Ensures an empty JSON array instead of just creating the file
        }

        try
        {
            accounts = JsonSerializer.Deserialize<List<Account>>(File.ReadAllText(path), jsonOptions) ?? new List<Account>();
        }
        catch (JsonException)
        {
            accounts = new List<Account>();
        }
    }

    public int[] ListAccounts()
    {
        return accounts.Select(a => a.Id).ToArray();
    }

    public Account AddAccount()
    {
        var newAccount = new Account
        {
            Id = GenerateNewAccountId(),
            Settings = new()
        };

        accounts.Add(newAccount);
        SaveChanges();
        return newAccount;

        int GenerateNewAccountId()
        {
            if (accounts.Count == 0)
            {
                return firstId;
            }

            return accounts.Max(a => a.Id) + 1;
        }
    }

    public Account? GetAccount(int id) => accounts.FirstOrDefault(a => a.Id == id);

    public void RemoveAccount(int id)
    {
        var account = GetAccount(id);
        if (account == null)
        {
            return;
        }

        accounts.Remove(account);
        SaveChanges();
    }

    private void SaveChanges()
    {
        var json = JsonSerializer.Serialize(accounts, jsonOptions);
        File.WriteAllText(path, json);
    }

    // LS Additions:
    public Transaction CreateTransaction(TransactionType type, double amount, DateTime date)
    {
        return new Transaction()
        {
            Type = type,
            Amount = amount,
            Date = date
        };
    }

    public void AddTransaction(Account account, double amount, TransactionType transaction)
    {
        account.TryAddTransaction(CreateTransaction(transaction, amount, DateTime.UtcNow));
        SaveChanges();
    }

    public IReadOnlyList<ITransaction> GetTransactions(int accountId)
    {
        var account = GetAccount(accountId);
        var transactions = account.GetTransactions().OfType<Transaction>().ToList();
        return transactions;
    }
}
