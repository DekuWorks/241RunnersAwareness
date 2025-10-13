using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _241RunnersAPI.Models
{
    public class DataDeletionRequest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(500)]
        public string DataTypes { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Reason { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Processing, Completed, Failed

        [Required]
        public DateTime RequestedAt { get; set; }

        public DateTime? ProcessedAt { get; set; }

        [StringLength(1000)]
        public string? ProcessingNotes { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}
