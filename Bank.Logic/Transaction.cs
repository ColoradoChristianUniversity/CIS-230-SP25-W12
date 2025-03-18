using Bank.Logic.Abstractions;

namespace Bank.Logic;

public class Transaction : ITransaction
{
    private TransactionType _type;
    public TransactionType Type 
    { 
        get => _type; 
        set => _type = value; 
    }

    private double _amount;
    public double Amount 
    { 
        get => _amount;
        set
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException();
            }

            if (Utilities.InidicatesNegativeAmount(Type) && value > 0)
            {
                throw new ArgumentOutOfRangeException("Negative amount expected for this transaction type.");
            }
            else if (!Utilities.InidicatesNegativeAmount(Type) && value < 0)
            {
                throw new ArgumentOutOfRangeException("Positive amount expected for this transaction type.");
            }

            _amount = value;
        } 
    }

    private DateTime _date;
    public DateTime Date { get => _date; set => _date = value; }
}