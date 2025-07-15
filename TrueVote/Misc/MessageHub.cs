using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace TrueVote.Hubs
{
    [Authorize]
    public class MessageHub : Hub
    {
        public override Task OnConnectedAsync()
        {
            Console.WriteLine("SignalR connected: " + Context.UserIdentifier);
            return base.OnConnectedAsync();
        }
    }
}
