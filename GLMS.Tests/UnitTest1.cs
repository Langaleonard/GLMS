using Xunit;

namespace GLMS.Tests
{
    public class CurrencyCalculationTests
    {
        [Fact]
        public void Should_Calculate_Zar_Correctly()
        {
            // Arrange
            decimal usdAmount = 2500m;
            decimal exchangeRate = 18.45m;

            // Act
            decimal zarAmount = usdAmount * exchangeRate;

            // Assert
            Assert.Equal(46125m, zarAmount);
        }
    }
}