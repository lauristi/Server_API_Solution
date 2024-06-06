using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Server_API.Controllers;
using Server_API.Model.clipboard;

namespace Server_API.Tests
{
    public class ControllerTests
    {
        private readonly Mock<ICrypto> _cryptoMock;
        private readonly Mock<ILogger<ClipboardController>> _loggerMock;
        private readonly ClipboardController _controller;

        public ControllerTests()
        {
            _cryptoMock = new Mock<ICrypto>();
            _loggerMock = new Mock<ILogger<ClipboardController>>();
            _controller = new ClipboardController(_cryptoMock.Object, _loggerMock.Object);
        }

        //---------------------------------------------------------------------------
        //Basic tests
        //---------------------------------------------------------------------------

        [Fact]
        public async Task PostClipBoard_ShouldEncryptContent()
        {
            // Arrange
            var transmition = new Transmition { Content = "test content" };
            _cryptoMock.Setup(c => c.Encrypt(It.IsAny<string>())).Returns("encrypted content");

            // Act
            var result = await _controller.PostClipBoard(transmition);

            // Assert
            var okResult = Assert.IsType<OkResult>(result);
            _cryptoMock.Verify(c => c.Encrypt("test content"), Times.Once);
        }

        [Fact]
        public async Task GetClipBoard_ShouldReturnDecryptedContent()
        {
            // Arrange
            _cryptoMock.Setup(c => c.Decrypt(It.IsAny<string>())).Returns("decrypted content");
            await _controller.PostClipBoard(new Transmition { Content = "test content" });

            // Act
            var result = await _controller.GetClipBoard();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var clipboardResponse = Assert.IsType<ClipboardResponse>(okResult.Value);
            Assert.Equal("decrypted content", clipboardResponse.Clipboard);
            _cryptoMock.Verify(c => c.Decrypt(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetClipBoard_ShouldReturnEmptyClipboardResponseIfNoData()
        {
            // Act
            var result = await _controller.GetClipBoard();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var clipboardResponse = Assert.IsType<ClipboardResponse>(okResult.Value);
            Assert.Null(clipboardResponse.Clipboard);
        }
    }
}