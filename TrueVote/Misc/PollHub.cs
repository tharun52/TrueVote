using TrueVote.Models.DTOs;
using Microsoft.AspNetCore.SignalR;
using TrueVote.Interfaces;

namespace TrueVote.Misc
{
    public class PollHub : Hub
    {
        private readonly IPollService _pollService;

        public PollHub(IPollService pollService)
        {
            _pollService = pollService;
        }
        public async Task GetAllPolls(List<PollResponseDto> polls)
        {
            await Clients.Caller.SendAsync("ReceiveAllPolls", polls);
        }
        public async Task GetPollsByModeratorEmail(string email)
        {
            try
            {
                var pollQuery = new PollQueryDto
                {
                    CreatedByEmail = email,
                    Page = 1,
                    PageSize = 100
                };

                var polls = await _pollService.QueryPollsPaged(pollQuery);

                foreach (var pollResp in polls.Data)
                {
                    if (pollResp.Poll?.PoleFile != null)
                    {
                        pollResp.Poll.PoleFile.Content = null!; 
                    }
                }

                var cleanPolls = polls.Data.Select(p => new
                {
                    Poll = new
                    {
                        p.Poll.Id,
                        p.Poll.Title,
                        p.Poll.Description,
                        p.Poll.CreatedByEmail,
                        p.Poll.StartDate,
                        p.Poll.EndDate,
                        p.Poll.IsDeleted,
                        PoleFile = p.Poll.PoleFile != null ? new
                        {
                            p.Poll.PoleFile.Id,
                            p.Poll.PoleFile.Filename,
                            p.Poll.PoleFile.FileType,
                            p.Poll.PoleFile.UploadedAt,
                            p.Poll.PoleFile.UploadedByUsername,
                            p.Poll.PoleFile.PollId
                        } : null
                    },
                    p.PollOptions,
                    p.PollImageBase64,
                    p.PollImageType
                }).ToList();

                await Clients.Caller.SendAsync("ReceivePollsByModerator", cleanPolls);


            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", "Error fetching polls for moderator: " + ex.Message);
            }
        }

    }
}