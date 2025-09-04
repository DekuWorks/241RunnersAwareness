using System.ComponentModel.DataAnnotations;

namespace _241RunnersAwarenessAPI.Models
{
    public class ImageUploadRequest
    {
        [Required]
        public IFormFile Image { get; set; } = null!;
        
        [MaxLength(100)]
        public string? Description { get; set; }
        
        [MaxLength(50)]
        public string? Category { get; set; } // "profile", "case", "document", etc.
        
        public int? RelatedId { get; set; } // User ID or Runner ID
    }

    public class ImageUploadResponse
    {
        public bool Success { get; set; }
        public string? ImageUrl { get; set; }
        public string? Message { get; set; }
        public string? Error { get; set; }
    }

    public class MultipleImageUploadRequest
    {
        [Required]
        public List<IFormFile> Images { get; set; } = new();
        
        [MaxLength(100)]
        public string? Description { get; set; }
        
        [MaxLength(50)]
        public string? Category { get; set; }
        
        public int? RelatedId { get; set; }
    }

    public class MultipleImageUploadResponse
    {
        public bool Success { get; set; }
        public List<string> ImageUrls { get; set; } = new();
        public string? Message { get; set; }
        public string? Error { get; set; }
    }

    public class ImageDeleteRequest
    {
        [Required]
        public string ImageUrl { get; set; } = string.Empty;
        
        public int? RelatedId { get; set; }
    }

    public class ImageDeleteResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? Error { get; set; }
    }
} 