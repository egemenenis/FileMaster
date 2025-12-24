using FileMaster.Api.Controllers;
using FileMaster.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;

namespace FileMaster.Tests
{
    public class FilesControllerTests
    {
        private readonly Mock<AppDbContext> _mockContext;
        private readonly Mock<ILogger<FilesController>> _mockLogger;

        public FilesControllerTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _mockContext = new Mock<AppDbContext>(options);
            _mockLogger = new Mock<ILogger<FilesController>>();
        }

        private void SetupControllerContext(FilesController controller)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
                new Claim(ClaimTypes.Name, "test@example.com")
            }, "mock"));

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        [Fact]
        public async Task Upload_InvalidExtension_ReturnsBadRequest()
        {
            var controller = new FilesController(_mockContext.Object, _mockLogger.Object);
            SetupControllerContext(controller);

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(_ => _.FileName).Returns("virus.exe");
            fileMock.Setup(_ => _.Length).Returns(100);

            var result = await controller.Upload(fileMock.Object);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Bu dosya tipine izin verilmiyor.", badRequest.Value);
        }

        [Fact]
        public async Task Upload_FileTooLarge_ReturnsBadRequest()
        {
            var controller = new FilesController(_mockContext.Object, _mockLogger.Object);
            SetupControllerContext(controller);

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(_ => _.FileName).Returns("resim.jpg");
            fileMock.Setup(_ => _.Length).Returns(20 * 1024 * 1024);

            var result = await controller.Upload(fileMock.Object);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Dosya boyutu çok büyük (Maks 10MB).", badRequest.Value);
        }
    }
}