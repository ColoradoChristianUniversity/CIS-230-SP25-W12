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

        var isNegative = TransactionTypeExtensions.InidicatesNegativeAmount(type);
        if (isNegative && amount >= 0)
        {
            return false;
        }

        if (!isNegative && amount < 0)
        {
            return false;
        }

        // start of method
        var transaction = type switch
        {
            TransactionType.Withdrawal when account.Balance + amount < 0 => new Transaction(TransactionType.Fee_Overdraft, -Math.Abs(account.Settings.OverdraftFee), DateTime.Now),
             _ => new Transaction(type, amount, DateTime.Now)
        };
        var list = GetWritableTransactionList(account);
        list.Add(transaction);

        return true;
    }

    private static List<Transaction> GetWritableTransactionList(Account account)
    {
        var prop = typeof(Account).GetProperty("txns", BindingFlags.NonPublic | BindingFlags.Instance);

        return (List<Transaction>)prop.GetValue(account)!;
    }
}
