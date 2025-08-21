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

        // ===== ENHANCED SPECIAL NEEDS & DISABILITY INFORMATION =====
        
        // Primary Disability/Special Need Classification
        [StringLength(100)]
        public string? PrimaryDisability { get; set; } // "Autism", "Down Syndrome", "Cerebral Palsy", "Intellectual Disability", "Sensory Impairment", etc.

        [StringLength(500)]
        public string? DisabilityDescription { get; set; }

        // Communication Abilities
        [StringLength(100)]
        public string? CommunicationMethod { get; set; } // "Verbal", "Non-verbal", "Sign Language", "AAC Device", "Picture Cards", etc.

        [StringLength(200)]
        public string? CommunicationNeeds { get; set; }

        public bool? IsNonVerbal { get; set; }

        public bool? UsesAACDevice { get; set; }

        [StringLength(100)]
        public string? AACDeviceType { get; set; }

        // Mobility and Physical Needs
        [StringLength(100)]
        public string? MobilityStatus { get; set; } // "Independent", "Wheelchair", "Walker", "Cane", "Assistance Required"

        public bool? UsesWheelchair { get; set; }

        public bool? UsesMobilityDevice { get; set; }

        [StringLength(100)]
        public string? MobilityDeviceType { get; set; }

        // Sensory Needs
        public bool? HasVisualImpairment { get; set; }

        public bool? HasHearingImpairment { get; set; }

        public bool? HasSensoryProcessingDisorder { get; set; }

        [StringLength(200)]
        public string? SensoryTriggers { get; set; } // Loud noises, bright lights, crowds, etc.

        [StringLength(200)]
        public string? SensoryComforts { get; set; } // Preferred items, calming techniques

        // Behavioral and Safety Information
        [StringLength(200)]
        public string? BehavioralTriggers { get; set; }

        [StringLength(200)]
        public string? CalmingTechniques { get; set; }

        public bool? MayWanderOrElope { get; set; }

        public bool? IsAttractedToWater { get; set; }

        public bool? IsAttractedToRoads { get; set; }

        public bool? IsAttractedToBrightLights { get; set; }

        [StringLength(200)]
        public string? WanderingPatterns { get; set; }

        [StringLength(200)]
        public string? PreferredLocations { get; set; }

        // Medical and Safety Information
        [StringLength(500)]
        public string? MedicalConditions { get; set; }

        [StringLength(200)]
        public string? Medications { get; set; }

        [StringLength(200)]
        public string? Allergies { get; set; }

        public bool? RequiresMedication { get; set; }

        [StringLength(200)]
        public string? MedicationSchedule { get; set; }

        public bool? HasSeizureDisorder { get; set; }

        [StringLength(200)]
        public string? SeizureTriggers { get; set; }

        public bool? HasDiabetes { get; set; }

        public bool? HasAsthma { get; set; }

        public bool? HasHeartCondition { get; set; }

        // Emergency Response Information
        [StringLength(200)]
        public string? EmergencyResponseInstructions { get; set; }

        [StringLength(200)]
        public string? PreferredEmergencyContact { get; set; }

        public bool? ShouldCall911 { get; set; }

        [StringLength(200)]
        public string? SpecialInstructionsForFirstResponders { get; set; }

        // Real-Time Alert Configuration
        public bool? EnableRealTimeAlerts { get; set; } = true;

        public bool? EnableSMSAlerts { get; set; } = true;

        public bool? EnableEmailAlerts { get; set; } = true;

        public bool? EnablePushNotifications { get; set; } = true;

        [StringLength(100)]
        public string? AlertRadius { get; set; } // "1 mile", "5 miles", "10 miles", "County", "State"

        public int? AlertRadiusMiles { get; set; } = 5;

        // GPS and Tracking Information
        public bool? HasGPSDevice { get; set; }

        [StringLength(100)]
        public string? GPSDeviceType { get; set; }

        [StringLength(100)]
        public string? GPSDeviceID { get; set; }

        public bool? HasMedicalID { get; set; }

        [StringLength(100)]
        public string? MedicalIDNumber { get; set; }

        // Support Network Information
        [StringLength(200)]
        public string? CaregiverName { get; set; }

        [StringLength(20)]
        public string? CaregiverPhone { get; set; }

        [StringLength(100)]
        public string? CaregiverEmail { get; set; }

        [StringLength(200)]
        public string? SupportOrganization { get; set; }

        [StringLength(20)]
        public string? SupportOrganizationPhone { get; set; }

        // Legacy fields for backward compatibility
        [StringLength(500)]
        public string? SpecialNeeds { get; set; }

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
        public string? CurrentStatus { get; set; } // "Missing", "Found", "Safe", "At Risk"

        public DateTime? LastSeenDate { get; set; }

        [StringLength(200)]
        public string? LastSeenLocation { get; set; }

        [StringLength(500)]
        public string? Circumstances { get; set; }

        // Risk Assessment
        [StringLength(50)]
        public string? RiskLevel { get; set; } // "Low", "Medium", "High", "Critical"

        public bool? IsAtImmediateRisk { get; set; }

        [StringLength(500)]
        public string? RiskFactors { get; set; }

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

        public virtual ICollection<AlertLog> AlertLogs { get; set; } = new List<AlertLog>();

        // Computed Properties
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        [NotMapped]
        public int Age => DateTime.Now.Year - DateOfBirth.Year - (DateTime.Now.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);

        [NotMapped]
        public bool HasDNASample => !string.IsNullOrEmpty(DNASample);

        [NotMapped]
        public bool HasBiometricData => !string.IsNullOrEmpty(FingerprintData) || !string.IsNullOrEmpty(DentalRecords);

        [NotMapped]
        public bool IsHighRisk => IsAtImmediateRisk == true || RiskLevel == "High" || RiskLevel == "Critical" || MayWanderOrElope == true;

        [NotMapped]
        public bool RequiresImmediateAttention => IsAtImmediateRisk == true || RiskLevel == "Critical";
    }

    // New model for tracking real-time alerts
    public class AlertLog
    {
        [Key]
        public int Id { get; set; }

        public int IndividualId { get; set; }

        [StringLength(50)]
        public string AlertType { get; set; } // "Missing", "Found", "Sighting", "Risk Alert", "Medical Emergency"

        [StringLength(200)]
        public string AlertTitle { get; set; }

        [StringLength(1000)]
        public string AlertMessage { get; set; }

        [StringLength(200)]
        public string? Location { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        [StringLength(50)]
        public string AlertStatus { get; set; } // "Active", "Resolved", "Expired"

        public DateTime AlertTime { get; set; } = DateTime.UtcNow;

        public DateTime? ResolvedTime { get; set; }

        [StringLength(100)]
        public string? ResolvedBy { get; set; }

        [StringLength(500)]
        public string? ResolutionNotes { get; set; }

        public bool IsUrgent { get; set; } = false;

        public int? AlertRadiusMiles { get; set; }

        public virtual Individual Individual { get; set; }
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
