using System.ComponentModel.DataAnnotations;

namespace _241RunnersAwarenessAPI.Models
{
    public class ImageUploadRequest
    {
        [Required(ErrorMessage = "Image file is required")]
        public IFormFile Image { get; set; } = null!;
        
        [MaxLength(100, ErrorMessage = "Description cannot exceed 100 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-'.,!?()]+$", ErrorMessage = "Description contains invalid characters")]
        public string? Description { get; set; }
        
        [Required(ErrorMessage = "Category is required")]
        [MaxLength(50, ErrorMessage = "Category cannot exceed 50 characters")]
        [RegularExpression(@"^(profile|case|document|evidence|general)$", ErrorMessage = "Category must be one of: profile, case, document, evidence, general")]
        public string Category { get; set; } = "general";
        
        [Range(1, int.MaxValue, ErrorMessage = "Related ID must be a positive number")]
        public int? RelatedId { get; set; }
        
        [MaxLength(200, ErrorMessage = "Tags cannot exceed 200 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-_,]+$", ErrorMessage = "Tags contain invalid characters")]
        public string? Tags { get; set; }
        
        [MaxLength(100, ErrorMessage = "Source cannot exceed 100 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-_.]+$", ErrorMessage = "Source contains invalid characters")]
        public string? Source { get; set; } // Where the image came from
        
        public bool IsPublic { get; set; } = false; // Whether the image should be publicly accessible
    }

    public class ImageUploadResponse
    {
        public bool Success { get; set; }
        public string? ImageUrl { get; set; }
        public string? ImageId { get; set; }
        public string? Message { get; set; }
        public string? Error { get; set; }
        public ImageMetadata? Metadata { get; set; }
    }

    public class MultipleImageUploadRequest
    {
        [Required(ErrorMessage = "At least one image is required")]
        [MinLength(1, ErrorMessage = "At least one image must be provided")]
        [MaxLength(10, ErrorMessage = "Maximum 10 images can be uploaded at once")]
        public List<IFormFile> Images { get; set; } = new();
        
        [MaxLength(100, ErrorMessage = "Description cannot exceed 100 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-'.,!?()]+$", ErrorMessage = "Description contains invalid characters")]
        public string? Description { get; set; }
        
        [Required(ErrorMessage = "Category is required")]
        [MaxLength(50, ErrorMessage = "Category cannot exceed 50 characters")]
        [RegularExpression(@"^(profile|case|document|evidence|general)$", ErrorMessage = "Category must be one of: profile, case, document, evidence, general")]
        public string Category { get; set; } = "general";
        
        [Range(1, int.MaxValue, ErrorMessage = "Related ID must be a positive number")]
        public int? RelatedId { get; set; }
        
        [MaxLength(200, ErrorMessage = "Tags cannot exceed 200 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-_,]+$", ErrorMessage = "Tags contain invalid characters")]
        public string? Tags { get; set; }
        
        public bool IsPublic { get; set; } = false;
    }

    public class MultipleImageUploadResponse
    {
        public bool Success { get; set; }
        public List<string> ImageUrls { get; set; } = new();
        public List<string> ImageIds { get; set; } = new();
        public string? Message { get; set; }
        public string? Error { get; set; }
        public List<ImageMetadata> Metadata { get; set; } = new();
        public int TotalUploaded { get; set; }
        public int TotalFailed { get; set; }
    }

    public class ImageDeleteRequest
    {
        [Required(ErrorMessage = "Image URL is required")]
        [Url(ErrorMessage = "Invalid image URL format")]
        public string ImageUrl { get; set; } = string.Empty;
        
        [Range(1, int.MaxValue, ErrorMessage = "Related ID must be a positive number")]
        public int? RelatedId { get; set; }
        
        [MaxLength(200, ErrorMessage = "Reason cannot exceed 200 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-'.,!?()]+$", ErrorMessage = "Reason contains invalid characters")]
        public string? Reason { get; set; }
    }

    public class ImageDeleteResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? Error { get; set; }
        public DateTime? DeletedAt { get; set; }
    }

    public class ImageMetadata
    {
        public string ImageId { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string FileExtension { get; set; } = string.Empty;
        public string MimeType { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string? Description { get; set; }
        public string? Tags { get; set; }
        public string Category { get; set; } = string.Empty;
        public string UploadedBy { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        public bool IsPublic { get; set; }
        public string? Checksum { get; set; } // SHA256 hash for integrity
    }

    public class ImageSearchRequest
    {
        [MaxLength(50, ErrorMessage = "Category cannot exceed 50 characters")]
        [RegularExpression(@"^(profile|case|document|evidence|general|all)$", ErrorMessage = "Invalid category")]
        public string? Category { get; set; }
        
        [Range(1, int.MaxValue, ErrorMessage = "Related ID must be a positive number")]
        public int? RelatedId { get; set; }
        
        [MaxLength(200, ErrorMessage = "Tags cannot exceed 200 characters")]
        public string? Tags { get; set; }
        
        [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
        public int PageSize { get; set; } = 20;
        
        [Range(1, int.MaxValue, ErrorMessage = "Page number must be a positive number")]
        public int Page { get; set; } = 1;
        
        public bool IncludePublic { get; set; } = true;
        public bool IncludePrivate { get; set; } = false;
    }

    public class ImageSearchResponse
    {
        public bool Success { get; set; }
        public List<ImageMetadata> Images { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public string? Message { get; set; }
        public string? Error { get; set; }
    }
} 