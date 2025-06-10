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
        [HttpGet("all")]
        public async Task<IActionResult> GetAllPollsAsync()
        {
            try
            {
                var polls = await _pollService.ViewAllPolls();
                return Ok(ApiResponseHelper.Success(polls, "Polls fetched successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure<object>("An unexpected error occurred: " + ex.Message));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPollByIdAsync(Guid id)
        {
            try
            {
                var poll = await _pollService.ViewPollById(id);
                if (poll == null)
                    return NotFound(ApiResponseHelper.Failure<object>("Poll not found"));
                return Ok(ApiResponseHelper.Success(poll, "Poll fetched successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure<object>("An unexpected error occurred: " + ex.Message));
            }
        }

        [HttpGet("by-uploader/{username}")]
        public async Task<IActionResult> GetPollsByUsernameAsync(string username)
        {
            try
            {
                var polls = await _pollService.ViewPollsByUploadedByUsername(username);
                return Ok(ApiResponseHelper.Success(polls, "Polls fetched successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure<object>("An unexpected error occurred: " + ex.Message));
            }
        }
    }
}
