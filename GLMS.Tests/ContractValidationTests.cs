using Xunit;
using GLMS.Web.Models.Enums;

namespace GLMS.Tests
{
    public class ContractValidationTests
    {
        [Fact]
        public void Should_Allow_Active_Contract()
        {
            // Arrange
            ContractStatus status = ContractStatus.Active;

            // Act
            bool canCreateRequest =
                status == ContractStatus.Active;

            // Assert
            Assert.True(canCreateRequest);
        }

        [Fact]
        public void Should_Block_Expired_Contract()
        {
            // Arrange
            ContractStatus status = ContractStatus.Expired;

            // Act
            bool canCreateRequest =
                status == ContractStatus.Active;

            // Assert
            Assert.False(canCreateRequest);
        }

        [Fact]
        public void Should_Block_OnHold_Contract()
        {
            // Arrange
            ContractStatus status = ContractStatus.OnHold;

            // Act
            bool canCreateRequest =
                status == ContractStatus.Active;

            // Assert
            Assert.False(canCreateRequest);
        }
    }
}