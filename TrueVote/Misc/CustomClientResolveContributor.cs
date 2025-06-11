using AspNetCoreRateLimit;

namespace TrueVote.Misc
{
    public class CustomClientResolveContributor : IClientResolveContributor
    {
        public Task<string> ResolveClientAsync(HttpContext httpContext)
        {
            var userId = httpContext.User?.Claims
                .FirstOrDefault(c => c.Type == "UserId")?.Value;

            return Task.FromResult(userId ?? "anonymous");
        }
    }
}