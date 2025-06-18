using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using TrueVote.Controllers;
using TrueVote.Interfaces;
using TrueVote.Models.DTOs;
using TrueVote.Models;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

public class ModeratorControllerTests
{
    private readonly Mock<IModeratorService> _mockService;
    private readonly ModeratorController _controller;

    public ModeratorControllerTests()
    {
        _mockService = new Mock<IModeratorService>();
        _controller = new ModeratorController(_mockService.Object);
    }

    [Fact]
    public async Task AddModeratorAsync_ValidRequest_ReturnsCreated()
    {
        var moderatorDto = new AddModeratorRequestDto { Email = "mod@example.com" };
        var moderator = new Moderator { Id = Guid.NewGuid(), Email = "mod@example.com" };

        _mockService.Setup(s => s.AddModerator(moderatorDto)).ReturnsAsync(moderator);

        var result = await _controller.AddModeratorAsync(moderatorDto);
        var createdResult = Assert.IsType<CreatedResult>(result);
        Assert.Equal(201, createdResult.StatusCode);
    }

    [Fact]
    public async Task AddModeratorAsync_NullDto_ReturnsBadRequest()
    {
        var result = await _controller.AddModeratorAsync(null);
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequest.StatusCode);
    }

    [Fact]
    public async Task QueryModeratorsAsync_ReturnsOk()
    {
        var query = new ModeratorQueryDto();
        var mockPaged = new PagedResponseDto<Moderator>();

        _mockService.Setup(s => s.QueryModeratorsPaged(query)).ReturnsAsync(mockPaged);

        var result = await _controller.QueryModeratorsAsync(query);
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task GetModeratorByIdAsync_ReturnsOk()
    {
        var id = Guid.NewGuid();
        var moderator = new Moderator { Id = id };

        _mockService.Setup(s => s.GetModeratorByIdAsync(id)).ReturnsAsync(moderator);

        var result = await _controller.GetModeratorByIdAsync(id);
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task GetModeratorByIdAsync_Throws_ReturnsNotFound()
    {
        var id = Guid.NewGuid();

        _mockService.Setup(s => s.GetModeratorByIdAsync(id)).ThrowsAsync(new Exception("Not found"));

        var result = await _controller.GetModeratorByIdAsync(id);
        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFound.StatusCode);
    }

    [Fact]
    public async Task GetModeratorByEmailAsync_ReturnsOk()
    {
        var email = "mod@example.com";
        var moderator = new Moderator { Email = email };

        _mockService.Setup(s => s.GetModeratorByEmailAsync(email)).ReturnsAsync(moderator);

        var result = await _controller.GetModeratorByEmailAsync(email);
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task UpdateModeratorAsync_ValidRequest_ReturnsOk()
    {
        var username = "mod@example.com";
        var updateDto = new UpdateModeratorDto();
        var updated = new Moderator { Email = username };

        _mockService.Setup(s => s.UpdateModerator(username, updateDto)).ReturnsAsync(updated);

        var result = await _controller.UpdateModeratorAsync(username, updateDto);
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task UpdateModeratorAsync_NullDto_ReturnsBadRequest()
    {
        var result = await _controller.UpdateModeratorAsync("mod", null);
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequest.StatusCode);
    }

    [Fact]
    public async Task UpdateModeratorAsync_Unauthorized_ReturnsUnauthorized()
    {
        var dto = new UpdateModeratorDto();
        _mockService.Setup(s => s.UpdateModerator("mod", dto))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid password"));

        var result = await _controller.UpdateModeratorAsync("mod", dto);
        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal(401, unauthorized.StatusCode);
    }

    [Fact]
    public async Task UpdateModeratorAsync_OtherException_ReturnsBadRequest()
    {
        var dto = new UpdateModeratorDto();
        _mockService.Setup(s => s.UpdateModerator("mod", dto))
            .ThrowsAsync(new Exception("Database error"));

        var result = await _controller.UpdateModeratorAsync("mod", dto);
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequest.StatusCode);
    }

    [Fact]
    public async Task DeleteModeratorAsync_ValidId_ReturnsCreated()
    {
        var moderator = new Moderator { Id = Guid.NewGuid() };

        _mockService.Setup(s => s.DeleteModerator(moderator.Id)).ReturnsAsync(moderator);

        var result = await _controller.DeleteModeratorAsync(moderator.Id);
        var created = Assert.IsType<CreatedResult>(result);
        Assert.Equal(201, created.StatusCode);
    }

    [Fact]
    public async Task DeleteModeratorAsync_NullResult_ReturnsBadRequest()
    {
        var id = Guid.NewGuid();
        _mockService.Setup(s => s.DeleteModerator(id)).ReturnsAsync((Moderator)null);

        var result = await _controller.DeleteModeratorAsync(id);
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequest.StatusCode);
    }
}
