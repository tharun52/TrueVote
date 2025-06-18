using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TrueVote.Controllers;
using TrueVote.Interfaces;
using TrueVote.Models;
using TrueVote.Models.DTOs;
using Xunit;

namespace TrueVote.Tests.Controllers
{
    public class AdminControllerTests
    {
        private readonly Mock<IAdminService> _mockAdminService;
        private readonly AdminController _controller;

        public AdminControllerTests()
        {
            _mockAdminService = new Mock<IAdminService>();
            _controller = new AdminController(_mockAdminService.Object);
        }

        [Fact]
        public async Task AddAdminAsync_ValidRequest_ReturnsCreatedResult()
        {
            var dto = new AddAdminRequestDto { Email = "admin@example.com", Password = "Pass123", Name = "Admin" };
            var expectedAdmin = new Admin { Email = dto.Email, Name = dto.Name };

            _mockAdminService.Setup(s => s.AddAdmin(dto)).ReturnsAsync(expectedAdmin);

            var result = await _controller.AddAdminAsync(dto) as CreatedResult;

            Assert.NotNull(result);
            Assert.Equal(201, result.StatusCode);
            Assert.Equal($"/api/admin/{expectedAdmin.Name}", result.Location);
        }

        [Fact]
        public async Task AddAdminAsync_NullDto_ReturnsBadRequest()
        {
            var result = await _controller.AddAdminAsync(null) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task AddAdminAsync_Unauthorized_ThrowsUnauthorizedAccessException()
        {
            var dto = new AddAdminRequestDto();
            _mockAdminService.Setup(s => s.AddAdmin(dto)).ThrowsAsync(new UnauthorizedAccessException("Not allowed"));

            var result = await _controller.AddAdminAsync(dto) as UnauthorizedObjectResult;

            Assert.NotNull(result);
            Assert.Equal(401, result.StatusCode);
        }

        [Fact]
        public async Task UpdateAdminAsync_ValidRequest_ReturnsOk()
        {
            var email = "admin@example.com";
            var dto = new UpdateAdminRequest { PrevPassword = "old", NewPassword = "new", Name = "UpdatedAdmin" };
            var updatedAdmin = new Admin { Email = email, Name = dto.Name };

            _mockAdminService.Setup(s => s.UpdateAdmin(email, dto.PrevPassword, dto.NewPassword, dto.Name))
                             .ReturnsAsync(updatedAdmin);

            var result = await _controller.UpdateAdminAsync(email, dto) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task UpdateAdminAsync_NullDto_ReturnsBadRequest()
        {
            var result = await _controller.UpdateAdminAsync("admin@example.com", null) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task UpdateAdminAsync_MissingPrevPassword_ReturnsBadRequest()
        {
            var dto = new UpdateAdminRequest { PrevPassword = null };
            var result = await _controller.UpdateAdminAsync("admin@example.com", dto) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task GetAdminByIdAsync_AdminExists_ReturnsOk()
        {
            var id = Guid.NewGuid();
            var admin = new Admin { Id = id, Email = "admin@example.com", Name = "Admin" };

            _mockAdminService.Setup(s => s.GetAdminByIdAsync(id)).ReturnsAsync(admin);

            var result = await _controller.GetAdminByIdAsync(id) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task GetAdminByIdAsync_NotFound_ReturnsNotFound()
        {
            var id = Guid.NewGuid();
            _mockAdminService.Setup(s => s.GetAdminByIdAsync(id)).ThrowsAsync(new Exception("Admin not found"));

            var result = await _controller.GetAdminByIdAsync(id) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task DeleteAdminAsync_Success_ReturnsOk()
        {
            var id = Guid.NewGuid();
            _mockAdminService.Setup(s => s.DeleteAdminAsync(id)).ReturnsAsync(true);

            var result = await _controller.DeleteAdminAsync(id) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task DeleteAdminAsync_NotFound_ReturnsNotFound()
        {
            var id = Guid.NewGuid();
            _mockAdminService.Setup(s => s.DeleteAdminAsync(id)).ReturnsAsync(false);

            var result = await _controller.DeleteAdminAsync(id) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task DeleteAdminAsync_Exception_Returns500()
        {
            var id = Guid.NewGuid();
            _mockAdminService.Setup(s => s.DeleteAdminAsync(id)).ThrowsAsync(new Exception("DB Error"));

            var result = await _controller.DeleteAdminAsync(id) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
        }
    }
}
