using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _241RunnersAwarenessAPI.Models
{
    public class Runner
    {
        [Key] 
        public int Id { get; set; }
        
        // Basic Identification
        [Required] 
        [MaxLength(100)] 
        public string FirstName { get; set; } = string.Empty;
        
        [MaxLength(100)] 
        public string? MiddleName { get; set; }
        
        [Required] 
        [MaxLength(100)] 
        public string LastName { get; set; } = string.Empty;
        
        [MaxLength(100)] 
        public string? MaidenName { get; set; }
        
        [MaxLength(100)] 
        public string? ChosenName { get; set; } // Nickname/Alias
        
        // NamUs Case Information
        [Required] 
        [MaxLength(50)] 
        public string RunnerId { get; set; } = string.Empty; // NamUs Case Number
        
        [MaxLength(50)] 
        public string? NcicNumber { get; set; }
        
        [MaxLength(50)] 
        public string? ViCapNumber { get; set; }
        
        [MaxLength(50)] 
        public string? NcmecNumber { get; set; }
        
        // Demographics
        [Required] 
        public int Age { get; set; }
        
        [Required] 
        [MaxLength(50)] 
        public string Gender { get; set; } = string.Empty;
        
        [MaxLength(100)] 
        public string? PlaceOfBirth { get; set; }
        
        [MaxLength(100)] 
        public string? Tribe { get; set; } // For Native American cases
        
        // Case Status and Resolution
        [Required] 
        [MaxLength(50)] 
        public string Status { get; set; } = "missing";
        
        [MaxLength(50)] 
        public string? ResolutionStatus { get; set; } // found, identified, etc.
        
        public DateTime? DateFound { get; set; }
        
        [MaxLength(100)] 
        public string? CityFound { get; set; }
        
        [MaxLength(50)] 
        public string? StateFound { get; set; }
        
        [MaxLength(100)] 
        public string? MannerOfDeath { get; set; }
        
        // Location Information
        [Required] 
        [MaxLength(100)] 
        public string City { get; set; } = string.Empty;
        
        [Required] 
        [MaxLength(50)] 
        public string State { get; set; } = string.Empty;
        
        [MaxLength(100)] 
        public string? County { get; set; }
        
        [Required] 
        [MaxLength(500)] 
        public string Address { get; set; } = string.Empty;
        
        // Physical Description
        [Required] 
        [MaxLength(50)] 
        public string Height { get; set; } = string.Empty; // Format: "5'10""
        
        [Required] 
        [MaxLength(50)] 
        public string Weight { get; set; } = string.Empty; // Format: "150 lbs"
        
        [Required] 
        [MaxLength(50)] 
        public string HairColor { get; set; } = string.Empty;
        
        [Required] 
        [MaxLength(50)] 
        public string EyeColor { get; set; } = string.Empty;
        
        [MaxLength(50)] 
        public string? LeftEyeColor { get; set; }
        
        [MaxLength(50)] 
        public string? RightEyeColor { get; set; }
        
        [Required] 
        [MaxLength(500)] 
        public string IdentifyingMarks { get; set; } = string.Empty;
        
        [MaxLength(500)] 
        public string? DistinctiveFeatures { get; set; }
        
        // Circumstances
        [Required] 
        public DateTime DateReported { get; set; }
        
        public DateTime? DateOfBirth { get; set; }
        
        public DateTime? DateOfLastContact { get; set; }
        
        public DateTime? LastSeen { get; set; }
        
        [Required] 
        [MaxLength(1000)] 
        public string Description { get; set; } = string.Empty;
        
        [MaxLength(1000)] 
        public string? Circumstances { get; set; }
        
        // Medical Information
        [Required] 
        [MaxLength(1000)] 
        public string MedicalConditions { get; set; } = string.Empty;
        
        [Required] 
        [MaxLength(500)] 
        public string Medications { get; set; } = string.Empty;
        
        [Required] 
        [MaxLength(500)] 
        public string Allergies { get; set; } = string.Empty;
        
        // DNA and Biometrics
        [MaxLength(100)] 
        public string? DnaStatus { get; set; } // "Sample Available", "Profile Uploaded to CODIS", etc.
        
        [MaxLength(100)] 
        public string? FingerprintStatus { get; set; }
        
        [MaxLength(100)] 
        public string? DentalStatus { get; set; }
        
        // Contact and Agency Information
        [Required] 
        [MaxLength(200)] 
        public string ContactInfo { get; set; } = string.Empty;
        
        [MaxLength(200)] 
        public string? InvestigatingAgency { get; set; }
        
        [MaxLength(100)] 
        public string? AgencyCaseNumber { get; set; }
        
        [MaxLength(100)] 
        public string? AgencyCity { get; set; }
        
        [MaxLength(50)] 
        public string? AgencyState { get; set; }
        
        // Images and Documents
        [MaxLength(500)] 
        public string? ProfileImageUrl { get; set; }
        
        [MaxLength(1000)] 
        public string? AdditionalImageUrls { get; set; } // JSON array of image URLs
        
        [MaxLength(1000)] 
        public string? DocumentUrls { get; set; } // JSON array of document URLs
        
        // Transportation and Personal Items
        [MaxLength(100)] 
        public string? VehicleMake { get; set; }
        
        [MaxLength(100)] 
        public string? VehicleModel { get; set; }
        
        [MaxLength(50)] 
        public string? VehicleColor { get; set; }
        
        [MaxLength(50)] 
        public string? VehicleYear { get; set; }
        
        [MaxLength(100)] 
        public string? VehicleVin { get; set; }
        
        [MaxLength(100)] 
        public string? ClothingDescription { get; set; }
        
        [MaxLength(500)] 
        public string? PersonalItems { get; set; }
        
        // Tags and Categories
        [Required] 
        [MaxLength(500)] 
        public string Tags { get; set; } = string.Empty;
        
        [Required] 
        public bool IsActive { get; set; } = true;
        
        [Required] 
        public bool IsUrgent { get; set; } = false;
        
        // Timestamps
        [Required] 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Relationships
        public int? ReportedByUserId { get; set; }
        
        [ForeignKey("ReportedByUserId")] 
        public User? ReportedByUser { get; set; }
        
        // Computed Properties
        [NotMapped] 
        public string FullName => $"{FirstName} {LastName}";
        
        [NotMapped] 
        public string CurrentStatus => Status;
        
        [NotMapped] 
        public DateTime DateAdded => DateReported;
        
        [NotMapped] 
        public int AgeInYears => Age;
        
        [NotMapped] 
        public string DisplayHeight => Height;
        
        [NotMapped] 
        public string DisplayWeight => Weight;
    }

    public class RunnerDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public string LastName { get; set; } = string.Empty;
        public string? MaidenName { get; set; }
        public string? ChosenName { get; set; }
        public string RunnerId { get; set; } = string.Empty;
        public string? NcicNumber { get; set; }
        public string? ViCapNumber { get; set; }
        public string? NcmecNumber { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string? PlaceOfBirth { get; set; }
        public string? Tribe { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ResolutionStatus { get; set; }
        public DateTime? DateFound { get; set; }
        public string? CityFound { get; set; }
        public string? StateFound { get; set; }
        public string? MannerOfDeath { get; set; }
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string? County { get; set; }
        public string Address { get; set; } = string.Empty;
        public string Height { get; set; } = string.Empty;
        public string Weight { get; set; } = string.Empty;
        public string HairColor { get; set; } = string.Empty;
        public string EyeColor { get; set; } = string.Empty;
        public string? LeftEyeColor { get; set; }
        public string? RightEyeColor { get; set; }
        public string IdentifyingMarks { get; set; } = string.Empty;
        public string? DistinctiveFeatures { get; set; }
        public DateTime DateReported { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime? DateOfLastContact { get; set; }
        public DateTime? LastSeen { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? Circumstances { get; set; }
        public string MedicalConditions { get; set; } = string.Empty;
        public string Medications { get; set; } = string.Empty;
        public string Allergies { get; set; } = string.Empty;
        public string? DnaStatus { get; set; }
        public string? FingerprintStatus { get; set; }
        public string? DentalStatus { get; set; }
        public string ContactInfo { get; set; } = string.Empty;
        public string? InvestigatingAgency { get; set; }
        public string? AgencyCaseNumber { get; set; }
        public string? AgencyCity { get; set; }
        public string? AgencyState { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? AdditionalImageUrls { get; set; }
        public string? DocumentUrls { get; set; }
        public string? VehicleMake { get; set; }
        public string? VehicleModel { get; set; }
        public string? VehicleColor { get; set; }
        public string? VehicleYear { get; set; }
        public string? VehicleVin { get; set; }
        public string? ClothingDescription { get; set; }
        public string? PersonalItems { get; set; }
        public string Tags { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsUrgent { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? ReportedByUserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string CurrentStatus { get; set; } = string.Empty;
        public DateTime DateAdded { get; set; }
        public int AgeInYears { get; set; }
        public string DisplayHeight { get; set; } = string.Empty;
        public string DisplayWeight { get; set; } = string.Empty;
    }
} 