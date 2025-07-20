using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _241RunnersAwareness.BackendAPI.DBContext.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(1000)]
        public string? LongDescription { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? SalePrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? CompareAtPrice { get; set; }

        [StringLength(50)]
        public string? SKU { get; set; }

        [StringLength(50)]
        public string? Barcode { get; set; }

        [StringLength(100)]
        public string? Brand { get; set; } // "Varlo", "241RA", "241RA x Varlo"

        [StringLength(100)]
        public string? Category { get; set; } // "Running", "Triathlon", "Cycling", "Everyday", "Accessories"

        [StringLength(100)]
        public string? Subcategory { get; set; } // "Singlets", "Shorts", "Tees", "Bibs", "Jerseys"

        [StringLength(100)]
        public string? Collection { get; set; } // "Heritage", "Mercury", "Wildbloom", "241RA Awareness"

        [StringLength(50)]
        public string? Gender { get; set; } // "Men", "Women", "Unisex"

        [StringLength(100)]
        public string? Size { get; set; } // "XS", "S", "M", "L", "XL", "XXL"

        [StringLength(100)]
        public string? Color { get; set; }

        [StringLength(500)]
        public string? ImageUrls { get; set; } // JSON array of image URLs

        [StringLength(500)]
        public string? Tags { get; set; } // Comma-separated tags

        public bool IsActive { get; set; } = true;

        public bool IsFeatured { get; set; } = false;

        public bool IsOnSale { get; set; } = false;

        public int StockQuantity { get; set; } = 0;

        public int? MinOrderQuantity { get; set; }

        public int? MaxOrderQuantity { get; set; }

        public bool TrackInventory { get; set; } = true;

        public bool AllowBackorders { get; set; } = false;

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Weight { get; set; } // in pounds

        [StringLength(50)]
        public string? WeightUnit { get; set; } // "lb", "kg"

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Length { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Width { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Height { get; set; }

        [StringLength(50)]
        public string? DimensionUnit { get; set; } // "in", "cm"

        // 241RA Specific Fields
        [StringLength(100)]
        public string? AwarenessType { get; set; } // "Missing Persons", "Special Needs", "Law Enforcement"

        [StringLength(500)]
        public string? MissionStatement { get; set; } // Product-specific mission info

        [StringLength(200)]
        public string? PartnerOrganization { get; set; } // "Varlo", "NAMUS", "Houston PD"

        public decimal? DonationPercentage { get; set; } // Percentage of proceeds to 241RA

        [Column(TypeName = "decimal(18,2)")]
        public decimal? DonationAmount { get; set; } // Fixed donation amount per item

        // SEO and Marketing
        [StringLength(200)]
        public string? MetaTitle { get; set; }

        [StringLength(500)]
        public string? MetaDescription { get; set; }

        [StringLength(200)]
        public string? Slug { get; set; }

        // System Fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string? CreatedBy { get; set; }

        [StringLength(100)]
        public string? UpdatedBy { get; set; }

        // Navigation Properties
        public virtual ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();

        public virtual ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();

        public virtual ICollection<ProductReview> Reviews { get; set; } = new List<ProductReview>();

        // Computed Properties
        [NotMapped]
        public bool IsInStock => StockQuantity > 0 || AllowBackorders;

        [NotMapped]
        public decimal CurrentPrice => SalePrice ?? Price;

        [NotMapped]
        public bool HasDiscount => SalePrice.HasValue && SalePrice < Price;
    }

    public class ProductVariant
    {
        [Key]
        public int Id { get; set; }

        public int ProductId { get; set; }

        [StringLength(100)]
        public string? Size { get; set; }

        [StringLength(100)]
        public string? Color { get; set; }

        [StringLength(100)]
        public string? Style { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Price { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? SalePrice { get; set; }

        [StringLength(50)]
        public string? SKU { get; set; }

        public int StockQuantity { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public virtual Product Product { get; set; }
    }

    public class ProductImage
    {
        [Key]
        public int Id { get; set; }

        public int ProductId { get; set; }

        [StringLength(500)]
        public string ImageUrl { get; set; }

        [StringLength(100)]
        public string? AltText { get; set; }

        public int SortOrder { get; set; } = 0;

        public bool IsPrimary { get; set; } = false;

        [StringLength(100)]
        public string? ImageType { get; set; } // "Main", "Detail", "Lifestyle", "Size Chart"

        public virtual Product Product { get; set; }
    }

    public class ProductReview
    {
        [Key]
        public int Id { get; set; }

        public int ProductId { get; set; }

        [StringLength(100)]
        public string CustomerName { get; set; }

        [StringLength(500)]
        public string? Comment { get; set; }

        public int Rating { get; set; } // 1-5 stars

        public bool IsVerified { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual Product Product { get; set; }
    }

    // Order and Cart Models
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [StringLength(50)]
        public string OrderNumber { get; set; }

        [StringLength(100)]
        public string CustomerEmail { get; set; }

        [StringLength(100)]
        public string CustomerName { get; set; }

        [StringLength(20)]
        public string? CustomerPhone { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Tax { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Shipping { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DonationAmount { get; set; } = 0;

        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Paid, Shipped, Delivered, Cancelled

        [StringLength(50)]
        public string? PaymentMethod { get; set; }

        [StringLength(100)]
        public string? TrackingNumber { get; set; }

        [StringLength(500)]
        public string? ShippingAddress { get; set; }

        [StringLength(100)]
        public string? ShippingCity { get; set; }

        [StringLength(50)]
        public string? ShippingState { get; set; }

        [StringLength(20)]
        public string? ShippingZipCode { get; set; }

        [StringLength(100)]
        public string? ShippingCountry { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }

    public class OrderItem
    {
        [Key]
        public int Id { get; set; }

        public int OrderId { get; set; }

        public int ProductId { get; set; }

        public int? ProductVariantId { get; set; }

        [StringLength(200)]
        public string ProductName { get; set; }

        [StringLength(100)]
        public string? VariantName { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DonationAmount { get; set; } = 0;

        public virtual Order Order { get; set; }

        public virtual Product Product { get; set; }

        public virtual ProductVariant? Variant { get; set; }
    }

    // Partnership and Collaboration Models
    public class Partnership
    {
        [Key]
        public int Id { get; set; }

        [StringLength(100)]
        public string PartnerName { get; set; } // "Varlo", "NAMUS", "Houston PD"

        [StringLength(200)]
        public string? PartnerWebsite { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? PartnershipType { get; set; } // "Merchandise", "Awareness", "Law Enforcement", "DNA Database"

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        [StringLength(500)]
        public string? LogoUrl { get; set; }

        [StringLength(1000)]
        public string? Terms { get; set; }

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }

    // Campaign and Fundraising Models
    public class Campaign
    {
        [Key]
        public int Id { get; set; }

        [StringLength(200)]
        public string Name { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? CampaignType { get; set; } // "Missing Person", "Awareness", "Fundraising", "DNA Collection"

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? GoalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentAmount { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
} 