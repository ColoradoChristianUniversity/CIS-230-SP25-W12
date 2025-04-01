using FluentAssertions;
using Bank.Logic;

namespace Bank.Logic.Tests;

public class AccountTests
{
    private readonly Account account;

    public AccountTests()
    {
        account = new Account
        {
            Settings = new AccountSettings
            {
                OverdraftFee = 35.00,
                ManagementFee = 10.00
            }
        };
    }

    [Fact]
    public void GetBalance_ShouldBeZeroInitially()
    {
        account.GetBalance().Should().Be(0);
    }

    [Fact]
    public void Settings_ShouldBeAssignableAndRetrievable()
    {
        var newSettings = new AccountSettings { OverdraftFee = 50, ManagementFee = 20 };
        account.Settings = newSettings;
        account.Settings.Should().Be(newSettings);
    }

    [Fact]
    public void GetTransactions_ShouldBeReadOnly()
    {
        var transactions = account.GetTransactions();
        transactions.Should().BeAssignableTo<IReadOnlyList<Transaction>>();
        var action = () => ((IList<Transaction>)transactions).Add(default!);
        action.Should().Throw<NotSupportedException>();
    }

    [Theory]
    [InlineData(TransactionType.Interest)]
    [InlineData(TransactionType.Fee_Overdraft)]
    [InlineData(TransactionType.Fee_Management)]
    public void TryAddTransaction_ShouldRejectSystemTransactionTypes(TransactionType type)
    {
        var result = account.TryAddTransaction(100, type);
        result.Should().BeFalse();
        account.GetTransactions().Should().BeEmpty();
    }

    [Theory]
    [InlineData(TransactionType.Withdrawal, 100)]
    [InlineData(TransactionType.Fee_Overdraft, 50)]
    public void TryAddTransaction_ShouldRejectPositiveAmountForNegativeTypes(TransactionType type, double amount)
    {
        var result = account.TryAddTransaction(amount, type);
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(TransactionType.Deposit, -100)]
    [InlineData(TransactionType.Interest, -50)]
    public void TryAddTransaction_ShouldRejectNegativeAmountForPositiveTypes(TransactionType type, double amount)
    {
        var result = account.TryAddTransaction(amount, type);
        result.Should().BeFalse();
    }

    [Fact]
    public void TryAddTransaction_ShouldApplyOverdraftFee()
    {
        var result = account.TryAddTransaction(-100, TransactionType.Withdrawal);

        result.Should().BeTrue();
        account.GetTransactions().Count.Should().Be(2);
        account.GetTransactions().Any(t => t.Type == TransactionType.Fee_Overdraft).Should().BeTrue();
        account.GetBalance().Should().Be(-100 - account.Settings.OverdraftFee);
    }

    [Fact]
    public void TryAddTransaction_ShouldAllowValidDeposit()
    {
        var result = account.TryAddTransaction(200, TransactionType.Deposit);

        result.Should().BeTrue();
        account.GetBalance().Should().Be(200);
        account.GetTransactions().Count.Should().Be(1);
    }

    [Fact]
    public void TryAddTransaction_ShouldAllowValidWithdrawal()
    {
        account.TryAddTransaction(200, TransactionType.Deposit);
        var result = account.TryAddTransaction(-100, TransactionType.Withdrawal);

        result.Should().BeTrue();
        account.GetBalance().Should().Be(100);
        account.GetTransactions().Count.Should().Be(2);
    }

    [Fact]
    public void Transaction_ShouldThrowOnPositiveAmountForNegativeType()
    {
        Action act = () => _ = new Transaction(TransactionType.Fee_Management, 10, DateTime.Now);
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*Negative amount expected*");
    }

    [Fact]
    public void Transaction_ShouldThrowOnNegativeAmountForPositiveType()
    {
        Action act = () => _ = new Transaction(TransactionType.Deposit, -10, DateTime.Now);
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*Positive amount expected*");
    }

    [Fact]
    public void Transaction_ShouldThrowOnNaNOrInfinity()
    {
        var now = DateTime.Now;

        Action nan = () => _ = new Transaction(TransactionType.Deposit, double.NaN, now);
        Action posInf = () => _ = new Transaction(TransactionType.Deposit, double.PositiveInfinity, now);
        Action negInf = () => _ = new Transaction(TransactionType.Deposit, double.NegativeInfinity, now);

        nan.Should().Throw<ArgumentOutOfRangeException>();
        posInf.Should().Throw<ArgumentOutOfRangeException>();
        negInf.Should().Throw<ArgumentOutOfRangeException>();
    }
}
