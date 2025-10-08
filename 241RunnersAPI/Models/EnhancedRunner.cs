using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _241RunnersAPI.Models
{
    /// <summary>
    /// Enhanced Runner model with comprehensive fields for better case tracking
    /// </summary>
    public class EnhancedRunner
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        public int? CreatedByUserId { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [Required]
        [MaxLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
        [RegularExpression("^(Missing|Found|Resolved|Active|Safe)$", ErrorMessage = "Status must be one of: Missing, Found, Resolved, Active, Safe")]
        public string Status { get; set; } = "Active";

        [Required(ErrorMessage = "Runner name is required")]
        [MaxLength(200, ErrorMessage = "Runner name cannot exceed 200 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-'\.]+$", ErrorMessage = "Runner name can only contain letters, spaces, hyphens, apostrophes, and periods")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "First name is required")]
        [MaxLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "First name can only contain letters, spaces, hyphens, and apostrophes")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [MaxLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "Last name can only contain letters, spaces, hyphens, and apostrophes")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date of birth is required")]
        public DateTime DateOfBirth { get; set; }

        [NotMapped]
        public int Age => DateTime.UtcNow.Year - DateOfBirth.Year - (DateTime.UtcNow.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);

        [Required(ErrorMessage = "Gender is required")]
        [MaxLength(20, ErrorMessage = "Gender cannot exceed 20 characters")]
        [RegularExpression("^(Male|Female|Other|Prefer not to say)$", ErrorMessage = "Gender must be one of: Male, Female, Other, Prefer not to say")]
        public string Gender { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Physical description cannot exceed 500 characters")]
        public string? PhysicalDescription { get; set; }

        [MaxLength(10, ErrorMessage = "Height cannot exceed 10 characters")]
        public string? Height { get; set; } // e.g., "5'8\"", "175cm"

        [MaxLength(10, ErrorMessage = "Weight cannot exceed 10 characters")]
        public string? Weight { get; set; } // e.g., "150 lbs", "68 kg"

        [MaxLength(20, ErrorMessage = "Eye color cannot exceed 20 characters")]
        public string? EyeColor { get; set; }

        [MaxLength(1000, ErrorMessage = "Medical conditions cannot exceed 1000 characters")]
        public string? MedicalConditions { get; set; }

        [MaxLength(1000, ErrorMessage = "Medications cannot exceed 1000 characters")]
        public string? Medications { get; set; }

        [MaxLength(1000, ErrorMessage = "Allergies cannot exceed 1000 characters")]
        public string? Allergies { get; set; }

        [MaxLength(500, ErrorMessage = "Emergency instructions cannot exceed 500 characters")]
        public string? EmergencyInstructions { get; set; }

        [MaxLength(200, ErrorMessage = "Preferred running locations cannot exceed 200 characters")]
        public string? PreferredRunningLocations { get; set; }

        [MaxLength(200, ErrorMessage = "Typical running times cannot exceed 200 characters")]
        public string? TypicalRunningTimes { get; set; }

        [MaxLength(100, ErrorMessage = "Running experience level cannot exceed 100 characters")]
        [RegularExpression("^(Beginner|Intermediate|Advanced|Expert)$", ErrorMessage = "Experience level must be one of: Beginner, Intermediate, Advanced, Expert")]
        public string? ExperienceLevel { get; set; }

        [MaxLength(500, ErrorMessage = "Special needs cannot exceed 500 characters")]
        public string? SpecialNeeds { get; set; }

        [MaxLength(1000, ErrorMessage = "Additional notes cannot exceed 1000 characters")]
        public string? AdditionalNotes { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        // GPS and tracking information
        [MaxLength(50, ErrorMessage = "Last known location cannot exceed 50 characters")]
        public string? LastKnownLocation { get; set; }

        public DateTime? LastLocationUpdate { get; set; }

        [MaxLength(50, ErrorMessage = "Preferred contact method cannot exceed 50 characters")]
        [RegularExpression("^(Phone|Email|Text|Emergency Contact)$", ErrorMessage = "Preferred contact method must be one of: Phone, Email, Text, Emergency Contact")]
        public string? PreferredContactMethod { get; set; }

        // Profile images
        [MaxLength(500, ErrorMessage = "Profile image URL cannot exceed 500 characters")]
        [Url(ErrorMessage = "Profile image URL must be a valid URL")]
        public string? ProfileImageUrl { get; set; }

        [MaxLength(2000, ErrorMessage = "Additional image URLs cannot exceed 2000 characters")]
        public string? AdditionalImageUrls { get; set; } // JSON array of image URLs

        // Photo management for 6-month update reminders
        public DateTime? LastPhotoUpdate { get; set; }
        public DateTime? NextPhotoReminder { get; set; } // 6 months from last update
        public bool PhotoUpdateReminderSent { get; set; } = false;
        public int PhotoUpdateReminderCount { get; set; } = 0;

        // Verification status
        public bool IsProfileComplete { get; set; } = false;
        public bool IsVerified { get; set; } = false;
        public DateTime? VerifiedAt { get; set; }
        public string? VerifiedBy { get; set; }
    }

    /// <summary>
    /// Enhanced DTO for runner registration with all required fields
    /// </summary>
    public class EnhancedRunnerRegistrationDto
    {
        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [MaxLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "First name can only contain letters, spaces, hyphens, and apostrophes")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [MaxLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "Last name can only contain letters, spaces, hyphens, and apostrophes")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date of birth is required")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        [MaxLength(20, ErrorMessage = "Gender cannot exceed 20 characters")]
        [RegularExpression("^(Male|Female|Other|Prefer not to say)$", ErrorMessage = "Gender must be one of: Male, Female, Other, Prefer not to say")]
        public string Gender { get; set; } = string.Empty;

        [MaxLength(10, ErrorMessage = "Height cannot exceed 10 characters")]
        public string? Height { get; set; }

        [MaxLength(10, ErrorMessage = "Weight cannot exceed 10 characters")]
        public string? Weight { get; set; }

        [MaxLength(20, ErrorMessage = "Eye color cannot exceed 20 characters")]
        public string? EyeColor { get; set; }

        [MaxLength(1000, ErrorMessage = "Medical conditions cannot exceed 1000 characters")]
        public string? MedicalConditions { get; set; }

        [MaxLength(500, ErrorMessage = "Physical description cannot exceed 500 characters")]
        public string? PhysicalDescription { get; set; }

        [MaxLength(1000, ErrorMessage = "Medications cannot exceed 1000 characters")]
        public string? Medications { get; set; }

        [MaxLength(1000, ErrorMessage = "Allergies cannot exceed 1000 characters")]
        public string? Allergies { get; set; }

        [MaxLength(500, ErrorMessage = "Emergency instructions cannot exceed 500 characters")]
        public string? EmergencyInstructions { get; set; }

        [MaxLength(200, ErrorMessage = "Preferred running locations cannot exceed 200 characters")]
        public string? PreferredRunningLocations { get; set; }

        [MaxLength(200, ErrorMessage = "Typical running times cannot exceed 200 characters")]
        public string? TypicalRunningTimes { get; set; }

        [MaxLength(100, ErrorMessage = "Running experience level cannot exceed 100 characters")]
        [RegularExpression("^(Beginner|Intermediate|Advanced|Expert)$", ErrorMessage = "Experience level must be one of: Beginner, Intermediate, Advanced, Expert")]
        public string? ExperienceLevel { get; set; }

        [MaxLength(500, ErrorMessage = "Special needs cannot exceed 500 characters")]
        public string? SpecialNeeds { get; set; }

        [MaxLength(1000, ErrorMessage = "Additional notes cannot exceed 1000 characters")]
        public string? AdditionalNotes { get; set; }

        [MaxLength(50, ErrorMessage = "Preferred contact method cannot exceed 50 characters")]
        [RegularExpression("^(Phone|Email|Text|Emergency Contact)$", ErrorMessage = "Preferred contact method must be one of: Phone, Email, Text, Emergency Contact")]
        public string? PreferredContactMethod { get; set; }
    }

    /// <summary>
    /// Enhanced DTO for runner update
    /// </summary>
    public class EnhancedRunnerUpdateDto
    {
        [Required(ErrorMessage = "First name is required")]
        [MaxLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "First name can only contain letters, spaces, hyphens, and apostrophes")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [MaxLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "Last name can only contain letters, spaces, hyphens, and apostrophes")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date of birth is required")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        [MaxLength(20, ErrorMessage = "Gender cannot exceed 20 characters")]
        [RegularExpression("^(Male|Female|Other|Prefer not to say)$", ErrorMessage = "Gender must be one of: Male, Female, Other, Prefer not to say")]
        public string Gender { get; set; } = string.Empty;

        [MaxLength(10, ErrorMessage = "Height cannot exceed 10 characters")]
        public string? Height { get; set; }

        [MaxLength(10, ErrorMessage = "Weight cannot exceed 10 characters")]
        public string? Weight { get; set; }

        [MaxLength(20, ErrorMessage = "Eye color cannot exceed 20 characters")]
        public string? EyeColor { get; set; }

        [MaxLength(1000, ErrorMessage = "Medical conditions cannot exceed 1000 characters")]
        public string? MedicalConditions { get; set; }

        [MaxLength(500, ErrorMessage = "Physical description cannot exceed 500 characters")]
        public string? PhysicalDescription { get; set; }

        [MaxLength(1000, ErrorMessage = "Medications cannot exceed 1000 characters")]
        public string? Medications { get; set; }

        [MaxLength(1000, ErrorMessage = "Allergies cannot exceed 1000 characters")]
        public string? Allergies { get; set; }

        [MaxLength(500, ErrorMessage = "Emergency instructions cannot exceed 500 characters")]
        public string? EmergencyInstructions { get; set; }

        [MaxLength(200, ErrorMessage = "Preferred running locations cannot exceed 200 characters")]
        public string? PreferredRunningLocations { get; set; }

        [MaxLength(200, ErrorMessage = "Typical running times cannot exceed 200 characters")]
        public string? TypicalRunningTimes { get; set; }

        [MaxLength(100, ErrorMessage = "Running experience level cannot exceed 100 characters")]
        [RegularExpression("^(Beginner|Intermediate|Advanced|Expert)$", ErrorMessage = "Experience level must be one of: Beginner, Intermediate, Advanced, Expert")]
        public string? ExperienceLevel { get; set; }

        [MaxLength(500, ErrorMessage = "Special needs cannot exceed 500 characters")]
        public string? SpecialNeeds { get; set; }

        [MaxLength(1000, ErrorMessage = "Additional notes cannot exceed 1000 characters")]
        public string? AdditionalNotes { get; set; }

        [MaxLength(50, ErrorMessage = "Preferred contact method cannot exceed 50 characters")]
        [RegularExpression("^(Phone|Email|Text|Emergency Contact)$", ErrorMessage = "Preferred contact method must be one of: Phone, Email, Text, Emergency Contact")]
        public string? PreferredContactMethod { get; set; }
    }

    /// <summary>
    /// Enhanced DTO for runner response with photo management info
    /// </summary>
    public class EnhancedRunnerResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? PhysicalDescription { get; set; }
        public string? Height { get; set; }
        public string? Weight { get; set; }
        public string? EyeColor { get; set; }
        public string? MedicalConditions { get; set; }
        public string? Medications { get; set; }
        public string? Allergies { get; set; }
        public string? EmergencyInstructions { get; set; }
        public string? PreferredRunningLocations { get; set; }
        public string? TypicalRunningTimes { get; set; }
        public string? ExperienceLevel { get; set; }
        public string? SpecialNeeds { get; set; }
        public string? AdditionalNotes { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? LastKnownLocation { get; set; }
        public DateTime? LastLocationUpdate { get; set; }
        public string? PreferredContactMethod { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? AdditionalImageUrls { get; set; }
        
        // Photo management fields
        public DateTime? LastPhotoUpdate { get; set; }
        public DateTime? NextPhotoReminder { get; set; }
        public bool PhotoUpdateReminderSent { get; set; }
        public int PhotoUpdateReminderCount { get; set; }
        public bool IsPhotoUpdateRequired { get; set; } // Calculated field
        
        public bool IsProfileComplete { get; set; }
        public bool IsVerified { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public string? VerifiedBy { get; set; }
        
        // User information
        public string? UserEmail { get; set; }
        public string? UserPhoneNumber { get; set; }
        public string? UserEmergencyContactName { get; set; }
        public string? UserEmergencyContactPhone { get; set; }
    }

    /// <summary>
    /// DTO for runner photo upload
    /// </summary>
    public class RunnerPhotoUploadDto
    {
        [Required(ErrorMessage = "Runner ID is required")]
        public int RunnerId { get; set; }

        [Required(ErrorMessage = "Photo type is required")]
        [RegularExpression("^(Profile|Additional)$", ErrorMessage = "Photo type must be Profile or Additional")]
        public string PhotoType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Image file is required")]
        public IFormFile ImageFile { get; set; } = null!;

        [MaxLength(200, ErrorMessage = "Description cannot exceed 200 characters")]
        public string? Description { get; set; }
    }
}
