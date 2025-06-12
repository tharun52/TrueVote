using System.ComponentModel.DataAnnotations;

namespace TrueVote.Models
{
    public class AuditLog
    {
        [Key]
        public Guid AuditId { get; set; } = Guid.NewGuid();

        public string? Description { get; set; }  

        [Required]
        public Guid EntityId { get; set; }  
        public string? CreatedBy { get; set; }  

        [Required]
        public DateTime? CreatedAt { get; set; } 

        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}