using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrueVote.Interfaces;
using TrueVote.Models;
using TrueVote.Service;
using Xunit;

namespace TrueVote.tests
{
    public class VoteServiceTests
    {
        private static IHttpContextAccessor GetHttpContextAccessor(string email)
        {
            var context = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(
                        new[] { new Claim(ClaimTypes.NameIdentifier, email) },
                        "mock"))
            };

            var mock = new Mock<IHttpContextAccessor>();
            mock.Setup(x => x.HttpContext).Returns(context);
            return mock.Object;
        }

        private VoteService GetVoteService(
            Mock<IRepository<Guid, VoterCheck>> voterCheckRepoMock,
            Mock<IRepository<Guid, PollVote>> pollVoteRepoMock,
            Mock<IRepository<Guid, PollOption>> pollOptionRepoMock,
            Mock<IRepository<Guid, Voter>> voterRepoMock,
            IHttpContextAccessor contextAccessor)
        {
            return new VoteService(
                voterCheckRepoMock.Object,
                pollVoteRepoMock.Object,
                pollOptionRepoMock.Object,
                contextAccessor,
                voterRepoMock.Object);
        }

        [Fact]
        public async Task AddVoteAsync_Success()
        {
            var email = "voter@example.com";
            var voter = new Voter { Id = Guid.NewGuid(), Email = email };
            var pollId = Guid.NewGuid();
            var pollOptionId = Guid.NewGuid();
            var pollOption = new PollOption { Id = pollOptionId, PollId = pollId, VoteCount = 0 };

            var voterRepoMock = new Mock<IRepository<Guid, Voter>>();
            voterRepoMock.Setup(r => r.GetAll()).ReturnsAsync(new List<Voter> { voter });

            var pollOptionRepoMock = new Mock<IRepository<Guid, PollOption>>();
            pollOptionRepoMock.Setup(r => r.GetAll()).ReturnsAsync(new List<PollOption> { pollOption });
            pollOptionRepoMock.Setup(r => r.Update(pollOptionId, It.IsAny<PollOption>()))
                .ReturnsAsync((Guid id, PollOption opt) => opt);

            var voterCheckRepoMock = new Mock<IRepository<Guid, VoterCheck>>();
            voterCheckRepoMock.Setup(r => r.GetAll()).ReturnsAsync(new List<VoterCheck>());
            voterCheckRepoMock.Setup(r => r.Add(It.IsAny<VoterCheck>()))
                .ReturnsAsync((VoterCheck check) => check);

            var pollVoteRepoMock = new Mock<IRepository<Guid, PollVote>>();
            pollVoteRepoMock.Setup(r => r.Add(It.IsAny<PollVote>()))
                .ReturnsAsync((PollVote vote) => vote);

            var contextAccessor = GetHttpContextAccessor(email);

            var service = GetVoteService(voterCheckRepoMock, pollVoteRepoMock, pollOptionRepoMock, voterRepoMock, contextAccessor);

            var result = await service.AddVoteAsync(pollOptionId);

            Assert.NotNull(result);
            Assert.Equal(pollOptionId, result.PollOptionId);
        }

        [Fact]
        public async Task AddVoteAsync_Throws_WhenUserNotLoggedIn()
        {
            var contextAccessor = new Mock<IHttpContextAccessor>();
            contextAccessor.Setup(x => x.HttpContext).Returns(new DefaultHttpContext()); // no claims

            var service = GetVoteService(
                new Mock<IRepository<Guid, VoterCheck>>(),
                new Mock<IRepository<Guid, PollVote>>(),
                new Mock<IRepository<Guid, PollOption>>(),
                new Mock<IRepository<Guid, Voter>>(),
                contextAccessor.Object);

            await Assert.ThrowsAsync<Exception>(() => service.AddVoteAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task AddVoteAsync_Throws_WhenAlreadyVoted()
        {
            var email = "voter@example.com";
            var voterId = Guid.NewGuid();
            var pollId = Guid.NewGuid();
            var optionId = Guid.NewGuid();

            var voter = new Voter { Id = voterId, Email = email };
            var pollOption = new PollOption { Id = optionId, PollId = pollId, VoteCount = 5 };

            var voterRepoMock = new Mock<IRepository<Guid, Voter>>();
            voterRepoMock.Setup(r => r.GetAll()).ReturnsAsync(new List<Voter> { voter });

            var pollOptionRepoMock = new Mock<IRepository<Guid, PollOption>>();
            pollOptionRepoMock.Setup(r => r.GetAll()).ReturnsAsync(new List<PollOption> { pollOption });

            var voterCheckRepoMock = new Mock<IRepository<Guid, VoterCheck>>();
            voterCheckRepoMock.Setup(r => r.GetAll()).ReturnsAsync(new List<VoterCheck>
        {
            new VoterCheck { VoterId = voterId, PollId = pollId }
        });

            var contextAccessor = GetHttpContextAccessor(email);

            var service = GetVoteService(
                voterCheckRepoMock, new Mock<IRepository<Guid, PollVote>>(), pollOptionRepoMock, voterRepoMock, contextAccessor);

            await Assert.ThrowsAsync<Exception>(() => service.AddVoteAsync(optionId));
        }

        [Fact]
        public async Task DeleteVoteAsync_Success()
        {
            var pollOptionId = Guid.NewGuid();
            var voteId = Guid.NewGuid();

            var vote = new PollVote { Id = voteId, PollOptionId = pollOptionId };
            var pollOption = new PollOption { Id = pollOptionId, VoteCount = 3 };

            var voteRepoMock = new Mock<IRepository<Guid, PollVote>>();
            voteRepoMock.Setup(r => r.Get(voteId)).ReturnsAsync(vote);
            voteRepoMock.Setup(r => r.Delete(voteId)).ReturnsAsync(vote);

            var optionRepoMock = new Mock<IRepository<Guid, PollOption>>();
            optionRepoMock.Setup(r => r.Get(pollOptionId)).ReturnsAsync(pollOption);
            optionRepoMock.Setup(r => r.Update(pollOptionId, It.IsAny<PollOption>()))
                .ReturnsAsync((Guid id, PollOption p) => p);

            var service = GetVoteService(
                new Mock<IRepository<Guid, VoterCheck>>(),
                voteRepoMock,
                optionRepoMock,
                new Mock<IRepository<Guid, Voter>>(),
                GetHttpContextAccessor("voter@example.com"));

            var result = await service.DeleteVoteAsync(voteId);

            Assert.Equal(voteId, result.Id);
        }

        [Fact]
        public async Task DeleteVoteAsync_Throws_WhenVoteNotFound()
        {
            var voteRepoMock = new Mock<IRepository<Guid, PollVote>>();
            voteRepoMock.Setup(r => r.Get(It.IsAny<Guid>())).ReturnsAsync((PollVote)null);

            var service = GetVoteService(
                new Mock<IRepository<Guid, VoterCheck>>(),
                voteRepoMock,
                new Mock<IRepository<Guid, PollOption>>(),
                new Mock<IRepository<Guid, Voter>>(),
                GetHttpContextAccessor("voter@example.com"));

            await Assert.ThrowsAsync<Exception>(() => service.DeleteVoteAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task DeleteVoteAsync_Throws_WhenPollOptionNotFound()
        {
            var voteId = Guid.NewGuid();
            var vote = new PollVote { Id = voteId, PollOptionId = Guid.NewGuid() };

            var voteRepoMock = new Mock<IRepository<Guid, PollVote>>();
            voteRepoMock.Setup(r => r.Get(voteId)).ReturnsAsync(vote);

            var optionRepoMock = new Mock<IRepository<Guid, PollOption>>();
            optionRepoMock.Setup(r => r.Get(vote.PollOptionId)).ReturnsAsync((PollOption)null);

            var service = GetVoteService(
                new Mock<IRepository<Guid, VoterCheck>>(),
                voteRepoMock,
                optionRepoMock,
                new Mock<IRepository<Guid, Voter>>(),
                GetHttpContextAccessor("voter@example.com"));

            await Assert.ThrowsAsync<Exception>(() => service.DeleteVoteAsync(voteId));
        }
    }
}