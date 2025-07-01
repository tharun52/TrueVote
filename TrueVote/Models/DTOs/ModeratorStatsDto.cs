namespace TrueVote.Models.DTOs
{
    public class ModeratorStatsDto
    {
        public int TotalPollsCreated { get; set; }
        public int TotalVoterEmailsCreated { get; set; }
        public int TotalVoterEmailsUsed { get; set; }
        public int TotalVotesReceived { get; set; }
    }
}
