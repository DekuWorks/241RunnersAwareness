using System.ComponentModel.DataAnnotations;

namespace _241RunnersAwarenessAPI.Models
{
    /// <summary>
    /// DTO for creating a new runner (public endpoint)
    /// </summary>
    public class CreateRunnerDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string LastName { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? MiddleName { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }
        
        [Range(0, 120)]
        public int? AgeInYears { get; set; }
        
        [StringLength(50)]
        public string? Gender { get; set; }
        
        [StringLength(50)]
        public string? Height { get; set; }
        
        [StringLength(50)]
        public string? Weight { get; set; }
        
        [StringLength(50)]
        public string? HairColor { get; set; }
        
        [StringLength(50)]
        public string? EyeColor { get; set; }
        
        [StringLength(500)]
        public string? IdentifyingMarks { get; set; }
        
        [StringLength(500)]
        public string? DistinctiveFeatures { get; set; }
        
        [StringLength(100)]
        public string? City { get; set; }
        
        [StringLength(50)]
        public string? State { get; set; }
        
        [StringLength(100)]
        public string? County { get; set; }
        
        [StringLength(500)]
        public string? Address { get; set; }
        
        [DataType(DataType.DateTime)]
        public DateTime? DateReported { get; set; }
        
        [DataType(DataType.DateTime)]
        public DateTime? DateOfLastContact { get; set; }
        
        [DataType(DataType.DateTime)]
        public DateTime? LastSeen { get; set; }
        
        [StringLength(1000)]
        public string? Description { get; set; }
        
        [StringLength(1000)]
        public string? Circumstances { get; set; }
        
        [StringLength(500)]
        public string? MedicalConditions { get; set; }
        
        [StringLength(500)]
        public string? Medications { get; set; }
        
        [StringLength(500)]
        public string? Allergies { get; set; }
        
        [Required]
        [StringLength(250)]
        [EmailAddress]
        public string ContactInfo { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string? InvestigatingAgency { get; set; }
        
        [StringLength(100)]
        public string? AgencyCaseNumber { get; set; }
        
        [StringLength(100)]
        public string? AgencyCity { get; set; }
        
        [StringLength(50)]
        public string? AgencyState { get; set; }
        
        [StringLength(500)]
        public string? ClothingDescription { get; set; }
        
        [StringLength(500)]
        public string? PersonalItems { get; set; }
        
        [StringLength(100)]
        public string? VehicleMake { get; set; }
        
        [StringLength(100)]
        public string? VehicleModel { get; set; }
        
        [StringLength(50)]
        public string? VehicleColor { get; set; }
        
        [StringLength(10)]
        public string? VehicleYear { get; set; }
        
        [StringLength(50)]
        public string? VehicleVin { get; set; }
        
        [StringLength(50)]
        public string? Status { get; set; }
        
        [StringLength(50)]
        public string? CurrentStatus { get; set; }
        
        public bool? IsUrgent { get; set; }
        
        [StringLength(500)]
        public string? Tags { get; set; }
    }
}
