using Microsoft.EntityFrameworkCore;
using TrueVote.Models;

namespace TrueVote.Contexts
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        { 
        }

        public DbSet<Admin> Admins { get; set; }
        public DbSet<Moderator> Moderators { get; set; }
        public DbSet<PoleFile> PoleFiles { get; set; }
        public DbSet<Poll> Polls { get; set; }
        public DbSet<PollOption> PollOptions { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<PollVote> PollVotes { get; set; }
        public DbSet<Voter> Voters { get; set; }
        public DbSet<VoterPoll> VoterPolls { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // User
            modelBuilder.Entity<User>()
                .HasKey(u => u.Username);

            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .IsRequired();

            // Admin
            modelBuilder.Entity<Admin>()
                .HasIndex(a => a.Email)
                .IsUnique();

            // Moderator
            modelBuilder.Entity<Moderator>()
                .HasIndex(m => m.Email)
                .IsUnique();

            modelBuilder.Entity<Moderator>()
                .HasMany(m => m.Polls)
                .WithOne()
                .HasForeignKey(p => p.CreatedByEmail)
                .HasPrincipalKey(m => m.Email)
                .OnDelete(DeleteBehavior.Restrict);

            // Poll
            modelBuilder.Entity<Poll>()
                .HasMany(p => p.PollOptions)
                .WithOne()
                .HasForeignKey(po => po.PollId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Poll>()
                .HasOne(p => p.PoleFile)
                .WithOne()
                .HasForeignKey<Poll>(p => p.Id)
                .OnDelete(DeleteBehavior.Restrict);

            // PollOption
            modelBuilder.Entity<PollOption>()
                .HasIndex(po => new { po.PollId, po.OptionText })
                .IsUnique(); 

            // PollVote
            modelBuilder.Entity<PollVote>()
                .HasOne<PollOption>()
                .WithMany()
                .HasForeignKey(pv => pv.PollOptionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Voter
            modelBuilder.Entity<Voter>()
                .HasIndex(v => v.Email)
                .IsUnique();

            // VoterPoll
            modelBuilder.Entity<VoterPoll>()
                .HasIndex(vp => new { vp.VoterId, vp.PollId })
                .IsUnique(); // Prevents duplicate votes

            modelBuilder.Entity<VoterPoll>()
                .HasOne<Voter>()
                .WithMany()
                .HasForeignKey(vp => vp.VoterId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VoterPoll>()
                .HasOne<Poll>()
                .WithMany()
                .HasForeignKey(vp => vp.PollId)
                .OnDelete(DeleteBehavior.Cascade);

            // PoleFile
            modelBuilder.Entity<PoleFile>()
                .Property(pf => pf.Content)
                .IsRequired();

            modelBuilder.Entity<PoleFile>()
                .HasIndex(pf => new { pf.Filename, pf.UploadedByUsername })
                .IsUnique();
        }
    }
}
