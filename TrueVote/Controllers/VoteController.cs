using Microsoft.AspNetCore.Mvc;
using TrueVote.Interfaces;
using TrueVote.Misc;
using TrueVote.Models;

namespace TrueVote.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class VoteController : ControllerBase
    {
        private readonly IVoteService _voteService;

        public VoteController(IVoteService voteService)
        {
            _voteService = voteService;
        }

        [HttpPost]
        public async Task<IActionResult> AddVoteAsync([FromForm] Guid? pollOptionId)
        {
            if (pollOptionId == null)
            {
                return BadRequest(ApiResponseHelper.Failure<object>("Invalid request body"));
            }
            try
            {
                var newVote = await _voteService.AddVoteAsync((Guid)pollOptionId);
                return Created($"/api/vote/{newVote.Id}", ApiResponseHelper.Success(newVote, "Voter added successfully"));
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
    }
}