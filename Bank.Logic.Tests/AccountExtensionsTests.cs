using Bank.Logic.Models;
using FluentAssertions;
using System;
using Xunit;

namespace Bank.Logic.Tests
{
    public class AccountExtensionsTests
    {
        [Fact]
        public void TryAddTransaction_Withdrawal_ExceedingBalance_ShouldAddOverdraftFee()
        {
            // Arrange
            var account = new Account();
            
            // Set up the overdraft fee in settings
            var settings = new AccountSettings { OverdraftFee = 25 };
            account = account with { Settings = settings };
            
            // Add initial deposit
            account.TryAddTransaction(100, TransactionType.Deposit).Should().BeTrue();
            
            // Verify initial balance
            account.Balance.Should().Be(100);
            account.Transactions.Count.Should().Be(1);
            
            // Act
            // Attempt to withdraw more than the balance (causes overdraft)
            var result = account.TryAddTransaction(-120, TransactionType.Withdrawal);
            
            // Assert
            result.Should().BeFalse();
            
            // Verify there are now 3 transactions (deposit, NOT withdrawal, and overdraft fee)
            account.Transactions.Count.Should().Be(2);
            
            // Verify the withdrawal transaction was added
            account.Transactions[1].Type.Should().Be(TransactionType.Fee_Overdraft);
            account.Transactions[1].Amount.Should().Be(-25);
            
            // Verify the final balance is: $100 - NOT $120 - $25 = -$45
            account.Balance.Should().Be(75);
        }

        [Fact]
        public void TryAddTransaction_ReturnsFalse_WhenAccountIsNull()
        {
            Account? nullAccount = null;
            var result = nullAccount.TryAddTransaction(100, TransactionType.Deposit);
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData(-100, TransactionType.Deposit, false)]
        [InlineData(50, TransactionType.Withdrawal, false)]
        public void TryAddTransaction_ReturnsFalse_WhenAccountIsInvalid(double amount, TransactionType type, bool expectedResult)
        {
            var account = new Account();
            var result = account.TryAddTransaction(amount, type);
            result.Should().Be(expectedResult);
        }

        [Fact]
        public void TryAddTransaction_ReturnsFalse_WhenTypeIsSystemType()
        {
            var account = new Account();
            var result = account.TryAddTransaction(100, TransactionType.Fee_Management);
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void TryAddTransaction_ReturnsFalse_WhenAmountIsNaNOrInfinity(double amount)
        {
            var account = new Account();
            var result = account.TryAddTransaction(amount, TransactionType.Deposit);
            result.Should().BeFalse();
        }
    }
}