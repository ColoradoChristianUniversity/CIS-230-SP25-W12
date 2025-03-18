namespace Bank.Logic.Tests
{
    public class AccountSettingsTests
    {
        [Fact]
        public void AccountSettings_DefaultConstructor_ShouldSetDefaultValues()
        {
            // Act
            var settings = new AccountSettings();
            
            // Assert
            Assert.Equal(35.00, settings.OverdraftFee);
            Assert.Equal(10.00, settings.ManagementFee);
        }
        
        [Fact]
        public void AccountSettings_ShouldAllowModifyingOverdraftFee()
        {
            // Arrange
            var settings = new AccountSettings();
            
            // Act
            settings.OverdraftFee = 45.00;
            
            // Assert
            Assert.Equal(45.00, settings.OverdraftFee);
        }
        
        [Fact]
        public void AccountSettings_ShouldAllowModifyingManagementFee()
        {
            // Arrange
            var settings = new AccountSettings();
            
            // Act
            settings.ManagementFee = 15.00;
            
            // Assert
            Assert.Equal(15.00, settings.ManagementFee);
        }
        
        [Fact]
        public void AccountSettings_ShouldBeRecordClass_WithValueEquality()
        {
            // Arrange
            var settings1 = new AccountSettings 
            { 
                OverdraftFee = 40.00, 
                ManagementFee = 12.00 
            };
            
            var settings2 = new AccountSettings 
            { 
                OverdraftFee = 40.00, 
                ManagementFee = 12.00 
            };
            
            var differentSettings = new AccountSettings 
            { 
                OverdraftFee = 30.00, 
                ManagementFee = 8.00 
            };
            
            // Assert
            Assert.Equal(settings1, settings2); // Should be equal due to same property values
            Assert.NotEqual(settings1, differentSettings); // Should not be equal due to different property values
        }
        
        [Fact]
        public void AccountSettings_ShouldSupportWithExpression()
        {
            // Arrange
            var originalSettings = new AccountSettings
            {
                OverdraftFee = 40.00,
                ManagementFee = 12.00
            };
            
            // Act - Using the "with" expression from record class
            var modifiedSettings = originalSettings with { OverdraftFee = 50.00 };
            
            // Assert
            Assert.Equal(50.00, modifiedSettings.OverdraftFee); // Modified property
            Assert.Equal(12.00, modifiedSettings.ManagementFee); // Unchanged property
            Assert.NotEqual(originalSettings, modifiedSettings); // Should be different instances
        }
        
        [Fact]
        public void AccountSettings_ShouldGenerateCorrectToString()
        {
            // Arrange
            var settings = new AccountSettings
            {
                OverdraftFee = 40.00,
                ManagementFee = 12.00
            };
            
            // Act
            string toString = settings.ToString();
            
            // Assert
            Assert.Contains("OverdraftFee = 40", toString);
            Assert.Contains("ManagementFee = 12", toString);
        }
    }
}