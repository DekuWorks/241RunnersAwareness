using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _241RunnersAwarenessAPI.Models
{
    public class PublicCase
    {
        [Key]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "NamUs case number is required")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "NamUs case number must be between 1 and 50 characters")]
        public string NamusCaseNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full name is required")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Full name must be between 1 and 200 characters")]
        public string FullName { get; set; } = string.Empty;

        [StringLength(10, ErrorMessage = "Sex must be 10 characters or less")]
        public string? Sex { get; set; }

        [Range(0, 120, ErrorMessage = "Age at missing must be between 0 and 120")]
        public int? AgeAtMissing { get; set; }

        public DateTime? DateMissing { get; set; }

        [StringLength(100, ErrorMessage = "City must be 100 characters or less")]
        public string? City { get; set; }

        [StringLength(100, ErrorMessage = "County must be 100 characters or less")]
        public string? County { get; set; }

        [Required(ErrorMessage = "State is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "State must be between 2 and 50 characters")]
        public string State { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Agency must be 200 characters or less")]
        public string? Agency { get; set; }

        [StringLength(500, ErrorMessage = "Photo URL must be 500 characters or less")]
        [Url(ErrorMessage = "Photo URL must be a valid URL")]
        public string? PhotoUrl { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [StringLength(50, ErrorMessage = "Status must be 50 characters or less")]
        public string Status { get; set; } = "missing";

        [StringLength(500, ErrorMessage = "Status note must be 500 characters or less")]
        public string? StatusNote { get; set; }

        [StringLength(500, ErrorMessage = "Source URL must be 500 characters or less")]
        [Url(ErrorMessage = "Source URL must be a valid URL")]
        public string? SourceUrl { get; set; }

        public DateTime? SourceLastChecked { get; set; }

        [StringLength(500, ErrorMessage = "Verification source must be 500 characters or less")]
        public string? VerificationSource { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    // DTOs for API responses
    public class PublicCaseDto
    {
        public Guid Id { get; set; }
        public string NamusCaseNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Sex { get; set; }
        public int? AgeAtMissing { get; set; }
        public DateTime? DateMissing { get; set; }
        public string? City { get; set; }
        public string? County { get; set; }
        public string State { get; set; } = string.Empty;
        public string? Agency { get; set; }
        public string? PhotoUrl { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? StatusNote { get; set; }
        public string? SourceUrl { get; set; }
        public DateTime? SourceLastChecked { get; set; }
        public string? VerificationSource { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class PublicCaseSearchDto
    {
        public string? Region { get; set; }
        public string? Status { get; set; }
        public string? City { get; set; }
        public string? County { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class NamusImportRequest
    {
        [Required(ErrorMessage = "CSV file is required")]
        public IFormFile CsvFile { get; set; } = null!;
    }

    public class TxDpsCheckRequest
    {
        [Required(ErrorMessage = "Case ID is required")]
        public Guid CaseId { get; set; }

        [Required(ErrorMessage = "Verification source URL is required")]
        [Url(ErrorMessage = "Verification source must be a valid URL")]
        public string VerificationSource { get; set; } = string.Empty;

        [Required(ErrorMessage = "New status is required")]
        [StringLength(50, ErrorMessage = "Status must be 50 characters or less")]
        public string NewStatus { get; set; } = string.Empty;

        public string? Notes { get; set; }
    }
} 