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
        public DbSet<PollFile> PollFiles { get; set; }
        public DbSet<Poll> Polls { get; set; }
        public DbSet<PollOption> PollOptions { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<PollVote> PollVotes { get; set; }
        public DbSet<Voter> Voters { get; set; }
        public DbSet<VoterCheck> VoterChecks { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<VoterEmail> VoterEmails { get; set; }
        
        public DbSet<Message> Messages { get; set; }
        public DbSet<UserMessage> UserMessages { get; set; }
        public DbSet<MagicLoginToken> MagicLoginTokens { get; set; }
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
                .HasOne<PollFile>()
                .WithMany()
                .HasForeignKey(p => p.PoleFileId)
                .OnDelete(DeleteBehavior.SetNull);

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

            modelBuilder.Entity<Voter>(entity =>
            {
                entity.HasOne<Moderator>()
                        .WithMany()
                        .HasForeignKey(v => v.ModeratorId)
                        .OnDelete(DeleteBehavior.Restrict);
            });


            // VoterCheck
            modelBuilder.Entity<VoterCheck>()
                .HasIndex(vp => new { vp.VoterId, vp.PollId })
                .IsUnique();

            modelBuilder.Entity<VoterCheck>()
                .HasOne<Voter>()
                .WithMany()
                .HasForeignKey(vp => vp.VoterId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VoterCheck>()
                .HasOne<Poll>()
                .WithMany()
                .HasForeignKey(vp => vp.PollId)
                .OnDelete(DeleteBehavior.Cascade);

            // PoleFile
            modelBuilder.Entity<PollFile>()
                .Property(pf => pf.Content)
                .IsRequired();

            // RefreshToken
            modelBuilder.Entity<RefreshToken>()
                .HasIndex(rt => rt.Token)
                .IsUnique();

            // VoterEmails
            modelBuilder.Entity<VoterEmail>(entity =>
            {
                entity.HasOne<Moderator>()
                    .WithMany()
                    .HasForeignKey(e => e.ModeratorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // UserMessage
            modelBuilder.Entity<UserMessage>()
                .HasIndex(um => new { um.MessageId, um.UserId })
                .IsUnique();

            modelBuilder.Entity<UserMessage>(entity =>
            {
                entity.HasOne<Message>()
                        .WithMany()
                        .HasForeignKey(um => um.MessageId)
                        .OnDelete(DeleteBehavior.Restrict);
            });


        }
    }
}
