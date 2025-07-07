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
    public class VoterController : ControllerBase
    {
        private readonly IVoterService _voterService;

        public VoterController(IVoterService voterService)
        {
            _voterService = voterService;
        }

        [HttpGet("check-email")]
        public async Task<IActionResult> CheckEmail([FromQuery] string email, [FromQuery] bool isVoter)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(new { message = "Email is required." });
            }

            var exists = await _voterService.CheckEmail(email, isVoter);

            return Ok(exists);
        }

        [HttpGet("stats/{voterId}")]
        public async Task<ActionResult<VoterStatsDto>> GetVoterStats(Guid voterId)
        {
            if (voterId == Guid.Empty)
            {
                return BadRequest("Invalid voter ID.");
            }

            var stats = await _voterService.GetVoterStats(voterId);

            if (stats == null)
            {
                return NotFound("Voter not found or no stats available.");
            }

            return Ok(stats);
        }

        [HttpGet]
        [Authorize(Roles = "Admin, Moderator")]
        public async Task<IActionResult> GetAllVotersAsync()
        {
            var voters = await _voterService.GetAllVoters();
            return Ok(ApiResponseHelper.Success(voters, "Voters fetched successfully"));
        }

        [HttpGet("moderator/emails")]
        [Authorize(Roles = "Moderator")]
        public async Task<IActionResult> GetWhiteListedEmailsByModeratorAsync()
        {
            var moderatorId = User?.FindFirst("UserId")?.Value;

            if (string.IsNullOrEmpty(moderatorId) || !Guid.TryParse(moderatorId, out Guid moderatorGuid))
            {
                return Unauthorized(ApiResponseHelper.Failure<object>("Invalid or missing moderator ID"));
            }

            try
            {
                var voters = await _voterService.GetWhiteListedEmailsByModerator(moderatorGuid);
                return Ok(ApiResponseHelper.Success(voters, "Voters fetched successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure<object>($"Error: {ex.Message}"));
            }
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddVoterAsync([FromBody] AddVoterRequestDto voterDto)
        {
            if (voterDto == null)
            {

                return BadRequest(ApiResponseHelper.Failure<object>("Invalid request body"));
            }

            try
            {
                var newVoter = await _voterService.AddVoter(voterDto);
                return Created($"/api/moderator/{newVoter.Name}", ApiResponseHelper.Success(newVoter, "Voter added successfully"));
            }
            catch (ArgumentException ex)
            {
                var error = new Dictionary<string, List<string>> {
                    { "voter", new List<string> { ex.Message } }
                };
                return BadRequest(ApiResponseHelper.Failure<object>("Invalid voter data", error));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure<object>("An unexpected error occurred : " + ex.Message));
            }
        }

        [HttpGet("moderator/voters")]
        [Authorize(Roles = "Moderator")]
        public async Task<IActionResult> GetVotersByModeratorAsync()
        {
            var moderatorId = User?.FindFirst("UserId")?.Value;

            if (string.IsNullOrEmpty(moderatorId) || !Guid.TryParse(moderatorId, out Guid moderatorGuid))
            {
                return Unauthorized(ApiResponseHelper.Failure<object>("Invalid or missing moderator ID"));
            }

            try
            {
                var voters = await _voterService.GetVotersByModeratorId(moderatorGuid);
                return Ok(ApiResponseHelper.Success(voters, "Voters fetched successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure<object>($"Error: {ex.Message}"));
            }
        }

        [HttpGet("{email}")]
        [Authorize(Roles = "Moderator, Voter")]
        public async Task<IActionResult> GetVoterByEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return Unauthorized(ApiResponseHelper.Failure<object>("Missing Email"));
            }

            try
            {
                var voters = await _voterService.GetVoterByEmail(email);
                return Ok(ApiResponseHelper.Success(voters, "Voter fetched successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure<object>($"Error: {ex.Message}"));
            }
        }

        [HttpPut("updateasadmin/{voterId}")]
        [Authorize(Roles = "Admin, Moderator")]
        public async Task<IActionResult> UpdateVoterAsModeratorAsync(Guid voterId, [FromBody] UpdateVoterAsModeratorDto dto)
        {
            if (dto == null)
            {
                var error = new Dictionary<string, List<string>> {
                    { "updateVoterDto", new List<string> { "Voter update data cannot be null" } }
                };
                return BadRequest(ApiResponseHelper.Failure<object>("Invalid request body", error));
            }

            try
            {
                var updatedVoter = await _voterService.UpdateVoterAsModerator(voterId, dto);
                return Ok(ApiResponseHelper.Success(updatedVoter, "Voter updated successfully"));
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
                return BadRequest(ApiResponseHelper.Failure<object>("Voter update failed", error));
            }
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateVoterAsync([FromBody] UpdateVoterDto dto)
        {
            if (dto == null)
            {
                var error = new Dictionary<string, List<string>> {
                    { "updateVoterDto", new List<string> { "Voter update data cannot be null" } }
                };
                return BadRequest(ApiResponseHelper.Failure<object>("Invalid request body", error));
            }

            try
            {
                var updatedVoter = await _voterService.UpdateVoter(dto);
                return Ok(ApiResponseHelper.Success(updatedVoter, "Voter updated successfully"));
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
                return BadRequest(ApiResponseHelper.Failure<object>("Voter update failed", error));
            }
        }
        [HttpDelete("delete/whitelist/{email}")]
        [Authorize(Roles = "Moderator")]
        public async Task<IActionResult> DeleteWhitelistEmailAsync(string email)
        {
            if (email == null)
            {
                return BadRequest(ApiResponseHelper.Failure<object>("Invalid Email"));
            }

            try
            {
                var result = await _voterService.DeleteWhitelistEmail(email);
                return Ok(ApiResponseHelper.Success(result, $"{email} deleted successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure<object>($"Error: {ex.Message}"));
            }
        }

        [HttpPost("whitelist")]
        [Authorize(Roles = "Moderator")]
        public async Task<IActionResult> WhitelistVoterEmailsAsync([FromBody] WhitelistVoterEmailDto dto)
        {
            if (dto == null || dto.Emails == null || dto.Emails.Count == 0)
            {
                return BadRequest(ApiResponseHelper.Failure<object>("No valid emails provided"));
            }

            try
            {
                var result = await _voterService.WhitelistVoterEmails(dto);
                return Ok(ApiResponseHelper.Success(result, $"{result.Count} email(s) successfully whitelisted"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure<object>($"Error: {ex.Message}"));
            }
        }




        [HttpDelete("delete/{voterId}")]
        [Authorize(Roles = "Admin, Moderator")]
        public async Task<IActionResult> DeleteVoterAsync(Guid voterId)
        {
            var deletedVoter = await _voterService.DeleteVoter(voterId);
            if (deletedVoter == null)
            {
                var error = new Dictionary<string, List<string>> {
                    {"voter", new List<string>{"Failed to delete voter"}}
                };
                return BadRequest(ApiResponseHelper.Failure<object>("Moderator deletion failed", error));
            }
            return Created($"/api/voter/{deletedVoter.Id}", ApiResponseHelper.Success(deletedVoter, "Voter Deleted Succesfully"));
        }
    }
}