using FluentAssertions;

namespace Bank.Logic.Tests;

public class TransactionTests
{
    Transaction CreateTransaction(TransactionType type, double amount, DateTime date)
        => new(type, amount, date);

    [Fact]
    public void CreateTransaction_WithNaNAmount_ShouldThrow()
    {
        var act = () => CreateTransaction(TransactionType.Deposit, double.NaN, DateTime.UtcNow);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void CreateTransaction_WithInfinityAmount_ShouldThrow(double amount)
    {
        var act = () => CreateTransaction(TransactionType.Deposit, amount, DateTime.UtcNow);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void CreateTransaction_ValidNegativeWithdrawal_ShouldSucceed()
    {
        var tx = CreateTransaction(TransactionType.Withdrawal, -50, DateTime.UtcNow);
        tx.Amount.Should().Be(-50);
    }

    [Fact]
    public void CreateTransaction_PositiveWithdrawal_ShouldThrow()
    {
        var act = () => CreateTransaction(TransactionType.Withdrawal, 50, DateTime.UtcNow);
        act.Should().Throw<ArgumentOutOfRangeException>().WithMessage("*Negative amount expected*");
    }

    [Theory]
    [InlineData(TransactionType.Fee_Overdraft)]
    [InlineData(TransactionType.Fee_Management)]
    public void CreateTransaction_ZeroAmountForFee_ShouldBeValid(TransactionType type)
    {
        var tx = CreateTransaction(type, 0, DateTime.UtcNow);
        tx.Amount.Should().Be(0);
    }

    [Theory]
    [InlineData(TransactionType.Fee_Overdraft)]
    [InlineData(TransactionType.Fee_Management)]
    public void CreateTransaction_PositiveAmountForFee_ShouldThrow(TransactionType type)
    {
        var act = () => CreateTransaction(type, 10, DateTime.UtcNow);
        act.Should().Throw<ArgumentOutOfRangeException>().WithMessage("*Negative amount expected*");
    }

    [Theory]
    [InlineData(TransactionType.Deposit)]
    [InlineData(TransactionType.Interest)]
    public void CreateTransaction_ZeroAmountForPositiveTypes_ShouldBeValid(TransactionType type)
    {
        var tx = CreateTransaction(type, 0, DateTime.UtcNow);
        tx.Amount.Should().Be(0);
    }

    [Fact]
    public void Transaction_WithValidInputs_ShouldSetValues()
    {
        var now = DateTime.UtcNow;
        var tx = CreateTransaction(TransactionType.Deposit, 100, now);

        tx.Type.Should().Be(TransactionType.Deposit);
        tx.Amount.Should().Be(100);
        tx.Date.Should().BeCloseTo(now, TimeSpan.FromMilliseconds(10));
    }

    [Theory]
    [InlineData(TransactionType.Withdrawal, 100)]
    [InlineData(TransactionType.Fee_Overdraft, 20)]
    public void CreateTransaction_PositiveAmountForNegativeExpected_ShouldThrow(TransactionType type, double amount)
    {
        var act = () => CreateTransaction(type, amount, DateTime.UtcNow);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(TransactionType.Deposit, -100)]
    [InlineData(TransactionType.Interest, -50)]
    public void CreateTransaction_NegativeAmountForPositiveExpected_ShouldThrow(TransactionType type, double amount)
    {
        var act = () => CreateTransaction(type, amount, DateTime.UtcNow);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Transaction_ShouldBeImmutableAndSupportWithExpression()
    {
        var original = CreateTransaction(TransactionType.Deposit, 100, DateTime.UtcNow);
        var copy = original with { Amount = 200 };

        copy.Should().NotBeSameAs(original);
        copy.Type.Should().Be(original.Type);
        copy.Amount.Should().Be(200);
    }

    [Fact]
    public void Transaction_WithExpression_ShouldBypassValidation()
    {
        var valid = CreateTransaction(TransactionType.Deposit, 100, DateTime.UtcNow);
        var invalid = valid with { Type = TransactionType.Fee_Management };

        invalid.Amount.Should().BePositive("with-expressions do not re-run validation");
    }
}
