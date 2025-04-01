namespace Bank.Logic;

// do not edit this file

public record class Transaction
{
    public TransactionType Type { get; init; }
    public double Amount { get; init; }
    public DateTime Date { get; init; }

    public Transaction(TransactionType type, double amount, DateTime date)
    {
        ValidateAmount(type, amount);

        Type = type;
        Amount = amount;
        Date = date;

        static void ValidateAmount(TransactionType type, double amount)
        {
            if (double.IsNaN(amount) || double.IsInfinity(amount))
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            if (Utilities.InidicatesNegativeAmount(type) && amount > 0)
            {
                throw new ArgumentOutOfRangeException("Negative amount expected for this transaction type.");
            }

            if (!Utilities.InidicatesNegativeAmount(type) && amount < 0)
            {
                throw new ArgumentOutOfRangeException("Positive amount expected for this transaction type.");
            }
        }
    }
}
