namespace TrueVote.Models.DTOs
{
    public class MagicLinkRequest
    {
        public string Email { get; set; } = string.Empty;
        public string ClientURI { get; set; } = string.Empty;
    }

    public class MagicLinkVerifyRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }

}