using TrueVote.Models.DTOs;
using Microsoft.AspNetCore.SignalR;

namespace TrueVote.Misc
{
    public class PollHub : Hub
    {
        public async Task BroadcastPollUpdate(PollResponseDto poll)
        {
            await Clients.All.SendAsync("ReceivePollUpdate", poll);
        }

        public async Task GetAllPolls(List<PollResponseDto> polls)
        {
            await Clients.Caller.SendAsync("ReceiveAllPolls", polls);
        }
    }
}