using Bank.Logic.Abstractions;
using FluentAssertions;

namespace Bank.Logic.Tests
{
    public class AccountTests
    {
        private readonly IAccount account;

        public AccountTests()
        {
            account = new Account
            {
                Settings = new()
                {
                    OverdraftFee = 35.00,
                    ManagementFee = 10.00
                }
            };
        }

        public ITransaction CreateTransaction(TransactionType type, double amount, DateTime date)
        {
            return new Transaction
            {
                Type = type,
                Amount = amount,
                Date = date
            };
        }

        [Fact]
        public void TryAddTransaction_WithUnknownTransactionType_ShouldReturnFalse()
        {
            var transaction = new Transaction
            {
                Type = TransactionType.Unknown,
                Amount = 0,
                Date = DateTime.UtcNow
            };

            bool result = account.TryAddTransaction(transaction);

            result.Should().BeFalse("Unknown transaction types should be rejected");
            account.GetTransactions().Should().NotContain(transaction);
        }

        [Theory]
        [InlineData(0, false)]
        [InlineData(-10, true)]
        public void TryAddTransaction_ShouldHandleInvalidDeposits(double amount, bool shouldThrow)
        {
            var account = new Account();
            account.Settings = new AccountSettings { OverdraftFee = 35 };

            if (shouldThrow)
            {
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    var deposit = new Transaction { Type = TransactionType.Deposit, Amount = amount, Date = DateTime.Now };
                    account.TryAddTransaction(deposit);
                });
            }
            else
            {
                var deposit = new Transaction { Type = TransactionType.Deposit, Amount = amount, Date = DateTime.Now };
                var result = account.TryAddTransaction(deposit);

                Assert.False(result);
                Assert.Empty(account.GetTransactions());
            }
        }

        [Fact]
        public void GetTransactions_ShouldReturnReadOnlyList()
        {
            var transactions = account.GetTransactions();
            transactions.Should().BeAssignableTo<IReadOnlyList<ITransaction>>();

            Action action = () =>
            {
                var list = transactions as IList<ITransaction>;
                list?.Add(CreateTransaction(TransactionType.Deposit, 100, DateTime.UtcNow));
            };

            action.Should().Throw<NotSupportedException>("The returned list should be read-only");
        }

        [Fact]
        public void Settings_ShouldBeProperlyStored()
        {
            var newSettings = new AccountSettings
            {
                OverdraftFee = 40.00,
                ManagementFee = 15.00
            };

            account.Settings = newSettings;

            account.Settings.Should().Be(newSettings);
            account.Settings.OverdraftFee.Should().Be(40.00);
            account.Settings.ManagementFee.Should().Be(15.00);
        }

        [Fact]
        public void GetBalance_WithNoTransactions_ShouldReturnZero()
        {
            account.GetBalance().Should().Be(0, "Balance should be zero when no transactions exist");
        }

        [Fact]
        public void TryAddTransaction_WithEmptyBalanceAndManagementFee_ShouldReturnTrue()
        {
            var fee = CreateTransaction(TransactionType.Fee_Management, -account.Settings.ManagementFee, DateTime.UtcNow);
            bool result = account.TryAddTransaction(fee);

            result.Should().BeTrue("Management fees should be allowed even with empty balance");
            account.GetTransactions().Should().Contain(fee);
            account.GetBalance().Should().Be(-account.Settings.ManagementFee);
        }

        [Fact]
        public void TryAddTransaction_WithMultipleTransactionTypes_ShouldCalculateCorrectBalance()
        {
            account.TryAddTransaction(CreateTransaction(TransactionType.Deposit, 100, DateTime.UtcNow));
            account.TryAddTransaction(CreateTransaction(TransactionType.Fee_Management, -5, DateTime.UtcNow));
            account.TryAddTransaction(CreateTransaction(TransactionType.Withdraw, -30, DateTime.UtcNow));
            account.TryAddTransaction(CreateTransaction(TransactionType.Deposit, 50, DateTime.UtcNow));

            var expectedBalance = 100 - 5 - 30 + 50;
            account.GetBalance().Should().Be(expectedBalance, "Balance should reflect all transactions");
            account.GetTransactions().Count.Should().Be(4, "All transactions should be stored");
        }

        [Fact]
        public void GetBalance_ShouldSumAllTransactions()
        {
            var account = new Account();
            account.Settings = new AccountSettings { OverdraftFee = 35 };

            var deposit = new Transaction { Type = TransactionType.Deposit, Amount = 100, Date = DateTime.Now };
            var withdrawal = new Transaction { Type = TransactionType.Withdraw, Amount = -50, Date = DateTime.Now };

            account.TryAddTransaction(deposit);
            account.TryAddTransaction(withdrawal);

            Assert.Equal(50, account.GetBalance());
        }

        [Theory]
        [InlineData(TransactionType.Withdraw, 50)]
        [InlineData(TransactionType.Fee_Management, 10)]
        public void Transaction_ShouldThrowWhenAssigningPositiveAmountsToNegativeTypes(TransactionType type, double amount)
        {
            // Arrange
            var transaction = new Transaction { Type = type };

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                transaction.Amount = amount;
            });

            Assert.Contains("Negative amount expected for this transaction type", exception.Message);
        }

        [Theory]
        [InlineData(TransactionType.Deposit, -50)]
        [InlineData(TransactionType.Interest, -10)]
        public void Transaction_ShouldThrowWhenAssigningNegativeAmountsToPositiveTypes(TransactionType type, double amount)
        {
            // Arrange
            var transaction = new Transaction { Type = type };

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                transaction.Amount = amount;
            });

            Assert.Contains("Positive amount expected for this transaction type", exception.Message);
        }

        [Fact]
        public void Transaction_ShouldThrowWhenAssigningNaNOrInfinity()
        {
            // Arrange
            var transaction = new Transaction();

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => transaction.Amount = double.NaN);
            Assert.Throws<ArgumentOutOfRangeException>(() => transaction.Amount = double.PositiveInfinity);
            Assert.Throws<ArgumentOutOfRangeException>(() => transaction.Amount = double.NegativeInfinity);
        }

        [Fact]
        public void TryAddTransaction_ShouldApplyOverdraftFeeAndRejectWithdrawal()
        {
            var account = new Account();
            account.Settings = new AccountSettings { OverdraftFee = 35 };
            var withdrawal = new Transaction { Type = TransactionType.Withdraw, Amount = -100, Date = DateTime.Now };

            var result = account.TryAddTransaction(withdrawal);

            Assert.False(result);
            Assert.Single(account.GetTransactions());
            Assert.Equal(TransactionType.Fee_Overdraft, account.GetTransactions()[0].Type);
            Assert.Equal(-35, account.GetTransactions()[0].Amount);
        }

        [Fact]
        public void TryAddTransaction_ShouldRejectZeroWithdrawal()
        {
            var account = new Account();
            account.Settings = new AccountSettings { OverdraftFee = 35 };
            var zeroWithdrawal = new Transaction { Type = TransactionType.Withdraw, Amount = 0, Date = DateTime.Now };

            var result = account.TryAddTransaction(zeroWithdrawal);

            Assert.False(result);
            Assert.Empty(account.GetTransactions());
        }

        [Fact]
        public void TryAddTransaction_ShouldAllowSuccessfulTransactions()
        {
            var account = new Account();
            account.Settings = new AccountSettings { OverdraftFee = 35 };
            var deposit = new Transaction { Type = TransactionType.Deposit, Amount = 200, Date = DateTime.Now };
            var withdrawal = new Transaction { Type = TransactionType.Withdraw, Amount = -50, Date = DateTime.Now };

            var depositResult = account.TryAddTransaction(deposit);
            var withdrawalResult = account.TryAddTransaction(withdrawal);

            Assert.True(depositResult);
            Assert.True(withdrawalResult);
            Assert.Equal(2, account.GetTransactions().Count);
            Assert.Equal(150, account.GetBalance());
        }

        [Fact]
        public void TryAddTransaction_WithMultipleTransactionsInSameState_ShouldMaintainConsistency()
        {
            var account = new Account();
            account.Settings = new AccountSettings { OverdraftFee = 35 };

            for (int i = 0; i < 5; i++)
            {
                account.TryAddTransaction(new Transaction { Type = TransactionType.Deposit, Amount = 100, Date = DateTime.Now.AddMinutes(i) });
            }

            account.GetTransactions().Count.Should().Be(5, "All deposits should be recorded");
            account.GetBalance().Should().Be(500, "Balance should be the sum of all deposits");
        }

        [Theory]
        [InlineData(TransactionType.Interest)]
        [InlineData(TransactionType.Fee_Overdraft)]
        public void TryAddTransaction_ShouldRejectInterestAndOverdraftFees(TransactionType type)
        {
            var transaction = new Transaction { Type = type, Amount = 0, Date = DateTime.Now };

            var result = account.TryAddTransaction(transaction);

            result.Should().BeFalse("Interest and overdraft fees should not be added manually");
            account.GetTransactions().Should().NotContain(transaction);
        }
    }
}
