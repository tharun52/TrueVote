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

public class PollControllerTests
{
    private readonly Mock<IPollService> _mockService;
    private readonly PollController _controller;

    public PollControllerTests()
    {
        _mockService = new Mock<IPollService>();
        _controller = new PollController(_mockService.Object);
    }

    [Fact]
    public async Task AddPollAsync_ValidRequest_ReturnsCreated()
    {
        var request = new AddPollRequestDto { Title = "Election 2024" };
        var newPoll = new Poll { Id = Guid.NewGuid(), Title = request.Title };

        _mockService.Setup(s => s.AddPoll(request)).ReturnsAsync(newPoll);

        var result = await _controller.AddPollAsync(request);
        var createdResult = Assert.IsType<CreatedResult>(result);
        Assert.Equal(201, createdResult.StatusCode);
    }

    [Fact]
    public async Task AddPollAsync_NullRequest_ReturnsBadRequest()
    {
        var result = await _controller.AddPollAsync(null);
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequest.StatusCode);
    }

    [Fact]
    public async Task UpdatePollAsync_ValidRequest_ReturnsOk()
    {
        var pollId = Guid.NewGuid();
        var updateDto = new UpdatePollRequestDto { Title = "Updated Title" };
        var updatedPoll = new Poll { Id = pollId, Title = updateDto.Title };

        _mockService.Setup(s => s.UpdatePoll(pollId, updateDto)).ReturnsAsync(updatedPoll);

        var result = await _controller.UpdatePollAsync(pollId, updateDto);
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task UpdatePollAsync_NullDto_ReturnsBadRequest()
    {
        var result = await _controller.UpdatePollAsync(Guid.NewGuid(), null);
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequest.StatusCode);
    }

    [Fact]
    public async Task DeletePollAsync_ValidId_ReturnsOk()
    {
        var id = Guid.NewGuid();
        _mockService.Setup(s => s.DeletePollAsync(id)).ReturnsAsync(true);

        var result = await _controller.DeletePollAsync(id);
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task DeletePollAsync_InvalidId_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        _mockService.Setup(s => s.DeletePollAsync(id)).ReturnsAsync(false);

        var result = await _controller.DeletePollAsync(id);
        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFound.StatusCode);
    }




    [Fact]
    public async Task GetPollByIdAsync_NotFound_ReturnsNotFound()
    {
        var pollId = Guid.NewGuid();

        _mockService.Setup(s => s.GetPollByIdAsync(pollId))
            .ThrowsAsync(new Exception("Poll not found"));

        var result = await _controller.GetPollByIdAsync(pollId);
        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFound.StatusCode);
    }
}
