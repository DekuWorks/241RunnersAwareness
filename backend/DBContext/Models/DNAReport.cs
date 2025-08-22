using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _241RunnersAwareness.BackendAPI.DBContext.Models
{
    public class DNAReport
    {
        [Key]
        public Guid ReportId { get; set; }
        
        [Required]
        public Guid ReporterUserId { get; set; }
        
        [Required]
        public int IndividualId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string ReportTitle { get; set; }
        
        [Required]
        [StringLength(1000)]
        public string Description { get; set; }
        
        [Required]
        public DateTime ReportDate { get; set; } = DateTime.UtcNow;
        
        [Required]
        [StringLength(100)]
        public string Location { get; set; }
        
        [StringLength(50)]
        public string? Status { get; set; } = "Active"; // Active, Resolved, Closed
        
        // DNA Sample Information
        [StringLength(500)]
        public string? DNASampleDescription { get; set; }
        
        [StringLength(100)]
        public string? DNASampleType { get; set; } // Hair, Blood, Saliva, etc.
        
        [StringLength(100)]
        public string? DNASampleLocation { get; set; }
        
        public DateTime? DNASampleCollectionDate { get; set; }
        
        [StringLength(100)]
        public string? DNALabReference { get; set; }
        
        [StringLength(50)]
        public string? DNASequence { get; set; }
        
        public bool DNASampleCollected { get; set; } = false;
        
        public bool DNASampleProcessed { get; set; } = false;
        
        public bool DNASampleMatched { get; set; } = false;
        
        // Additional Report Details
        [StringLength(100)]
        public string? WeatherConditions { get; set; }
        
        [StringLength(100)]
        public string? ClothingDescription { get; set; }
        
        [StringLength(100)]
        public string? PhysicalDescription { get; set; }
        
        [StringLength(100)]
        public string? BehaviorDescription { get; set; }
        
        // Contact Information
        [StringLength(100)]
        public string? WitnessName { get; set; }
        
        [StringLength(20)]
        public string? WitnessPhone { get; set; }
        
        [StringLength(100)]
        public string? WitnessEmail { get; set; }
        
        // Resolution Information
        public DateTime? ResolutionDate { get; set; }
        
        [StringLength(500)]
        public string? ResolutionNotes { get; set; }
        
        [StringLength(100)]
        public string? ResolvedBy { get; set; }
        
        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        [ForeignKey("ReporterUserId")]
        public User Reporter { get; set; }
        
        [ForeignKey("IndividualId")]
        public Individual Individual { get; set; }
    }
}
