using System.ComponentModel.DataAnnotations;

namespace _241RunnersAwarenessAPI.Models
{
    public class Runner
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "First name is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "First name must be between 1 and 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "First name can only contain letters, spaces, hyphens, and apostrophes")]
        public string FirstName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Last name must be between 1 and 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "Last name can only contain letters, spaces, hyphens, and apostrophes")]
        public string LastName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Runner ID is required")]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "Runner ID must be between 5 and 50 characters")]
        [RegularExpression(@"^[A-Z0-9\-]+$", ErrorMessage = "Runner ID can only contain uppercase letters, numbers, and hyphens")]
        public string RunnerId { get; set; } = string.Empty; // e.g., "RUN-2024-001"
        
        [Range(0, 120, ErrorMessage = "Age must be between 0 and 120")]
        public int Age { get; set; }
        
        [StringLength(50, ErrorMessage = "Gender cannot exceed 50 characters")]
        public string Gender { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Status is required")]
        [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
        [RegularExpression(@"^(missing|found|safe|urgent|active|inactive)$", ErrorMessage = "Status must be one of: missing, found, safe, urgent, active, inactive")]
        public string Status { get; set; } = "missing";
        
        [Required(ErrorMessage = "City is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "City must be between 1 and 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "City can only contain letters, spaces, hyphens, and apostrophes")]
        public string City { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "State is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "State must be between 2 and 50 characters")]
        [RegularExpression(@"^[A-Z]{2}$", ErrorMessage = "State must be a 2-letter abbreviation")]
        public string State { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        public string Address { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = string.Empty;
        
        [StringLength(200, ErrorMessage = "Contact info cannot exceed 200 characters")]
        [RegularExpression(@"^[\w\s\-\(\)\+\.@]+$", ErrorMessage = "Contact info contains invalid characters")]
        public string ContactInfo { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Date reported is required")]
        public DateTime DateReported { get; set; } = DateTime.UtcNow;
        
        public DateTime? DateFound { get; set; }
        
        public DateTime? LastSeen { get; set; }
        
        public DateTime? DateOfBirth { get; set; }
        
        [StringLength(500, ErrorMessage = "Tags cannot exceed 500 characters")]
        public string Tags { get; set; } = string.Empty; // Comma-separated tags
        
        [Required(ErrorMessage = "Active status is required")]
        public bool IsActive { get; set; } = true;
        
        public bool IsUrgent { get; set; } = false;
        
        [Required(ErrorMessage = "Created date is required")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Physical description fields
        [StringLength(50, ErrorMessage = "Height cannot exceed 50 characters")]
        public string Height { get; set; } = string.Empty;
        
        [StringLength(50, ErrorMessage = "Weight cannot exceed 50 characters")]
        public string Weight { get; set; } = string.Empty;
        
        [StringLength(50, ErrorMessage = "Hair color cannot exceed 50 characters")]
        public string HairColor { get; set; } = string.Empty;
        
        [StringLength(50, ErrorMessage = "Eye color cannot exceed 50 characters")]
        public string EyeColor { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "Identifying marks cannot exceed 500 characters")]
        public string IdentifyingMarks { get; set; } = string.Empty;
        
        // Medical information
        [StringLength(1000, ErrorMessage = "Medical conditions cannot exceed 1000 characters")]
        public string MedicalConditions { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "Medications cannot exceed 500 characters")]
        public string Medications { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "Allergies cannot exceed 500 characters")]
        public string Allergies { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "Emergency contacts cannot exceed 500 characters")]
        public string EmergencyContacts { get; set; } = string.Empty;
        
        // Navigation properties
        public int? ReportedByUserId { get; set; }
        public User? ReportedByUser { get; set; }
        
        // Computed properties
        public string FullName => $"{FirstName} {LastName}".Trim();
        
        public int? CalculatedAge
        {
            get
            {
                if (DateOfBirth.HasValue)
                {
                    var today = DateTime.Today;
                    var age = today.Year - DateOfBirth.Value.Year;
                    if (DateOfBirth.Value.Date > today.AddYears(-age)) age--;
                    return age;
                }
                return Age > 0 ? Age : null;
            }
        }
    }

    public class RunnerDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string RunnerId { get; set; } = string.Empty;
        public int Age { get; set; }
        public int? CalculatedAge { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ContactInfo { get; set; } = string.Empty;
        public DateTime DateReported { get; set; }
        public DateTime? DateFound { get; set; }
        public DateTime? LastSeen { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public bool IsActive { get; set; }
        public bool IsUrgent { get; set; }
        public string Height { get; set; } = string.Empty;
        public string Weight { get; set; } = string.Empty;
        public string HairColor { get; set; } = string.Empty;
        public string EyeColor { get; set; } = string.Empty;
        public string IdentifyingMarks { get; set; } = string.Empty;
        public string MedicalConditions { get; set; } = string.Empty;
        public string Medications { get; set; } = string.Empty;
        public string Allergies { get; set; } = string.Empty;
        public string EmergencyContacts { get; set; } = string.Empty;
        public string ReportedBy { get; set; } = string.Empty;
    }

    public class CreateRunnerRequest
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "First name must be between 1 and 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "First name can only contain letters, spaces, hyphens, and apostrophes")]
        public string FirstName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Last name must be between 1 and 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "Last name can only contain letters, spaces, hyphens, and apostrophes")]
        public string LastName { get; set; } = string.Empty;
        
        [Range(0, 120, ErrorMessage = "Age must be between 0 and 120")]
        public int Age { get; set; }
        
        [StringLength(50, ErrorMessage = "Gender cannot exceed 50 characters")]
        public string Gender { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "City is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "City must be between 1 and 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "City can only contain letters, spaces, hyphens, and apostrophes")]
        public string City { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "State is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "State must be between 2 and 50 characters")]
        [RegularExpression(@"^[A-Z]{2}$", ErrorMessage = "State must be a 2-letter abbreviation")]
        public string State { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        public string Address { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = string.Empty;
        
        [StringLength(200, ErrorMessage = "Contact info cannot exceed 200 characters")]
        [RegularExpression(@"^[\w\s\-\(\)\+\.@]+$", ErrorMessage = "Contact info contains invalid characters")]
        public string ContactInfo { get; set; } = string.Empty;
        
        public DateTime? LastSeen { get; set; }
        
        public DateTime? DateOfBirth { get; set; }
        
        [StringLength(500, ErrorMessage = "Tags cannot exceed 500 characters")]
        public string Tags { get; set; } = string.Empty;
        
        public bool IsUrgent { get; set; } = false;
        
        [StringLength(50, ErrorMessage = "Height cannot exceed 50 characters")]
        public string Height { get; set; } = string.Empty;
        
        [StringLength(50, ErrorMessage = "Weight cannot exceed 50 characters")]
        public string Weight { get; set; } = string.Empty;
        
        [StringLength(50, ErrorMessage = "Hair color cannot exceed 50 characters")]
        public string HairColor { get; set; } = string.Empty;
        
        [StringLength(50, ErrorMessage = "Eye color cannot exceed 50 characters")]
        public string EyeColor { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "Identifying marks cannot exceed 500 characters")]
        public string IdentifyingMarks { get; set; } = string.Empty;
        
        [StringLength(1000, ErrorMessage = "Medical conditions cannot exceed 1000 characters")]
        public string MedicalConditions { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "Medications cannot exceed 500 characters")]
        public string Medications { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "Allergies cannot exceed 500 characters")]
        public string Allergies { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "Emergency contacts cannot exceed 500 characters")]
        public string EmergencyContacts { get; set; } = string.Empty;
    }

    public class UpdateRunnerRequest
    {
        [StringLength(100, MinimumLength = 1, ErrorMessage = "First name must be between 1 and 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "First name can only contain letters, spaces, hyphens, and apostrophes")]
        public string? FirstName { get; set; }
        
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Last name must be between 1 and 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "Last name can only contain letters, spaces, hyphens, and apostrophes")]
        public string? LastName { get; set; }
        
        [Range(0, 120, ErrorMessage = "Age must be between 0 and 120")]
        public int? Age { get; set; }
        
        [StringLength(50, ErrorMessage = "Gender cannot exceed 50 characters")]
        public string? Gender { get; set; }
        
        [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
        [RegularExpression(@"^(missing|found|safe|urgent|active|inactive)$", ErrorMessage = "Status must be one of: missing, found, safe, urgent, active, inactive")]
        public string? Status { get; set; }
        
        [StringLength(100, MinimumLength = 1, ErrorMessage = "City must be between 1 and 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "City can only contain letters, spaces, hyphens, and apostrophes")]
        public string? City { get; set; }
        
        [StringLength(50, MinimumLength = 2, ErrorMessage = "State must be between 2 and 50 characters")]
        [RegularExpression(@"^[A-Z]{2}$", ErrorMessage = "State must be a 2-letter abbreviation")]
        public string? State { get; set; }
        
        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        public string? Address { get; set; }
        
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }
        
        [StringLength(200, ErrorMessage = "Contact info cannot exceed 200 characters")]
        [RegularExpression(@"^[\w\s\-\(\)\+\.@]+$", ErrorMessage = "Contact info contains invalid characters")]
        public string? ContactInfo { get; set; }
        
        public DateTime? LastSeen { get; set; }
        
        public DateTime? DateOfBirth { get; set; }
        
        [StringLength(500, ErrorMessage = "Tags cannot exceed 500 characters")]
        public string? Tags { get; set; }
        
        public bool? IsUrgent { get; set; }
        
        [StringLength(50, ErrorMessage = "Height cannot exceed 50 characters")]
        public string? Height { get; set; }
        
        [StringLength(50, ErrorMessage = "Weight cannot exceed 50 characters")]
        public string? Weight { get; set; }
        
        [StringLength(50, ErrorMessage = "Hair color cannot exceed 50 characters")]
        public string? HairColor { get; set; }
        
        [StringLength(50, ErrorMessage = "Eye color cannot exceed 50 characters")]
        public string? EyeColor { get; set; }
        
        [StringLength(500, ErrorMessage = "Identifying marks cannot exceed 500 characters")]
        public string? IdentifyingMarks { get; set; }
        
        [StringLength(1000, ErrorMessage = "Medical conditions cannot exceed 1000 characters")]
        public string? MedicalConditions { get; set; }
        
        [StringLength(500, ErrorMessage = "Medications cannot exceed 500 characters")]
        public string? Medications { get; set; }
        
        [StringLength(500, ErrorMessage = "Allergies cannot exceed 500 characters")]
        public string? Allergies { get; set; }
        
        [StringLength(500, ErrorMessage = "Emergency contacts cannot exceed 500 characters")]
        public string? EmergencyContacts { get; set; }
    }

    public class RunnerSearchDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string RunnerId { get; set; } = string.Empty;
        public int Age { get; set; }
        public int? CalculatedAge { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime DateReported { get; set; }
        public DateTime? LastSeen { get; set; }
        public bool IsUrgent { get; set; }
        public string Height { get; set; } = string.Empty;
        public string Weight { get; set; } = string.Empty;
        public string HairColor { get; set; } = string.Empty;
        public string EyeColor { get; set; } = string.Empty;
        public string IdentifyingMarks { get; set; } = string.Empty;
    }
} 