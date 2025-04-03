using FluentAssertions;

namespace Bank.Logic.Tests;

public class TransactionTests
{
    Transaction CreateTransaction(TransactionType type, double amount, DateTime date)
        => new(type, amount, date);

    [Fact]
    public void CreateTransaction_WithNaNAmount_ShouldThrow()
    {
        // Arrange
        var type = TransactionType.Deposit;
        var amount = double.NaN;
        var date = DateTime.UtcNow;

        // Act
        var act = () => CreateTransaction(type, amount, date);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void CreateTransaction_WithInfinityAmount_ShouldThrow(double amount)
    {
        // Arrange
        var type = TransactionType.Deposit;
        var date = DateTime.UtcNow;

        // Act
        var act = () => CreateTransaction(type, amount, date);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void CreateTransaction_ValidNegativeWithdrawal_ShouldSucceed()
    {
        // Arrange
        var type = TransactionType.Withdrawal;
        var amount = -50;
        var date = DateTime.UtcNow;

        // Act
        var tx = CreateTransaction(type, amount, date);

        // Assert
        tx.Amount.Should().Be(amount);
    }

    [Fact]
    public void CreateTransaction_PositiveWithdrawal_ShouldThrow()
    {
        // Arrange
        var type = TransactionType.Withdrawal;
        var amount = 50;
        var date = DateTime.UtcNow;

        // Act
        var act = () => CreateTransaction(type, amount, date);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>().WithMessage("*Negative amount expected*");
    }

    [Theory]
    [InlineData(TransactionType.Fee_Overdraft)]
    [InlineData(TransactionType.Fee_Management)]
    public void CreateTransaction_ZeroAmountForFee_ShouldBeValid(TransactionType type)
    {
        // Arrange
        var amount = 0;
        var date = DateTime.UtcNow;

        // Act
        var tx = CreateTransaction(type, amount, date);

        // Assert
        tx.Amount.Should().Be(amount);
    }

    [Theory]
    [InlineData(TransactionType.Fee_Overdraft)]
    [InlineData(TransactionType.Fee_Management)]
    public void CreateTransaction_PositiveAmountForFee_ShouldThrow(TransactionType type)
    {
        // Arrange
        var amount = 10;
        var date = DateTime.UtcNow;

        // Act
        var act = () => CreateTransaction(type, amount, date);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>().WithMessage("*Negative amount expected*");
    }

    [Theory]
    [InlineData(TransactionType.Deposit)]
    [InlineData(TransactionType.Interest)]
    public void CreateTransaction_ZeroAmountForPositiveTypes_ShouldBeValid(TransactionType type)
    {
        // Arrange
        var amount = 0;
        var date = DateTime.UtcNow;

        // Act
        var tx = CreateTransaction(type, amount, date);

        // Assert
        tx.Amount.Should().Be(amount);
    }

    [Fact]
    public void Transaction_WithValidInputs_ShouldSetValues()
    {
        // Arrange
        var type = TransactionType.Deposit;
        var amount = 100;
        var now = DateTime.UtcNow;

        // Act
        var tx = CreateTransaction(type, amount, now);

        // Assert
        tx.Type.Should().Be(type);
        tx.Amount.Should().Be(amount);
        tx.Date.Should().BeCloseTo(now, TimeSpan.FromMilliseconds(10));
    }

    [Theory]
    [InlineData(TransactionType.Withdrawal, 100)]
    [InlineData(TransactionType.Fee_Overdraft, 20)]
    public void CreateTransaction_PositiveAmountForNegativeExpected_ShouldThrow(TransactionType type, double amount)
    {
        // Arrange
        var date = DateTime.UtcNow;

        // Act
        var act = () => CreateTransaction(type, amount, date);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(TransactionType.Deposit, -100)]
    [InlineData(TransactionType.Interest, -50)]
    public void CreateTransaction_NegativeAmountForPositiveExpected_ShouldThrow(TransactionType type, double amount)
    {
        // Arrange
        var date = DateTime.UtcNow;

        // Act
        var act = () => CreateTransaction(type, amount, date);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Transaction_ShouldBeImmutableAndSupportWithExpression()
    {
        // Arrange
        var original = CreateTransaction(TransactionType.Deposit, 100, DateTime.UtcNow);

        // Act
        var copy = original with { Amount = 200 };

        // Assert
        copy.Should().NotBeSameAs(original);
        copy.Type.Should().Be(original.Type);
        copy.Amount.Should().Be(200);
    }

    [Fact]
    public void Transaction_WithExpression_ShouldBypassValidation()
    {
        // Arrange
        var valid = CreateTransaction(TransactionType.Deposit, 100, DateTime.UtcNow);

        // Act
        var invalid = valid with { Type = TransactionType.Fee_Management };

        // Assert
        invalid.Amount.Should().BePositive("with-expressions do not re-run validation");
    }
}
