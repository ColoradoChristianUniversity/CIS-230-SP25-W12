using System.Text.Json.Serialization;

namespace Bank.Logic.Models;

// do not edit this file

public record class Account
{
    public AccountSettings Settings { get; init; } = new();

    public int Id { get; init; } = default;

    public string Nickname { get; init; } = string.Empty;

    [JsonIgnore]
    public double Balance => txns.Sum(t => t.Amount);

    [JsonIgnore]
    public IReadOnlyList<Transaction> Transactions => txns;

    [JsonInclude]
    [JsonPropertyName("txns")]
    private List<Transaction> txns { get; init; } = [];

    [JsonConstructor]
    private Account(AccountSettings settings, int id, string nickname, List<Transaction> txns)
    {
        Settings = settings ?? new();
        Id = id;
        Nickname = nickname ?? string.Empty;
        this.txns = txns ?? [];
    }

    public Account() { }
}