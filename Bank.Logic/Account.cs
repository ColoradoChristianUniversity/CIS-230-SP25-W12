using Bank.Logic.Abstractions;

namespace Bank.Logic;

public class Account : IAccount
{
    public int Id { get; set; }
    public double Balance { get; set; }
    public List<ITransaction> Transactions { get; set; } = new List<ITransaction>();
    public AccountSettings Settings { get; set; }

    public double GetBalance() { return Transactions.Sum(t => t.Amount); }

    public IReadOnlyList<ITransaction> GetTransactions() => Transactions.AsReadOnly();

    public bool TryAddTransaction(ITransaction transaction)
    {
        // Always reject unknown transactions
        if (transaction.Type == TransactionType.Unknown)
        {
            return false;
        }

        // Reject direct interest additions and overdraft fees
        if (
            transaction.Type == TransactionType.Interest ||
            transaction.Type == TransactionType.Fee_Overdraft
            )
        {
            return false;
        }

        // Reject zero or negative amounts for deposits
        if (transaction.Type == TransactionType.Deposit && transaction.Amount <= 0)
        {
            return false;
        }

        // Reject positive amounts for withdrawals and fees
        if (Utilities.InidicatesNegativeAmount(transaction.Type) && transaction.Amount > 0)
        {
            return false;
        }

        var balance = GetBalance();

        // Add automatic overdraft
        if (transaction.Type == TransactionType.Withdraw && balance < -transaction.Amount)
        {
            var feeOverdraft = new Transaction
            {
                Type = TransactionType.Fee_Overdraft,
                Amount = -Settings.OverdraftFee,
                Date = DateTime.Now
            };
            Transactions.Add(feeOverdraft);
            return false;
        }

        if (transaction.Type == TransactionType.Withdraw && transaction.Amount == 0)
        {
            return false;
        }

        Transactions.Add((Transaction)transaction);
        return true;
    }
}