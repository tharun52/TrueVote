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
    public class MessageController : ControllerBase
    {
        private readonly IMessageService _messageService;

        public MessageController(IMessageService messageService)
        {
            _messageService = messageService;
        }

        [Authorize(Roles = "Moderator, Voter")]
        [HttpPost("add")]
        public async Task<IActionResult> AddMessage([FromBody] MessageRequestDto messageDto)
        {
            if (messageDto == null || string.IsNullOrWhiteSpace(messageDto.Msg))
            {
                var error = new Dictionary<string, List<string>> {
                    { "Msg", new List<string> { "Message cannot be empty" } }
                };
                return BadRequest(ApiResponseHelper.Failure<object>("Invalid message request", error));
            }

            try
            {
                var message = await _messageService.AddMessage(messageDto);
                return Created($"/api/message/{message.Id}", ApiResponseHelper.Success(message, "Message sent successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<object>(ex.Message));
            }
        }

        [Authorize(Roles = "Moderator, Voter")]
        [HttpDelete("delete/{messageId}")]
        public async Task<IActionResult> DeleteMessage(Guid messageId)
        {
            try
            {
                await _messageService.DeleteMessage(messageId);
                return Ok(ApiResponseHelper.Success<object?>(null, "Message deleted successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<object>(ex.Message));
            }
        }

        [Authorize(Roles = "Moderator, Voter")]
        [HttpDelete("clear/{messageId}")]
        public async Task<IActionResult> ClearMessage(Guid messageId)
        {
            try
            {
                await _messageService.ClearUserMessage(messageId);
                return Ok(ApiResponseHelper.Success<object?>(null, "Message cleared for user"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<object>(ex.Message));
            }
        }

        [Authorize(Roles = "Moderator, Voter")]
        [HttpDelete("clear-all")]
        public async Task<IActionResult> ClearAllMessages()
        {
            try
            {
                await _messageService.ClearAllMessages();
                return Ok(ApiResponseHelper.Success<object?>(null, "All messages cleared for user"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<object>(ex.Message));
            }
        }

        [Authorize(Roles = "Moderator, Voter")]
        [HttpGet("inbox")]
        public async Task<IActionResult> GetInbox()
        {
            try
            {
                var messages = await _messageService.GetMessageForVoter();
                return Ok(ApiResponseHelper.Success(messages, "Messages fetched successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure<object>("Error: " + ex.Message));
            }
        }

        [Authorize(Roles = "Moderator")]
        [HttpGet("sent")]
        public async Task<IActionResult> GetSentMessages()
        {
            try
            {
                var messages = await _messageService.GetMessagesForModerator();
                return Ok(ApiResponseHelper.Success(messages, "Sent messages fetched successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure<object>("Error: " + ex.Message));
            }
        }

        [Authorize(Roles = "Moderator")]
        [HttpPost("cleanup")]
        public async Task<IActionResult> CleanupOldMessages()
        {
            try
            {
                await _messageService.CleanupOldUserMessagesAsync();
                return Ok(ApiResponseHelper.Success<object?>(null, "Old messages cleaned up"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<object>(ex.Message));
            }
        }
    }
}
