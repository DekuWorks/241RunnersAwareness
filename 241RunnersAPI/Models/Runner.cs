using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _241RunnersAPI.Models
{
    /// <summary>
    /// Runner model representing individuals who participate in running activities
    /// </summary>
    public class Runner
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        [Required(ErrorMessage = "Runner name is required")]
        [MaxLength(200, ErrorMessage = "Runner name cannot exceed 200 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-'\.]+$", ErrorMessage = "Runner name can only contain letters, spaces, hyphens, apostrophes, and periods")]
        public string Name { get; set; } = string.Empty;

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

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

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

        [MaxLength(1000, ErrorMessage = "Additional image URLs cannot exceed 1000 characters")]
        public string? AdditionalImageUrls { get; set; } // JSON array of image URLs

        // Verification status
        public bool IsProfileComplete { get; set; } = false;
        public bool IsVerified { get; set; } = false;
        public DateTime? VerifiedAt { get; set; }
        public string? VerifiedBy { get; set; }
    }

    /// <summary>
    /// DTO for runner registration
    /// </summary>
    public class RunnerRegistrationDto
    {
        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Runner name is required")]
        [MaxLength(200, ErrorMessage = "Runner name cannot exceed 200 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-'\.]+$", ErrorMessage = "Runner name can only contain letters, spaces, hyphens, apostrophes, and periods")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date of birth is required")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        [MaxLength(20, ErrorMessage = "Gender cannot exceed 20 characters")]
        [RegularExpression("^(Male|Female|Other|Prefer not to say)$", ErrorMessage = "Gender must be one of: Male, Female, Other, Prefer not to say")]
        public string Gender { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Physical description cannot exceed 500 characters")]
        public string? PhysicalDescription { get; set; }

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

        [MaxLength(50, ErrorMessage = "Preferred contact method cannot exceed 50 characters")]
        [RegularExpression("^(Phone|Email|Text|Emergency Contact)$", ErrorMessage = "Preferred contact method must be one of: Phone, Email, Text, Emergency Contact")]
        public string? PreferredContactMethod { get; set; }
    }

    /// <summary>
    /// DTO for runner update
    /// </summary>
    public class RunnerUpdateDto
    {
        [Required(ErrorMessage = "Runner name is required")]
        [MaxLength(200, ErrorMessage = "Runner name cannot exceed 200 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-'\.]+$", ErrorMessage = "Runner name can only contain letters, spaces, hyphens, apostrophes, and periods")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date of birth is required")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        [MaxLength(20, ErrorMessage = "Gender cannot exceed 20 characters")]
        [RegularExpression("^(Male|Female|Other|Prefer not to say)$", ErrorMessage = "Gender must be one of: Male, Female, Other, Prefer not to say")]
        public string Gender { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Physical description cannot exceed 500 characters")]
        public string? PhysicalDescription { get; set; }

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

        [MaxLength(50, ErrorMessage = "Preferred contact method cannot exceed 50 characters")]
        [RegularExpression("^(Phone|Email|Text|Emergency Contact)$", ErrorMessage = "Preferred contact method must be one of: Phone, Email, Text, Emergency Contact")]
        public string? PreferredContactMethod { get; set; }
    }

    /// <summary>
    /// DTO for runner response (without sensitive data)
    /// </summary>
    public class RunnerResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string? PhysicalDescription { get; set; }
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
}
