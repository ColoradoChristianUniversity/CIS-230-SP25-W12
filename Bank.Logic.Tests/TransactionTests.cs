using Bank.Logic.Abstractions;
using FluentAssertions;

namespace Bank.Logic.Tests
{
    public class TransactionTests
    {
        public ITransaction CreateTransaction(TransactionType type, double amount, DateTime date)
        {
            return new Transaction()
            {
                Type = type,
                Amount = amount,
                Date = date
            };
        }

        [Fact]
public void CreateTransaction_WithNaNAmount_ShouldThrowException()
{
    Action act = () => CreateTransaction(TransactionType.Deposit, double.NaN, DateTime.UtcNow);

    act.Should().Throw<ArgumentOutOfRangeException>($"{nameof(ITransaction.Amount)} should not be NaN");
}

[Fact]
public void CreateTransaction_WithInfinityAmount_ShouldThrowException()
{
    Action act = () => CreateTransaction(TransactionType.Deposit, double.PositiveInfinity, DateTime.UtcNow);

    act.Should().Throw<ArgumentOutOfRangeException>($"{nameof(ITransaction.Amount)} should not be Infinity");
}

[Fact]
public void CreateTransaction_WithNegativeInfinityAmount_ShouldThrowException()
{
    Action act = () => CreateTransaction(TransactionType.Withdraw, double.NegativeInfinity, DateTime.UtcNow);

    act.Should().Throw<ArgumentOutOfRangeException>($"{nameof(ITransaction.Amount)} should not be Negative Infinity");
}

[Fact]
public void CreateTransaction_WithWithdrawType_AndNegativeAmount_ShouldBeValid()
{
    var transaction = CreateTransaction(TransactionType.Withdraw, -50, DateTime.UtcNow);

    transaction.Amount.Should().BeNegative($"{nameof(ITransaction.Amount)} should be negative for {TransactionType.Withdraw}");
}

[Fact]
public void CreateTransaction_WithWithdrawType_AndPositiveAmount_ShouldThrowException()
{
    Action act = () => CreateTransaction(TransactionType.Withdraw, 50, DateTime.UtcNow);

    act.Should().Throw<ArgumentOutOfRangeException>($"{nameof(ITransaction.Amount)} should be negative for {TransactionType.Withdraw}");
}

[Theory]
[InlineData(TransactionType.Fee_Overdraft, 0)]
[InlineData(TransactionType.Fee_Management, 0)]
public void CreateTransaction_WithFeeType_AndZeroAmount_ShouldBeValid(TransactionType type, double amount)
{
    var transaction = CreateTransaction(type, amount, DateTime.UtcNow);

    transaction.Amount.Should().Be(0, $"{nameof(ITransaction.Amount)} should be zero for {type}");
}

[Theory]
[InlineData(TransactionType.Fee_Overdraft)]
[InlineData(TransactionType.Fee_Management)]
public void CreateTransaction_WithFeeType_AndPositiveAmount_ShouldThrowException(TransactionType type)
{
    Action act = () => CreateTransaction(type, 10, DateTime.UtcNow);

    act.Should().Throw<ArgumentOutOfRangeException>($"{nameof(ITransaction.Amount)} should be negative for {type}");
}

[Fact]
public void Withdraw_ShouldRequireNegativeAmount()
{
    // This test verifies indirectly that Utilities.IndicatesNegativeAmount(TransactionType.Withdraw) is true
    // by testing the behavior of the Transaction class
    Action act = () => CreateTransaction(TransactionType.Withdraw, 50, DateTime.UtcNow);
    
    act.Should().Throw<ArgumentOutOfRangeException>($"Withdraw should require negative amounts");
    
    // A negative amount should be valid
    var transaction = CreateTransaction(TransactionType.Withdraw, -50, DateTime.UtcNow);
    transaction.Amount.Should().BeNegative($"Withdraw should accept negative amounts");
}

[Theory]
[InlineData(TransactionType.Deposit)]
[InlineData(TransactionType.Interest)]
public void CreateTransaction_WithPositiveType_AndZeroAmount_ShouldBeValid(TransactionType type)
{
    var transaction = CreateTransaction(type, 0, DateTime.UtcNow);

    transaction.Amount.Should().Be(0, $"{nameof(ITransaction.Amount)} should be zero for {type}");
}

[Fact]
public void Transaction_ChangingTypeAfterSettingAmount_ShouldNotRevalidate()
{
    // First create a valid transaction
    var transaction = new Transaction
    {
        Type = TransactionType.Deposit,
        Amount = 100,
        Date = DateTime.UtcNow
    };
    
    // Then change the type to one that would normally require a negative amount
    transaction.Type = TransactionType.Fee_Management;
    
    // The amount should remain positive since changing the type doesn't trigger revalidation
    transaction.Amount.Should().Be(100, "Changing the type should not trigger amount revalidation");
}

[Fact]
public void Transaction_ChangingAmountAfterSettingType_ShouldValidateAgainstCurrentType()
{
    // First create a transaction with just a type
    var transaction = new Transaction
    {
        Type = TransactionType.Fee_Management,
        Date = DateTime.UtcNow
    };
    
    // Then try to set a positive amount, which should throw an exception
    Action act = () => transaction.Amount = 100;
    
    act.Should().Throw<ArgumentOutOfRangeException>("Amount should be validated against current type");
}

