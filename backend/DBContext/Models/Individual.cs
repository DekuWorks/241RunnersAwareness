using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _241RunnersAwareness.BackendAPI.DBContext.Models
{
    public class Individual
    {
        [Key]
        public int Id { get; set; }

        // Alias for Id to maintain compatibility
        [NotMapped]
        public int IndividualId => Id;

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        public string LastName { get; set; }

        [StringLength(100)]
        public string? MiddleName { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [StringLength(10)]
        public string? Gender { get; set; }

        [StringLength(200)]
        public string? LastKnownAddress { get; set; }

        [StringLength(200)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(50)]
        public string? State { get; set; }

        [StringLength(10)]
        public string? ZipCode { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        // Physical Description
        [StringLength(50)]
        public string? Height { get; set; }

        [StringLength(50)]
        public string? Weight { get; set; }

        [StringLength(50)]
        public string? HairColor { get; set; }

        [StringLength(50)]
        public string? EyeColor { get; set; }

        [StringLength(200)]
        public string? DistinguishingFeatures { get; set; }

        // Special Needs Information
        [StringLength(500)]
        public string? SpecialNeeds { get; set; }

        [StringLength(200)]
        public string? MedicalConditions { get; set; }

        [StringLength(200)]
        public string? Medications { get; set; }

        [StringLength(200)]
        public string? Allergies { get; set; }

        // DNA and Biometric Data
        [StringLength(500)]
        public string? DNASample { get; set; } // Base64 encoded DNA data or reference

        [StringLength(100)]
        public string? DNASampleType { get; set; } // "Saliva", "Blood", "Hair", "Buccal Swab"

        public DateTime? DNASampleDate { get; set; }

        [StringLength(100)]
        public string? DNALabReference { get; set; }

        [StringLength(500)]
        public string? DNASequence { get; set; } // Encrypted DNA sequence data

        [StringLength(200)]
        public string? FingerprintData { get; set; } // Encrypted fingerprint data

        [StringLength(200)]
        public string? DentalRecords { get; set; } // Reference to dental records

        [StringLength(200)]
        public string? MedicalRecords { get; set; } // Reference to medical records

        // Identification Data
        [StringLength(100)]
        public string? SocialSecurityNumber { get; set; }

        [StringLength(100)]
        public string? DriverLicenseNumber { get; set; }

        [StringLength(100)]
        public string? PassportNumber { get; set; }

        // Case Information
        [StringLength(50)]
        public string? CaseStatus { get; set; } // "Active", "Resolved", "Closed"

        // Current Status for map functionality
        [StringLength(50)]
        public string? CurrentStatus { get; set; } // "Missing", "Found", "Safe"

        public DateTime? LastSeenDate { get; set; }

        [StringLength(200)]
        public string? LastSeenLocation { get; set; }

        [StringLength(500)]
        public string? Circumstances { get; set; }

        // Law Enforcement Integration
        [StringLength(100)]
        public string? NAMUSCaseNumber { get; set; }

        [StringLength(100)]
        public string? LocalCaseNumber { get; set; }

        [StringLength(100)]
        public string? InvestigatingAgency { get; set; }

        [StringLength(100)]
        public string? InvestigatorName { get; set; }

        [StringLength(20)]
        public string? InvestigatorPhone { get; set; }

        // Media and Public Awareness
        [StringLength(500)]
        public string? MediaReferences { get; set; }

        [StringLength(500)]
        public string? SocialMediaPosts { get; set; }

        // System Fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Alias for CreatedAt to maintain compatibility
        [NotMapped]
        public DateTime DateAdded => CreatedAt;

        [StringLength(100)]
        public string? CreatedBy { get; set; }

        [StringLength(100)]
        public string? UpdatedBy { get; set; }

        // Additional fields for compatibility
        [StringLength(500)]
        public string? Notes { get; set; }

        [StringLength(500)]
        public string? SpecialNeedsDescription { get; set; }

        public bool? HasBeenAdopted { get; set; }

        [StringLength(100)]
        public string? PlacementStatus { get; set; }

        public DateTime? AdoptionDate { get; set; }

        // Navigation Properties
        public virtual ICollection<EmergencyContact> EmergencyContacts { get; set; } = new List<EmergencyContact>();

        public virtual ICollection<CaseImage> Images { get; set; } = new List<CaseImage>();

        public virtual ICollection<CaseDocument> Documents { get; set; } = new List<CaseDocument>();

        // Computed Properties
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        [NotMapped]
        public int Age => DateTime.Now.Year - DateOfBirth.Year - (DateTime.Now.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);

        [NotMapped]
        public bool HasDNASample => !string.IsNullOrEmpty(DNASample);

        [NotMapped]
        public bool HasBiometricData => !string.IsNullOrEmpty(FingerprintData) || !string.IsNullOrEmpty(DentalRecords);
    }

    // Supporting Models for Enhanced Features
    public class CaseImage
    {
        [Key]
        public int Id { get; set; }

        public int IndividualId { get; set; }

        [StringLength(200)]
        public string ImageUrl { get; set; }

        [StringLength(100)]
        public string ImageType { get; set; } // "Photo", "Sketch", "CCTV", "Social Media"

        [StringLength(500)]
        public string? Description { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string? UploadedBy { get; set; }

        public virtual Individual Individual { get; set; }
    }

    public class CaseDocument
    {
        [Key]
        public int Id { get; set; }

        public int IndividualId { get; set; }

        [StringLength(200)]
        public string DocumentUrl { get; set; }

        [StringLength(100)]
        public string DocumentType { get; set; } // "Medical Record", "Police Report", "DNA Report", "Dental Record"

        [StringLength(500)]
        public string? Description { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string? UploadedBy { get; set; }

        public virtual Individual Individual { get; set; }
    }
}
