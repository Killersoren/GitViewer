using GitViewer.Api.Controllers;
using GitViewer.Api.Dto;
using GitViewer.Api.RabbitMQ;
using GitViewer.Api.Services.Interfaces;
using GitViewer.DataAccess.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GitViewer.Tests
{
    public class AuthTests
    {
        [Fact]
        public async Task RegisterAccountTest_ReturnOK_Async()
        {
            // Arrange
            var userDto = new UserDto { UserName = "testuser", Password = "password123" };
            var expectedUser = new User { UserName = "testuser" };

            var mockAuthService = new Mock<IAuthService>();
            var mockLoggingService = new Mock<ILoggingService>();
            mockAuthService
                .Setup(s => s.RegisterAsync(userDto))
                .ReturnsAsync(expectedUser);

            var controller = new AuthController(mockAuthService.Object, mockLoggingService.Object);

            // Act
            var result = await controller.Register(userDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedUser = Assert.IsType<User>(okResult.Value);
            Assert.Equal(expectedUser.UserName, returnedUser.UserName);
        }

        [Fact]
        public async Task Register_UserExists_ReturnBadRequest_Async()
        {
            // Arrange
            var userDto = new UserDto { UserName = "existing", Password = "password123" };

            var mockAuthService = new Mock<IAuthService>();
            var mockMessageProducer = new Mock<IMessageProducer>();
            var mockLoggingService = new Mock<ILoggingService>();
            mockAuthService
                .Setup(s => s.RegisterAsync(userDto))
                .ReturnsAsync((User?)null);

            var controller = new AuthController(mockAuthService.Object, mockLoggingService.Object);

            // Act
            var result = await controller.Register(userDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Username already exists", badRequestResult.Value);
        }
    }
}