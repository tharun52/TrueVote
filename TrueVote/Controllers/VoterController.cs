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

        [HttpGet]
        [Authorize(Roles = "Admin, Moderator")]
        public async Task<IActionResult> GetAllVotersAsync()
        {
            var voters = await _voterService.GetAllVotersAsync();
            return Ok(ApiResponseHelper.Success(voters, "Voters fetched successfully"));
        }

        [HttpPost("add")]  
        [Authorize(Roles = "Admin, Moderator")]
        public async Task<IActionResult> AddVoterAsync([FromBody] AddVoterRequestDto voterDto)
        {
            if (voterDto == null)
            {

                return BadRequest(ApiResponseHelper.Failure<object>("Invalid request body"));
            }

            try
            {
                var newVoter = await _voterService.AddVoterAsync(voterDto);
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

        [HttpPut("update/{email}")]
        public async Task<IActionResult> UpdateVoterAsync(string email, [FromBody] UpdateVoterDto dto)
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
                var updatedVoter = await _voterService.UpdateVoterAsync(email, dto);
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

        [HttpDelete("delete/{voterId}")]
        [Authorize(Roles = "Admin, Moderator")]
        public async Task<IActionResult> DeleteVoterAsync(Guid voterId)
        {
            var deletedVoter = await _voterService.DeleteVoterAsync(voterId);
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