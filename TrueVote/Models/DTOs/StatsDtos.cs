namespace TrueVote.Models.DTOs
{
    public class ModeratorStatsDto
    {
        public int TotalPollsCreated { get; set; }
        public int TotalVoterEmailsCreated { get; set; }
        public int TotalVoterEmailsUsed { get; set; }
        public int TotalVotesReceived { get; set; }
    }

    public class VoterStatsDto
    {
        public int TotalOnGoingPolls { get; set; }
        public int TotalPollsVoted { get; set; }
    }
    public class AdminStatsDto
    {
        public int TotalPollsCreated { get; set; }
        public int TotalVotesVoted { get; set; }
        public int TotalModeratorRegistered { get; set; }
        public int TotalVotersREgistered { get; set; }
    }
}
