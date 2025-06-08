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
    public class ModeratorController : ControllerBase
    {
        private readonly IModeratorService _moderatorService;

        public ModeratorController(IModeratorService moderatorService)
        {
            _moderatorService = moderatorService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllModeratorsAsync()
        {
            var moderators = await _moderatorService.GetAllModeratorsAsync();
            return Ok(ApiResponseHelper.Success(moderators, "Moderators fetched successfully"));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{name}")]
        public async Task<IActionResult> GetModeratorByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest(ApiResponseHelper.Failure<object>("Invalid moderator name"));
            }

            var moderator = await _moderatorService.GetModeratorByNameAsync(name);
            if (moderator == null)
            {
                return NotFound(ApiResponseHelper.Failure<object>("Moderator not found"));
            }

            return Ok(ApiResponseHelper.Success(moderator, "Moderator fetched successfully"));
        }

        [HttpPost]
        public async Task<IActionResult> AddModeratorAsync([FromBody] AddModeratorRequestDto moderatorDto)
        {
            if (moderatorDto == null)
            {
                var error = new Dictionary<string, List<string>> {
                    { "moderatorDto", new List<string> { "Moderator data cannot be null" } }
                };
                return BadRequest(ApiResponseHelper.Failure<object>("Invalid request body", error));
            }

            var newModerator = await _moderatorService.AddModerator(moderatorDto);

            if (newModerator == null)
            {
                var error = new Dictionary<string, List<string>> {
                    { "moderator", new List<string> { "Failed to add moderator" } }
                };
                return BadRequest(ApiResponseHelper.Failure<object>("Moderator creation failed", error));
            }

            return Created($"/api/moderator/{newModerator.Id}", ApiResponseHelper.Success(newModerator, "Moderator added successfully"));
        }
    }
}