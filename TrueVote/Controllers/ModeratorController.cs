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

        [HttpGet("stats/{moderatorId}")]
        public async Task<ActionResult<ModeratorStatsDto>> GetModeratorStats(Guid moderatorId)
        {
            if (moderatorId == Guid.Empty)
            {
                return BadRequest("Invalid moderator ID.");
            }

            var stats = await _moderatorService.GetModeratorStats(moderatorId);

            if (stats == null)
            {
                return NotFound("Moderator not found or no stats available.");
            }

            return Ok(stats);
        }

        [HttpGet("query")]
        public async Task<IActionResult> QueryModeratorsAsync([FromQuery] ModeratorQueryDto query)
        {
            try
            {
                var pagedResult = await _moderatorService.QueryModeratorsPaged(query);
                return Ok(ApiResponseHelper.Success(pagedResult, "Moderators fetched successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure<object>("An unexpected error occurred: " + ex.Message));
            }
        }
       
        [HttpGet("{moderatorId}")]
        public async Task<IActionResult> GetModeratorByIdAsync(Guid moderatorId)
        {
            try
            {
                var moderator = await _moderatorService.GetModeratorByIdAsync(moderatorId);
                return Ok(ApiResponseHelper.Success(moderator, "Moderator fetched successfully"));
            }
            catch (Exception ex)
            {
                return NotFound(ApiResponseHelper.Failure<object>(ex.Message));
            }
        }

        [HttpGet("email/{email}")]
        public async Task<IActionResult> GetModeratorByEmailAsync(string email)
        {
            try
            {
                var moderator = await _moderatorService.GetModeratorByEmailAsync(email);
                return Ok(ApiResponseHelper.Success(moderator, "Moderator fetched successfully"));
            }
            catch (Exception ex)
            {
                return NotFound(ApiResponseHelper.Failure<object>(ex.Message));
            }
        }

        [HttpPost("add")]
        [Authorize(Roles = "Admin")]
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

        [HttpPut("update")]
        [Authorize(Roles = "Admin, Moderator")]
        public async Task<IActionResult> UpdateModeratorAsync([FromBody] UpdateModeratorDto moderatorDto)
        {
            if (moderatorDto == null)
            {
                var error = new Dictionary<string, List<string>> {
                        { "moderatorDto", new List<string> { "Moderator data cannot be null" } }
                    };
                return BadRequest(ApiResponseHelper.Failure<object>("Invalid request body", error));
            }

            try
            {
                var updatedModerator = await _moderatorService.UpdateModerator(moderatorDto);
                return Ok(ApiResponseHelper.Success(updatedModerator, "Moderator updated successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                var error = new Dictionary<string, List<string>> {
                        { "password", new List<string> { ex.Message } }
                    };
                return Unauthorized(ApiResponseHelper.Failure<object>("Password verification failed", error));
            }
            catch (Exception ex)
            {
                var error = new Dictionary<string, List<string>> {
                        { "exception", new List<string> { ex.Message } }
                    };
                return BadRequest(ApiResponseHelper.Failure<object>("Moderator updation failed", error));
            }
        }

        [HttpPut("updateasadmin/{moderatorId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateModeratorAsync(Guid moderatorId, [FromBody] UpdateModeratorasAdminDto moderatorDto)
        {
            if (moderatorDto == null)
            {
                var error = new Dictionary<string, List<string>> {
                        { "moderatorDto", new List<string> { "Moderator data cannot be null" } }
                    };
                return BadRequest(ApiResponseHelper.Failure<object>("Invalid request body", error));
            }

            try
            {
                var updatedModerator = await _moderatorService.UpdateModeratorAsAdmin(moderatorId, moderatorDto);
                return Ok(ApiResponseHelper.Success(updatedModerator, "Moderator updated successfully"));
            }
            catch (Exception ex)
            {
                var error = new Dictionary<string, List<string>> {
                        { "exception", new List<string> { ex.Message } }
                    };
                return BadRequest(ApiResponseHelper.Failure<object>("Moderator updation failed", error));
            }
        }

        [HttpDelete("delete/{moderatorId}")]
        [Authorize(Roles = "Admin, Moderator")]
        public async Task<IActionResult> DeleteModeratorAsync(Guid moderatorId)
        {
            var deletedModerator = await _moderatorService.DeleteModerator(moderatorId);
            if (deletedModerator == null)
            {
                var error = new Dictionary<string, List<string>> {
                    { "moderator", new List<string> { "Failed to delete moderator" } }
                };
                return BadRequest(ApiResponseHelper.Failure<object>("Moderator deletion failed", error));
            }
            return Created($"/api/moderator/{deletedModerator.Id}", ApiResponseHelper.Success(deletedModerator, "Moderator deleted successfully"));
        }
    }
}