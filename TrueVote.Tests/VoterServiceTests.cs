using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using TrueVote.Interfaces;
using TrueVote.Models;
using TrueVote.Models.DTOs;
using TrueVote.Service;
using Xunit;

namespace TrueVote.Tests
{
    public class VoterServiceTests
    {
        private readonly Mock<IRepository<Guid, Voter>> _mockVoterRepo;
        private readonly Mock<IRepository<string, User>> _mockUserRepo;
        private readonly Mock<IEncryptionService> _mockEncryptionService;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly Mock<IAuditLogger> _mockAuditLogger;
        private readonly VoterService _voterService;

        public VoterServiceTests()
        {
            _mockVoterRepo = new Mock<IRepository<Guid, Voter>>();
            _mockUserRepo = new Mock<IRepository<string, User>>();
            _mockEncryptionService = new Mock<IEncryptionService>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockAuditLogger = new Mock<IAuditLogger>();

            _voterService = new VoterService(
                _mockVoterRepo.Object,
                _mockUserRepo.Object,
                _mockEncryptionService.Object,
                _mockHttpContextAccessor.Object,
                _mockAuditLogger.Object
            );
        }

        [Fact]
        public async Task GetAllVotersAsync_Returns_NonDeletedVoters()
        {
            var voters = new List<Voter>
            {
                new Voter { Id = Guid.NewGuid(), IsDeleted = false },
                new Voter { Id = Guid.NewGuid(), IsDeleted = true }
            };

            _mockVoterRepo.Setup(r => r.GetAll()).ReturnsAsync(voters);

            var result = await _voterService.GetAllVotersAsync();

            Assert.Single(result);
            Assert.False(result.First().IsDeleted);
        }

        [Fact]
        public async Task AddVoterAsync_Successfully_Adds_Voter_And_User()
        {
            var dto = new AddVoterRequestDto
            {
                Name = "John Doe",
                Age = 20,
                Email = "john@example.com",
                Password = "password123"
            };

            _mockUserRepo.Setup(repo => repo.Get(dto.Email)).ReturnsAsync((User)null);

            _mockEncryptionService.Setup(enc => enc.EncryptData(It.IsAny<EncryptModel>()))
                .ReturnsAsync(new EncryptModel
                {
                    EncryptedText = "encryptedPassword",
                    HashKey = "hashKey"
                });

            var voterToAdd = new Voter { Id = Guid.NewGuid(), Email = dto.Email, Age = dto.Age };
            _mockVoterRepo.Setup(r => r.Add(It.IsAny<Voter>())).ReturnsAsync(voterToAdd);

            var result = await _voterService.AddVoterAsync(dto);

            Assert.Equal(dto.Email, result.Email);
            _mockUserRepo.Verify(r => r.Add(It.Is<User>(u => u.Username == dto.Email)), Times.Once);
        }

        [Fact]
        public async Task DeleteVoterAsync_SoftDeletes_Voter()
        {
            var voterId = Guid.NewGuid();
            var voter = new Voter { Id = voterId, IsDeleted = false };

            _mockVoterRepo.Setup(r => r.Get(voterId)).ReturnsAsync(voter);

            var claims = new ClaimsIdentity(new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "admin@example.com")
            }, "mock");

            var httpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(claims)
            };

            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

            _mockVoterRepo.Setup(r => r.Update(voterId, voter)).ReturnsAsync(voter);

            var result = await _voterService.DeleteVoterAsync(voterId);

            Assert.True(result.IsDeleted);
        }
    }
}
