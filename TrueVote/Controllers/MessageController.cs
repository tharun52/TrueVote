using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TrueVote.Hubs;
using TrueVote.Interfaces;
using TrueVote.Misc;
using TrueVote.Models;
using TrueVote.Models.DTOs;

namespace TrueVote.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class MessageController : ControllerBase
    {
        private readonly IMessageService _messageService;
        private readonly IUserService _userService;
        private readonly IHubContext<MessageHub> _hubContext;

        public MessageController(IMessageService messageService,IUserService userService, IHubContext<MessageHub> hubContext)
        {
            _messageService = messageService;
            _userService = userService;
            _hubContext = hubContext;
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

                if (message.To == null)
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveMessage", message);
                }
                else
                {
                    var userObject = await _userService.GetUserDetailsByIdAsync(message.To.Value.ToString());

                    string toEmail = userObject switch
                    {
                        Voter voter => voter.Email,
                        Moderator moderator => moderator.Email,
                        _ => throw new InvalidOperationException("Unknown user type")
                    };

                    await _hubContext.Clients.User(toEmail).SendAsync("ReceiveMessage", message);
                }

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
                var deletedMessage = await _messageService.DeleteMessage(messageId);

                await _hubContext.Clients.All.SendAsync("DeleteMessage", deletedMessage.Id);

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

        [Authorize(Roles = "Moderator, Voter")]
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
