namespace Bank.Logic.Tests
{
    public class UtilityTests
    {
        [Fact]
        public void IsNegative_ShouldReturnTrue_ForNegativeTransactionTypes()
        {
            Assert.True(TransactionType.Withdraw.InidicatesNegativeAmount(), nameof(TransactionType.Withdraw));
            Assert.True(TransactionType.Fee_Overdraft.InidicatesNegativeAmount(), nameof(TransactionType.Fee_Overdraft));
        }

        [Fact]
        public void IsNegative_ShouldReturnFalse_ForPositiveTransactionTypes()
        {
            Assert.False(TransactionType.Deposit.InidicatesNegativeAmount(), nameof(TransactionType.Deposit));
            Assert.False(TransactionType.Interest.InidicatesNegativeAmount(), nameof(TransactionType.Interest));
            Assert.False(TransactionType.Unknown.InidicatesNegativeAmount(), nameof(TransactionType.Unknown));
        }
    }
}
