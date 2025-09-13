using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _241RunnersAPI.Models
{
    /// <summary>
    /// Case model representing missing persons cases
    /// </summary>
    public class Case
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int RunnerId { get; set; }

        [ForeignKey("RunnerId")]
        public Runner Runner { get; set; } = null!;

        [Required]
        public int ReportedByUserId { get; set; }

        [ForeignKey("ReportedByUserId")]
        public User ReportedByUser { get; set; } = null!;

        public int? CreatedByUserId { get; set; }

        [Required(ErrorMessage = "Case title is required")]
        [MaxLength(200, ErrorMessage = "Case title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Case description is required")]
        [MaxLength(2000, ErrorMessage = "Case description cannot exceed 2000 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last seen date is required")]
        public DateTime LastSeenDate { get; set; }

        [Required(ErrorMessage = "Last seen location is required")]
        [MaxLength(500, ErrorMessage = "Last seen location cannot exceed 500 characters")]
        public string LastSeenLocation { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Location cannot exceed 500 characters")]
        public string? Location { get; set; }

        public DateTime? LastSeenAt { get; set; }

        [MaxLength(100, ErrorMessage = "Last seen time cannot exceed 100 characters")]
        public string? LastSeenTime { get; set; }

        [MaxLength(1000, ErrorMessage = "Last seen circumstances cannot exceed 1000 characters")]
        public string? LastSeenCircumstances { get; set; }

        [MaxLength(200, ErrorMessage = "Clothing description cannot exceed 200 characters")]
        public string? ClothingDescription { get; set; }

        [MaxLength(200, ErrorMessage = "Physical condition cannot exceed 200 characters")]
        public string? PhysicalCondition { get; set; }

        [MaxLength(200, ErrorMessage = "Mental state cannot exceed 200 characters")]
        public string? MentalState { get; set; }

        [MaxLength(1000, ErrorMessage = "Additional information cannot exceed 1000 characters")]
        public string? AdditionalInformation { get; set; }

        [Required(ErrorMessage = "Case status is required")]
        [MaxLength(50, ErrorMessage = "Case status cannot exceed 50 characters")]
        [RegularExpression("^(Active|Safe|Missing|Found|Investigating|Closed|Cancelled)$", ErrorMessage = "Case status must be one of: Active, Safe, Missing, Found, Investigating, Closed, Cancelled")]
        public string Status { get; set; } = "Active";

        [Required(ErrorMessage = "Case priority is required")]
        [MaxLength(20, ErrorMessage = "Case priority cannot exceed 20 characters")]
        [RegularExpression("^(Low|Medium|High|Critical)$", ErrorMessage = "Case priority must be one of: Low, Medium, High, Critical")]
        public string Priority { get; set; } = "Medium";

        [Required]
        public bool IsPublic { get; set; } = false;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public DateTime? ResolvedAt { get; set; }

        [MaxLength(1000, ErrorMessage = "Resolution notes cannot exceed 1000 characters")]
        public string? ResolutionNotes { get; set; }

        [MaxLength(200, ErrorMessage = "Resolved by cannot exceed 200 characters")]
        public string? ResolvedBy { get; set; }

        // Contact information for case
        [MaxLength(100, ErrorMessage = "Contact person name cannot exceed 100 characters")]
        public string? ContactPersonName { get; set; }

        [Phone(ErrorMessage = "Invalid contact phone number format")]
        [MaxLength(20, ErrorMessage = "Contact phone number cannot exceed 20 characters")]
        public string? ContactPersonPhone { get; set; }

        [EmailAddress(ErrorMessage = "Invalid contact email format")]
        [MaxLength(255, ErrorMessage = "Contact email cannot exceed 255 characters")]
        public string? ContactPersonEmail { get; set; }

        // Location coordinates (if available)
        public decimal? LastSeenLatitude { get; set; }
        public decimal? LastSeenLongitude { get; set; }

        // Case images and documents
        [MaxLength(1000, ErrorMessage = "Case image URLs cannot exceed 1000 characters")]
        public string? CaseImageUrls { get; set; } // JSON array of image URLs

        [MaxLength(1000, ErrorMessage = "Document URLs cannot exceed 1000 characters")]
        public string? DocumentUrls { get; set; } // JSON array of document URLs

        // Emergency contact information
        [MaxLength(100, ErrorMessage = "Emergency contact name cannot exceed 100 characters")]
        public string? EmergencyContactName { get; set; }

        [Phone(ErrorMessage = "Invalid emergency contact phone number format")]
        [MaxLength(20, ErrorMessage = "Emergency contact phone number cannot exceed 20 characters")]
        public string? EmergencyContactPhone { get; set; }

        [MaxLength(100, ErrorMessage = "Emergency contact relationship cannot exceed 100 characters")]
        public string? EmergencyContactRelationship { get; set; }

        // Verification and approval
        public bool IsVerified { get; set; } = false;
        public DateTime? VerifiedAt { get; set; }
        public string? VerifiedBy { get; set; }

        public bool IsApproved { get; set; } = false;
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedBy { get; set; }

        // Case tracking
        public int ViewCount { get; set; } = 0;
        public int ShareCount { get; set; } = 0;
        public int TipCount { get; set; } = 0;
    }

    /// <summary>
    /// DTO for case creation
    /// </summary>
    public class CaseCreationDto
    {
        [Required(ErrorMessage = "Runner ID is required")]
        public int RunnerId { get; set; }

        [Required(ErrorMessage = "Case title is required")]
        [MaxLength(200, ErrorMessage = "Case title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Case description is required")]
        [MaxLength(2000, ErrorMessage = "Case description cannot exceed 2000 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last seen date is required")]
        public DateTime LastSeenDate { get; set; }

        [Required(ErrorMessage = "Last seen location is required")]
        [MaxLength(500, ErrorMessage = "Last seen location cannot exceed 500 characters")]
        public string LastSeenLocation { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Location cannot exceed 500 characters")]
        public string? Location { get; set; }

        public DateTime? LastSeenAt { get; set; }

        [MaxLength(100, ErrorMessage = "Last seen time cannot exceed 100 characters")]
        public string? LastSeenTime { get; set; }

        [MaxLength(1000, ErrorMessage = "Last seen circumstances cannot exceed 1000 characters")]
        public string? LastSeenCircumstances { get; set; }

        [MaxLength(200, ErrorMessage = "Clothing description cannot exceed 200 characters")]
        public string? ClothingDescription { get; set; }

        [MaxLength(200, ErrorMessage = "Physical condition cannot exceed 200 characters")]
        public string? PhysicalCondition { get; set; }

        [MaxLength(200, ErrorMessage = "Mental state cannot exceed 200 characters")]
        public string? MentalState { get; set; }

        [MaxLength(1000, ErrorMessage = "Additional information cannot exceed 1000 characters")]
        public string? AdditionalInformation { get; set; }

        [Required(ErrorMessage = "Case priority is required")]
        [MaxLength(20, ErrorMessage = "Case priority cannot exceed 20 characters")]
        [RegularExpression("^(Low|Medium|High|Critical)$", ErrorMessage = "Case priority must be one of: Low, Medium, High, Critical")]
        public string Priority { get; set; } = "Medium";

        [Required]
        public bool IsPublic { get; set; } = false;

        [MaxLength(100, ErrorMessage = "Contact person name cannot exceed 100 characters")]
        public string? ContactPersonName { get; set; }

        [Phone(ErrorMessage = "Invalid contact phone number format")]
        [MaxLength(20, ErrorMessage = "Contact phone number cannot exceed 20 characters")]
        public string? ContactPersonPhone { get; set; }

        [EmailAddress(ErrorMessage = "Invalid contact email format")]
        [MaxLength(255, ErrorMessage = "Contact email cannot exceed 255 characters")]
        public string? ContactPersonEmail { get; set; }

        public decimal? LastSeenLatitude { get; set; }
        public decimal? LastSeenLongitude { get; set; }

        [MaxLength(100, ErrorMessage = "Emergency contact name cannot exceed 100 characters")]
        public string? EmergencyContactName { get; set; }

        [Phone(ErrorMessage = "Invalid emergency contact phone number format")]
        [MaxLength(20, ErrorMessage = "Emergency contact phone number cannot exceed 20 characters")]
        public string? EmergencyContactPhone { get; set; }

        [MaxLength(100, ErrorMessage = "Emergency contact relationship cannot exceed 100 characters")]
        public string? EmergencyContactRelationship { get; set; }
    }

    /// <summary>
    /// DTO for case update
    /// </summary>
    public class CaseUpdateDto
    {
        [Required(ErrorMessage = "Case title is required")]
        [MaxLength(200, ErrorMessage = "Case title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Case description is required")]
        [MaxLength(2000, ErrorMessage = "Case description cannot exceed 2000 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last seen date is required")]
        public DateTime LastSeenDate { get; set; }

        [Required(ErrorMessage = "Last seen location is required")]
        [MaxLength(500, ErrorMessage = "Last seen location cannot exceed 500 characters")]
        public string LastSeenLocation { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Location cannot exceed 500 characters")]
        public string? Location { get; set; }

        public DateTime? LastSeenAt { get; set; }

        [MaxLength(100, ErrorMessage = "Last seen time cannot exceed 100 characters")]
        public string? LastSeenTime { get; set; }

        [MaxLength(1000, ErrorMessage = "Last seen circumstances cannot exceed 1000 characters")]
        public string? LastSeenCircumstances { get; set; }

        [MaxLength(200, ErrorMessage = "Clothing description cannot exceed 200 characters")]
        public string? ClothingDescription { get; set; }

        [MaxLength(200, ErrorMessage = "Physical condition cannot exceed 200 characters")]
        public string? PhysicalCondition { get; set; }

        [MaxLength(200, ErrorMessage = "Mental state cannot exceed 200 characters")]
        public string? MentalState { get; set; }

        [MaxLength(1000, ErrorMessage = "Additional information cannot exceed 1000 characters")]
        public string? AdditionalInformation { get; set; }

        [Required(ErrorMessage = "Case status is required")]
        [MaxLength(50, ErrorMessage = "Case status cannot exceed 50 characters")]
        [RegularExpression("^(Active|Safe|Missing|Found|Investigating|Closed|Cancelled)$", ErrorMessage = "Case status must be one of: Active, Safe, Missing, Found, Investigating, Closed, Cancelled")]
        public string Status { get; set; } = "Active";

        [Required(ErrorMessage = "Case priority is required")]
        [MaxLength(20, ErrorMessage = "Case priority cannot exceed 20 characters")]
        [RegularExpression("^(Low|Medium|High|Critical)$", ErrorMessage = "Case priority must be one of: Low, Medium, High, Critical")]
        public string Priority { get; set; } = "Medium";

        [Required]
        public bool IsPublic { get; set; } = false;

        [MaxLength(1000, ErrorMessage = "Resolution notes cannot exceed 1000 characters")]
        public string? ResolutionNotes { get; set; }

        [MaxLength(100, ErrorMessage = "Contact person name cannot exceed 100 characters")]
        public string? ContactPersonName { get; set; }

        [Phone(ErrorMessage = "Invalid contact phone number format")]
        [MaxLength(20, ErrorMessage = "Contact phone number cannot exceed 20 characters")]
        public string? ContactPersonPhone { get; set; }

        [EmailAddress(ErrorMessage = "Invalid contact email format")]
        [MaxLength(255, ErrorMessage = "Contact email cannot exceed 255 characters")]
        public string? ContactPersonEmail { get; set; }

        public decimal? LastSeenLatitude { get; set; }
        public decimal? LastSeenLongitude { get; set; }

        [MaxLength(100, ErrorMessage = "Emergency contact name cannot exceed 100 characters")]
        public string? EmergencyContactName { get; set; }

        [Phone(ErrorMessage = "Invalid emergency contact phone number format")]
        [MaxLength(20, ErrorMessage = "Emergency contact phone number cannot exceed 20 characters")]
        public string? EmergencyContactPhone { get; set; }

        [MaxLength(100, ErrorMessage = "Emergency contact relationship cannot exceed 100 characters")]
        public string? EmergencyContactRelationship { get; set; }
    }

    /// <summary>
    /// DTO for case response (without sensitive data)
    /// </summary>
    public class CaseResponseDto
    {
        public int Id { get; set; }
        public int RunnerId { get; set; }
        public int ReportedByUserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime LastSeenDate { get; set; }
        public string LastSeenLocation { get; set; } = string.Empty;
        public string? LastSeenTime { get; set; }
        public string? LastSeenCircumstances { get; set; }
        public string? ClothingDescription { get; set; }
        public string? PhysicalCondition { get; set; }
        public string? MentalState { get; set; }
        public string? AdditionalInformation { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public bool IsPublic { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? ResolutionNotes { get; set; }
        public string? ResolvedBy { get; set; }
        public string? ContactPersonName { get; set; }
        public string? ContactPersonPhone { get; set; }
        public string? ContactPersonEmail { get; set; }
        public decimal? LastSeenLatitude { get; set; }
        public decimal? LastSeenLongitude { get; set; }
        public string? CaseImageUrls { get; set; }
        public string? DocumentUrls { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string? EmergencyContactRelationship { get; set; }
        public bool IsVerified { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public string? VerifiedBy { get; set; }
        public bool IsApproved { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedBy { get; set; }
        public int ViewCount { get; set; }
        public int ShareCount { get; set; }
        public int TipCount { get; set; }
        
        // Related data
        public RunnerResponseDto? Runner { get; set; }
        public string? ReportedByUserName { get; set; }
        public string? ReportedByUserEmail { get; set; }
    }
}
