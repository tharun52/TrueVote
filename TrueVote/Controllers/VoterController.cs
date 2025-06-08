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
        public async Task<IActionResult> GetAllVotersAsync()
        {
            var voters = await _voterService.GetAllVotersAsync();
            return Ok(ApiResponseHelper.Success(voters, "Voters fetched successfully"));
        }
        [HttpPost]
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
                return StatusCode(500, ApiResponseHelper.Failure<object>("An unexpected error occurred : "+ex.Message));
            }
        }
    }
}