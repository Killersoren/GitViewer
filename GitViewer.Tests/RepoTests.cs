using FluentResults;
using GitViewer.Api;
using GitViewer.Api.Controllers;
using GitViewer.Api.Dto;
using GitViewer.Api.Services;
using GitViewer.DataAccess.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Net;
using System.Security.Claims;

namespace GitViewer.Tests
{
    public class RepoTests
    {
        private readonly GitViewerServiceContext _context;

        public RepoTests()
        {
            var options = new DbContextOptionsBuilder<GitViewerServiceContext>()
                .UseInMemoryDatabase("TestDb")
                .Options;

            _context = new GitViewerServiceContext(options);
        }

        [Fact]
        public async Task GetRepoTest_Success_Async()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var mockRepositoryService = new Mock<IRepositoryService>();
            var mockGitFileService = new Mock<IGitFileService>();
            var mockGitRepoManager = new Mock<IGitRepoManager>();

            var repo = new Repository
            {
                Id = Guid.NewGuid(),
                Name = "TestRepo",
                Source = "https://Test.com/",
                UserId = userId,
                IsPublic = true,
                Created = DateTime.UtcNow
            };

            mockRepositoryService
                .Setup(x => x.GetRepoAsync(repo.Id, userId, It.IsAny<string>()))
                .ReturnsAsync(Result.Ok(repo));

            var controller = new RepoController(
                mockRepositoryService.Object,
                mockGitFileService.Object,
                mockGitRepoManager.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, "User")
            }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
            controller.ControllerContext.HttpContext.Connection.RemoteIpAddress = IPAddress.Loopback;

            // Act
            var result = await controller.GetRepo(repo.Id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedRepo = Assert.IsType<Repository>(okResult.Value);
            Assert.Equal(repo.Id, returnedRepo.Id);
            Assert.Equal(repo.Name, returnedRepo.Name);
        }

        [Fact]
        public async Task GetRepoTest_NotFound_Async()
        {
            // Arrange
            var mockRepositoryService = new Mock<IRepositoryService>();
            var mockGitFileService = new Mock<IGitFileService>();
            var mockGitRepoManager = new Mock<IGitRepoManager>();

            var nonExistentRepoId = Guid.NewGuid();

            mockRepositoryService
                .Setup(x => x.GetRepoAsync(nonExistentRepoId, It.IsAny<Guid?>(), It.IsAny<string>()))
                .ReturnsAsync(Result.Fail("Repository not found"));

            var controller = new RepoController(
                mockRepositoryService.Object,
                mockGitFileService.Object,
                mockGitRepoManager.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, "User")
            }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await controller.GetRepo(nonExistentRepoId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Repository does not exist", notFoundResult.Value);
        }

        [Fact]
        public async Task GetRepoTest_Unauthorized_Async()
        {
            // Arrange
            var mockRepositoryService = new Mock<IRepositoryService>();
            var mockGitFileService = new Mock<IGitFileService>();
            var mockGitRepoManager = new Mock<IGitRepoManager>();

            var repoId = Guid.NewGuid();

            mockRepositoryService
                .Setup(x => x.GetRepoAsync(repoId, null, It.IsAny<string>()))
                .ReturnsAsync(Result.Fail("Unauthorized"));

            var controller = new RepoController(
                mockRepositoryService.Object,
                mockGitFileService.Object,
                mockGitRepoManager.Object);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await controller.GetRepo(repoId);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedResult>(result.Result);
            Assert.NotNull(unauthorizedResult);
        }

        [Fact]
        public async Task CreateRepo_Success_Async()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var mockRepositoryService = new Mock<IRepositoryService>();
            var mockGitFileService = new Mock<IGitFileService>();
            var mockGitRepoManager = new Mock<IGitRepoManager>();

            var createRepoDto = new RepositoryDto
            {
                Source = "https://NewTest.com/",
            };

            var createdRepo = new Repository
            {
                Id = Guid.NewGuid(),
                Name = "NewTest",
                Source = createRepoDto.Source,
                UserId = userId,
                IsPublic = false,
                Created = DateTime.UtcNow
            };

            mockRepositoryService
                .Setup(x => x.AddRepoAsync(createRepoDto, userId))
                .ReturnsAsync(Result.Ok(createdRepo));

            var controller = new RepoController(
                mockRepositoryService.Object,
                mockGitFileService.Object,
                mockGitRepoManager.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, "User")
            }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await controller.AddRepo(createRepoDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedRepo = Assert.IsType<Repository>(okResult.Value);
            Assert.Equal("NewTest", returnedRepo.Name);
            Assert.Equal(createRepoDto.Source, returnedRepo.Source);
            Assert.Equal(userId, returnedRepo.UserId);
        }

        [Fact]
        public async Task CreateRepo_InvalidUrl_Async()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var mockRepositoryService = new Mock<IRepositoryService>();
            var mockGitFileService = new Mock<IGitFileService>();
            var mockGitRepoManager = new Mock<IGitRepoManager>();

            var invalidRepoDto = new RepositoryDto
            {
                Source = "not a valid url",
            };

            mockRepositoryService
                .Setup(x => x.AddRepoAsync(invalidRepoDto, userId))
                .ReturnsAsync(Result.Fail("Invalid URL format"));

            var controller = new RepoController(
                mockRepositoryService.Object,
                mockGitFileService.Object,
                mockGitRepoManager.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, "User")
            }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await controller.AddRepo(invalidRepoDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Invalid URL format", badRequestResult.Value);
        }

        [Fact]
        public async Task DeleteRepo_Success_Async()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var repoId = Guid.NewGuid();
            var mockRepositoryService = new Mock<IRepositoryService>();
            var mockGitFileService = new Mock<IGitFileService>();
            var mockGitRepoManager = new Mock<IGitRepoManager>();

            mockRepositoryService
                .Setup(x => x.DeleteRepoAsync(repoId, userId))
                .ReturnsAsync(Result.Ok());

            var controller = new RepoController(
                mockRepositoryService.Object,
                mockGitFileService.Object,
                mockGitRepoManager.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, "User")
            }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await controller.DeleteRepo(repoId);

            // Assert
            var okResult = Assert.IsType<OkResult>(result);
            mockRepositoryService.Verify(x => x.DeleteRepoAsync(repoId, userId), Times.Once);
        }

        [Fact]
        public async Task DeleteRepo_NotFound_Async()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var repoId = Guid.NewGuid();
            var mockRepositoryService = new Mock<IRepositoryService>();
            var mockGitFileService = new Mock<IGitFileService>();
            var mockGitRepoManager = new Mock<IGitRepoManager>();

            mockRepositoryService
                .Setup(x => x.DeleteRepoAsync(repoId, userId))
                .ReturnsAsync(Result.Fail("Repository not found"));

            var controller = new RepoController(
                mockRepositoryService.Object,
                mockGitFileService.Object,
                mockGitRepoManager.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, "User")
            }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await controller.DeleteRepo(repoId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Repository not found", notFoundResult.Value);
        }
    }
}