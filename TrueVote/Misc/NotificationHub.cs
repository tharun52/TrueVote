using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TrueVote.Interfaces;

namespace TrueVote.Misc
{
    [Authorize]
    public class NotificationHub : Hub
    {
        
    }
}