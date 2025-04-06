using System.Reflection;

using Bank.Logic.Models;

namespace Bank.Logic;

public static class AccountExtensions
{
    public static bool TryAddTransaction(this Account account, double amount, TransactionType type)
    {
        if (account is null)
        {
            return false;
        }

        if (double.IsNaN(amount) || double.IsInfinity(amount))
        {
            return false;
        }

        if (type.IndicatesSystemType())
        {
            return false;
        }

        if (!ValidateTransactionAmount(amount, type))
        {
            return false;
        }

        var list = GetWritableTransactionList(account);

        var predictedNewBalance = account.Balance + amount;
        if (predictedNewBalance < 0)
        {
            var overdraft = new Transaction(TransactionType.Fee_Overdraft, -Math.Abs(account.Settings.OverdraftFee), DateTime.Now);
            list.Add(overdraft);
            return false;
        }
        else
        {
            var transaction = new Transaction(type, amount, DateTime.Now);
            list.Add(transaction);
            return true;
        }

        static bool ValidateTransactionAmount(double amount, TransactionType type)
        {
            var isNegative = TransactionTypeExtensions.InidicatesNegativeAmount(type);
            if (isNegative && amount >= 0)
            {
                return false;
            }

            if (!isNegative && amount < 0)
            {
                return false;
            }

            return true;
        }
    }

    private static List<Transaction> GetWritableTransactionList(Account account)
    {
        var prop = typeof(Account).GetProperty("txns", BindingFlags.NonPublic | BindingFlags.Instance);
        if (prop is null)
        {
            throw new InvalidOperationException("Unable to access txns property.");
        }

        return (List<Transaction>)prop.GetValue(account)!;
    }
}
