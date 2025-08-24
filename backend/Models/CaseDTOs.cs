using System.ComponentModel.DataAnnotations;

namespace _241RunnersAwareness.BackendAPI.Models
{
    // Case DTOs
    public class CaseDto
    {
        public int Id { get; set; }
        public string CaseNumber { get; set; }
        public string PublicSlug { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public string RiskLevel { get; set; }
        public string? LastSeenLocation { get; set; }
        public DateTime? LastSeenDate { get; set; }
        public bool IsPublic { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public string? IndividualName { get; set; }
        public int UpdateCount { get; set; }
        public CaseUpdateDto? LatestUpdate { get; set; }
    }

    public class CaseDetailDto : CaseDto
    {
        public string? LawEnforcementCaseNumber { get; set; }
        public string? InvestigatingAgency { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public UserDto? Owner { get; set; }
        public CaseIndividualDto? Individual { get; set; }
        public List<CaseUpdateDto> Updates { get; set; } = new List<CaseUpdateDto>();
    }

    public class PublicCaseDto
    {
        public int Id { get; set; }
        public string CaseNumber { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string RiskLevel { get; set; }
        public string? LastSeenLocation { get; set; }
        public DateTime? LastSeenDate { get; set; }
        public string? InvestigatingAgency { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public CaseIndividualDto? Individual { get; set; }
        public List<PublicCaseUpdateDto> PublicUpdates { get; set; } = new List<PublicCaseUpdateDto>();
    }

    // Case Update DTOs
    public class CaseUpdateDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string UpdateType { get; set; }
        public bool IsPublic { get; set; }
        public bool IsUrgent { get; set; }
        public string? Location { get; set; }
        public DateTime UpdateDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public UserDto? CreatedBy { get; set; }
    }

    public class PublicCaseUpdateDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string UpdateType { get; set; }
        public bool IsUrgent { get; set; }
        public string? Location { get; set; }
        public DateTime UpdateDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Request DTOs
    public class CreateCaseRequest
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        [StringLength(2000)]
        public string Description { get; set; }

        public string? Status { get; set; } // Open, Closed, Under Investigation, Resolved
        public string? Priority { get; set; } // Low, Medium, High, Critical
        public string? RiskLevel { get; set; } // Low, Medium, High, Critical, Unknown
        public string? LastSeenLocation { get; set; }
        public DateTime? LastSeenDate { get; set; }
        public string? LawEnforcementCaseNumber { get; set; }
        public string? InvestigatingAgency { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public bool? IsPublic { get; set; }

        // Individual data
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

        // Special Needs & Disability Information
        [StringLength(100)]
        public string? PrimaryDisability { get; set; }

        [StringLength(500)]
        public string? DisabilityDescription { get; set; }

        [StringLength(100)]
        public string? CommunicationMethod { get; set; }

        [StringLength(200)]
        public string? CommunicationNeeds { get; set; }

        public bool? IsNonVerbal { get; set; }

        public bool? UsesAACDevice { get; set; }

        [StringLength(100)]
        public string? AACDeviceType { get; set; }

        [StringLength(100)]
        public string? MobilityStatus { get; set; }

        public bool? UsesWheelchair { get; set; }

        public bool? UsesMobilityDevice { get; set; }

        [StringLength(100)]
        public string? MobilityDeviceType { get; set; }

        public bool? HasVisualImpairment { get; set; }

        public bool? HasHearingImpairment { get; set; }

        public bool? HasSensoryProcessingDisorder { get; set; }

        [StringLength(200)]
        public string? SensoryTriggers { get; set; }

        [StringLength(200)]
        public string? SensoryComforts { get; set; }

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

        [StringLength(200)]
        public string? EmergencyResponseInstructions { get; set; }

        [StringLength(200)]
        public string? PreferredEmergencyContact { get; set; }

        public bool? ShouldCall911 { get; set; }

        [StringLength(200)]
        public string? SpecialInstructionsForFirstResponders { get; set; }

        public bool? EnableRealTimeAlerts { get; set; } = true;

        public bool? EnableSMSAlerts { get; set; } = true;

        public bool? EnableEmailAlerts { get; set; } = true;

        public bool? EnablePushNotifications { get; set; } = true;

        [StringLength(100)]
        public string? AlertRadius { get; set; }

        public int? AlertRadiusMiles { get; set; } = 5;

        public bool? HasGPSDevice { get; set; }

        [StringLength(100)]
        public string? GPSDeviceType { get; set; }
    }

    public class UpdateCaseRequest
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
        public string? Priority { get; set; }
        public string? RiskLevel { get; set; }
        public string? LastSeenLocation { get; set; }
        public DateTime? LastSeenDate { get; set; }
        public string? LawEnforcementCaseNumber { get; set; }
        public string? InvestigatingAgency { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public bool? IsPublic { get; set; }
    }

    public class CreateCaseUpdateRequest
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        [StringLength(5000)]
        public string Content { get; set; }

        public string? UpdateType { get; set; } // General, Status Change, Location Update, Investigation, Media, Other
        public bool? IsPublic { get; set; }
        public bool? IsUrgent { get; set; }
        public string? Location { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public DateTime? UpdateDate { get; set; }
        public bool? RequiresNotification { get; set; }
    }

    // Extended Individual DTO for case management
    public class CaseIndividualDto
    {
        public int IndividualId { get; set; }
        public string FullName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }
    }
}
