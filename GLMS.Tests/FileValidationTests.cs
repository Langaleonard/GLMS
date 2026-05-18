using Xunit;

namespace GLMS.Tests
{
    public class FileValidationTests
    {
        [Fact]
        public void Should_Accept_Pdf_File()
        {
            // Arrange
            string fileName = "agreement.pdf";

            // Act
            bool isPdf = fileName.EndsWith(".pdf");

            // Assert
            Assert.True(isPdf);
        }

        [Fact]
        public void Should_Reject_Exe_File()
        {
            // Arrange
            string fileName = "virus.exe";

            // Act
            bool isPdf = fileName.EndsWith(".pdf");

            // Assert
            Assert.False(isPdf);
        }
    }
}