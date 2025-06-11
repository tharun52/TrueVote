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

        [HttpPost]
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
       

        [HttpPut("update/{username}")]
        [Authorize(Roles = "Admin, Moderator")]
        public async Task<IActionResult> UpdateModeratorAsync(string username, [FromBody] UpdateModeratorDto moderatorDto)
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
                var updatedModerator = await _moderatorService.UpdateModerator(username, moderatorDto);
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