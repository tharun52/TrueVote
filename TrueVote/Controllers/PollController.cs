using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrueVote.Interfaces;
using TrueVote.Misc;
using TrueVote.Models.DTOs;

namespace TrueVote.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class PollController : ControllerBase
    {
        private readonly IPollService _pollService;

        public PollController(IPollService pollService)
        {
            _pollService = pollService;
        }

        [Authorize(Roles = "Admin, Moderator")]
        [HttpPost]
        public async Task<IActionResult> AddPollAsync([FromForm] AddPollRequestDto pollRequestDto)
        {
            if (pollRequestDto == null)
            {
                var error = new Dictionary<string, List<string>> {
                        { "pollRequestDto", new List<string> { "Poll data cannot be null" } }
                    };
                return BadRequest(ApiResponseHelper.Failure<object>("Invalid request body", error));
            }

            try
            {
                var newPoll = await _pollService.AddPoll(pollRequestDto);
                return Created($"/api/moderator/{newPoll.Title}", ApiResponseHelper.Success(newPoll, "Poll created successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<object>(ex.Message));
            }
        }

        [HttpPut("update/{pollId}")]
        [Authorize(Roles = "Admin, Moderator")]
        public async Task<IActionResult> UpdatePollAsync(Guid pollId, [FromForm] UpdatePollRequestDto updateDto)
        {
            if (updateDto == null)
            {
                var error = new Dictionary<string, List<string>> {
                    { "updateDto", new List<string> { "Poll update data cannot be null" } }
                };
                return BadRequest(ApiResponseHelper.Failure<object>("Invalid request body", error));
            }

            try
            {
                var updatedPoll = await _pollService.UpdatePoll(pollId, updateDto);
                return Ok(ApiResponseHelper.Success(updatedPoll, "Poll updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<object>(ex.Message));
            }
        }

        [HttpGet("query")]
        public async Task<IActionResult> QueryPollsAsync([FromQuery] PollQueryDto query)
        {
            try
            {
                var pagedResult = await _pollService.QueryPollsPaged(query);
                return Ok(ApiResponseHelper.Success(pagedResult, "Polls fetched successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure<object>("An unexpected error occurred: " + ex.Message));
            }
        }
        [HttpGet("{pollId}")]
        public async Task<IActionResult> GetPollByIdAsync(Guid pollId)
        {
            try
            {
                var poll = await _pollService.GetPollByIdAsync(pollId);
                return Ok(ApiResponseHelper.Success(poll, "Poll fetched successfully"));
            }
            catch (Exception ex)
            {
                return NotFound(ApiResponseHelper.Failure<object>(ex.Message));
            }
        }
    }
}
