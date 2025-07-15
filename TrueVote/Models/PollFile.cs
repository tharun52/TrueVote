using System.ComponentModel.DataAnnotations;

namespace TrueVote.Models
{
    public class PollFile
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Filename { get; set; } = string.Empty;

        [Required]
        public string FileType { get; set; } = string.Empty;

        [Required]
        public byte[] Content { get; set; } = Array.Empty<byte>();

        [Required]
        public string UploadedByUsername { get; set; } = string.Empty;

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; } = false;

        public string? BlobUrl { get; set; }
    }
}