        [Fact]
        public void CreateTransaction_WithValidType_ShouldSetCorrectType()
        {
            var transaction = CreateTransaction(TransactionType.Deposit, 100, DateTime.UtcNow);

            transaction.Type.Should().Be(TransactionType.Deposit, nameof(ITransaction.Type));
        }

        [Fact]
        public void CreateTransaction_WithValidAmount_ShouldSetCorrectAmount()
        {
            var transaction = CreateTransaction(TransactionType.Deposit, 100, DateTime.UtcNow);

            transaction.Amount.Should().Be(100, nameof(ITransaction.Amount));
        }

        [Fact]
        public void CreateTransaction_WithValidDate_ShouldSetCorrectDate()
        {
            var date = DateTime.UtcNow;
            var transaction = CreateTransaction(TransactionType.Deposit, 100, date);

            transaction.Date.Should().BeCloseTo(date, TimeSpan.FromMilliseconds(10), nameof(ITransaction.Date));
        }

        [Theory]
        [InlineData(TransactionType.Deposit, 100, false)]
        [InlineData(TransactionType.Withdraw, -50, true)]
        [InlineData(TransactionType.Fee_Overdraft, -35, true)]
        [InlineData(TransactionType.Interest, 10, false)]
        [InlineData(TransactionType.Unknown, 0, false)]
        public void CreateTransaction_WithDifferentTypes_ShouldValidateAmountSign(TransactionType type, double amount, bool shouldBeNegative)
        {
            var transaction = CreateTransaction(type, amount, DateTime.UtcNow);

            if (shouldBeNegative)
            {
                transaction.Amount.Should().BeNegative($"{nameof(ITransaction.Amount)} should be negative for {type}");
            }
            else if (amount == 0)
            {
                transaction.Amount.Should().Be(0, $"{nameof(ITransaction.Amount)} should be 0 for {type}");
            }
            else
            {
                transaction.Amount.Should().BePositive($"{nameof(ITransaction.Amount)} should be positive for {type}");
            }
        }

        [Theory]
        [InlineData(TransactionType.Deposit, -100)]
        [InlineData(TransactionType.Interest, -10)]
        public void CreateTransaction_WithNegativeAmountForPositiveTransaction_ShouldThrowException(TransactionType type, double amount)
        {
            Action act = () => CreateTransaction(type, amount, DateTime.UtcNow);

            act.Should().Throw<ArgumentOutOfRangeException>($"{nameof(ITransaction.Amount)} should be positive for {type}");
        }

        [Fact]
        public void CreateTransaction_WithManagementFee_ShouldSetCorrectType()
        {
            var transaction = CreateTransaction(TransactionType.Fee_Management, -10, DateTime.UtcNow);

            transaction.Type.Should().Be(TransactionType.Fee_Management, $"{nameof(ITransaction.Type)} should be {TransactionType.Fee_Management}");
        }

        [Fact]
        public void CreateTransaction_WithPositiveManagementFee_ShouldThrowException()
        {
            Action act = () => CreateTransaction(TransactionType.Fee_Management, 10, DateTime.UtcNow);

            act.Should().Throw<ArgumentOutOfRangeException>($"{nameof(ITransaction.Amount)} should be negative for {TransactionType.Fee_Management}");
        }

        [Fact]
        public void CreateTransaction_WithNegativeManagementFee_ShouldBeValid()
        {
            var transaction = CreateTransaction(TransactionType.Fee_Management, -10, DateTime.UtcNow);

            transaction.Amount.Should().BeNegative($"{nameof(ITransaction.Amount)} should be negative for {TransactionType.Fee_Management}");
        }
    }
}
