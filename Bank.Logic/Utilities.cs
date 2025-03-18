namespace Bank.Logic;

public static class Utilities
{
    public static bool InidicatesNegativeAmount(this TransactionType type)
    {
        if (type == TransactionType.Withdraw || type == TransactionType.Fee_Overdraft || type == TransactionType.Fee_Management)
        {
            return true;
        }
        return false;
    }
}