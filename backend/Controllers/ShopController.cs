using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using _241RunnersAwareness.BackendAPI.DBContext.Models;
using _241RunnersAwareness.BackendAPI.Services;

namespace _241RunnersAwareness.BackendAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShopController : ControllerBase
    {
        private readonly ILogger<ShopController> _logger;
        private readonly IAnalyticsService _analyticsService;

        public ShopController(ILogger<ShopController> logger, IAnalyticsService analyticsService)
        {
            _logger = logger;
            _analyticsService = analyticsService;
        }

        /// <summary>
        /// Get all products with filtering and pagination
        /// </summary>
        [HttpGet("products")]
        public async Task<IActionResult> GetProducts(
            [FromQuery] string? category = null,
            [FromQuery] string? brand = null,
            [FromQuery] string? collection = null,
            [FromQuery] string? gender = null,
            [FromQuery] bool? onSale = null,
            [FromQuery] bool? featured = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                // TODO: Replace with actual database query
                var mockProducts = GenerateMockProducts();

                // Apply filters
                var filteredProducts = mockProducts.AsQueryable();

                if (!string.IsNullOrEmpty(category))
                    filteredProducts = filteredProducts.Where(p => p.Category == category);

                if (!string.IsNullOrEmpty(brand))
                    filteredProducts = filteredProducts.Where(p => p.Brand == brand);

                if (!string.IsNullOrEmpty(collection))
                    filteredProducts = filteredProducts.Where(p => p.Collection == collection);

                if (!string.IsNullOrEmpty(gender))
                    filteredProducts = filteredProducts.Where(p => p.Gender == gender);

                if (onSale.HasValue)
                    filteredProducts = filteredProducts.Where(p => p.IsOnSale == onSale.Value);

                if (featured.HasValue)
                    filteredProducts = filteredProducts.Where(p => p.IsFeatured == featured.Value);

                if (!string.IsNullOrEmpty(searchTerm))
                    filteredProducts = filteredProducts.Where(p => 
                        p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        p.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));

                // Apply sorting
                if (!string.IsNullOrEmpty(sortBy))
                {
                    filteredProducts = sortBy.ToLower() switch
                    {
                        "price_asc" => filteredProducts.OrderBy(p => p.CurrentPrice),
                        "price_desc" => filteredProducts.OrderByDescending(p => p.CurrentPrice),
                        "name_asc" => filteredProducts.OrderBy(p => p.Name),
                        "name_desc" => filteredProducts.OrderByDescending(p => p.Name),
                        "newest" => filteredProducts.OrderByDescending(p => p.CreatedAt),
                        "popular" => filteredProducts.OrderByDescending(p => p.Reviews.Count),
                        _ => filteredProducts.OrderBy(p => p.Name)
                    };
                }

                // Apply pagination
                var totalCount = filteredProducts.Count();
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                var products = filteredProducts.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                // Track analytics
                await _analyticsService.TrackUserActionAsync("shop_products_viewed", 
                    $"Category: {category}, Brand: {brand}, Page: {page}");

                return Ok(new
                {
                    Products = products,
                    Pagination = new
                    {
                        Page = page,
                        PageSize = pageSize,
                        TotalCount = totalCount,
                        TotalPages = totalPages
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get products");
                return StatusCode(500, new { Error = "Failed to retrieve products" });
            }
        }

        /// <summary>
        /// Get a specific product by ID
        /// </summary>
        [HttpGet("products/{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            try
            {
                // TODO: Replace with actual database query
                var mockProducts = GenerateMockProducts();
                var product = mockProducts.FirstOrDefault(p => p.Id == id);

                if (product == null)
                    return NotFound(new { Error = "Product not found" });

                // Track product view
                await _analyticsService.TrackUserActionAsync("product_viewed", 
                    $"Product: {product.Name}, ID: {id}");

                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to get product {id}");
                return StatusCode(500, new { Error = "Failed to retrieve product" });
            }
        }

        /// <summary>
        /// Get featured products
        /// </summary>
        [HttpGet("products/featured")]
        public async Task<IActionResult> GetFeaturedProducts()
        {
            try
            {
                // TODO: Replace with actual database query
                var mockProducts = GenerateMockProducts();
                var featuredProducts = mockProducts.Where(p => p.IsFeatured).Take(8).ToList();

                return Ok(featuredProducts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get featured products");
                return StatusCode(500, new { Error = "Failed to retrieve featured products" });
            }
        }

        /// <summary>
        /// Get products on sale
        /// </summary>
        [HttpGet("products/sale")]
        public async Task<IActionResult> GetSaleProducts()
        {
            try
            {
                // TODO: Replace with actual database query
                var mockProducts = GenerateMockProducts();
                var saleProducts = mockProducts.Where(p => p.IsOnSale).Take(12).ToList();

                return Ok(saleProducts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get sale products");
                return StatusCode(500, new { Error = "Failed to retrieve sale products" });
            }
        }

        /// <summary>
        /// Get product categories
        /// </summary>
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = new[]
                {
                    new { Name = "Running", Count = 15, Icon = "🏃‍♂️" },
                    new { Name = "Triathlon", Count = 12, Icon = "🏊‍♂️" },
                    new { Name = "Cycling", Count = 8, Icon = "🚴‍♂️" },
                    new { Name = "Everyday", Count = 20, Icon = "👕" },
                    new { Name = "Accessories", Count = 10, Icon = "🎒" },
                    new { Name = "241RA Awareness", Count = 25, Icon = "❤️" }
                };

                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get categories");
                return StatusCode(500, new { Error = "Failed to retrieve categories" });
            }
        }

        /// <summary>
        /// Get product collections
        /// </summary>
        [HttpGet("collections")]
        public async Task<IActionResult> GetCollections()
        {
            try
            {
                var collections = new[]
                {
                    new { 
                        Name = "241RA x Varlo Heritage", 
                        Description = "Premium athletic wear supporting missing persons awareness",
                        ImageUrl = "/images/collections/heritage.jpg",
                        ProductCount = 8
                    },
                    new { 
                        Name = "Mercury Collection", 
                        Description = "High-performance triathlon gear",
                        ImageUrl = "/images/collections/mercury.jpg",
                        ProductCount = 12
                    },
                    new { 
                        Name = "Wildbloom Awareness", 
                        Description = "Everyday wear with mission-driven design",
                        ImageUrl = "/images/collections/wildbloom.jpg",
                        ProductCount = 15
                    },
                    new { 
                        Name = "Law Enforcement Support", 
                        Description = "Gear for those who protect and serve",
                        ImageUrl = "/images/collections/le-support.jpg",
                        ProductCount = 6
                    }
                };

                return Ok(collections);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get collections");
                return StatusCode(500, new { Error = "Failed to retrieve collections" });
            }
        }

        /// <summary>
        /// Create a new order
        /// </summary>
        [HttpPost("orders")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            try
            {
                // TODO: Implement actual order creation logic
                var orderNumber = GenerateOrderNumber();
                var order = new Order
                {
                    OrderNumber = orderNumber,
                    CustomerEmail = request.CustomerEmail,
                    CustomerName = request.CustomerName,
                    CustomerPhone = request.CustomerPhone,
                    Subtotal = request.Items.Sum(i => i.UnitPrice * i.Quantity),
                    Tax = 0, // TODO: Calculate tax
                    Shipping = request.ShippingCost,
                    Total = request.Items.Sum(i => i.UnitPrice * i.Quantity) + request.ShippingCost,
                    DonationAmount = request.Items.Sum(i => i.DonationAmount),
                    Status = "Pending",
                    ShippingAddress = request.ShippingAddress,
                    ShippingCity = request.ShippingCity,
                    ShippingState = request.ShippingState,
                    ShippingZipCode = request.ShippingZipCode,
                    ShippingCountry = request.ShippingCountry ?? "US",
                    CreatedAt = DateTime.UtcNow
                };

                // Track order creation
                await _analyticsService.TrackUserActionAsync("order_created", 
                    $"Order: {orderNumber}, Total: ${order.Total}, Donation: ${order.DonationAmount}");

                return Ok(new
                {
                    Success = true,
                    OrderNumber = orderNumber,
                    Order = order,
                    Message = "Order created successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create order");
                return StatusCode(500, new { Error = "Failed to create order" });
            }
        }

        /// <summary>
        /// Get order by number
        /// </summary>
        [HttpGet("orders/{orderNumber}")]
        public async Task<IActionResult> GetOrder(string orderNumber)
        {
            try
            {
                // TODO: Replace with actual database query
                var mockOrder = GenerateMockOrder(orderNumber);

                if (mockOrder == null)
                    return NotFound(new { Error = "Order not found" });

                return Ok(mockOrder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to get order {orderNumber}");
                return StatusCode(500, new { Error = "Failed to retrieve order" });
            }
        }

        /// <summary>
        /// Get partnerships
        /// </summary>
        [HttpGet("partnerships")]
        public async Task<IActionResult> GetPartnerships()
        {
            try
            {
                var partnerships = new[]
                {
                    new
                    {
                        PartnerName = "Varlo",
                        PartnerWebsite = "https://www.varlo.com/",
                        Description = "Premium athletic apparel collaboration supporting missing persons awareness",
                        PartnershipType = "Merchandise",
                        LogoUrl = "/images/partners/varlo-logo.png",
                        IsActive = true
                    },
                    new
                    {
                        PartnerName = "NAMUS",
                        PartnerWebsite = "https://namus.nij.ojp.gov/",
                        Description = "National Missing and Unidentified Persons System integration",
                        PartnershipType = "DNA Database",
                        LogoUrl = "/images/partners/namus-logo.png",
                        IsActive = true
                    },
                    new
                    {
                        PartnerName = "Houston Police Department",
                        PartnerWebsite = "https://www.houstonpolice.org/",
                        Description = "Local law enforcement collaboration for missing persons cases",
                        PartnershipType = "Law Enforcement",
                        LogoUrl = "/images/partners/hpd-logo.png",
                        IsActive = true
                    }
                };

                return Ok(partnerships);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get partnerships");
                return StatusCode(500, new { Error = "Failed to retrieve partnerships" });
            }
        }



        #region Private Methods

        private List<Product> GenerateMockProducts()
        {
            return new List<Product>
            {
                new Product
                {
                    Id = 1,
                    Name = "241RA x Varlo Heritage Singlet",
                    Description = "Premium triathlon singlet supporting missing persons awareness",
                    LongDescription = "This limited edition singlet features the 241RA mission statement and supports our DNA collection initiatives. Made with Varlo's signature performance fabric.",
                    Price = 89.99m,
                    SalePrice = 69.99m,
                    Brand = "241RA x Varlo",
                    Category = "Triathlon",
                    Subcategory = "Singlets",
                    Collection = "Heritage",
                    Gender = "Unisex",
                    Color = "Navy Blue",
                    IsActive = true,
                    IsFeatured = true,
                    IsOnSale = true,
                    StockQuantity = 50,
                    DonationPercentage = 15,
                    DonationAmount = 10.50m,
                    AwarenessType = "Missing Persons",
                    MissionStatement = "Every purchase supports DNA collection and identification technology",
                    PartnerOrganization = "Varlo",
                    Tags = "triathlon,awareness,missing persons,DNA",
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                },
                new Product
                {
                    Id = 2,
                    Name = "Mercury Tri Shorts",
                    Description = "High-performance triathlon shorts with 241RA awareness",
                    Price = 129.99m,
                    Brand = "Varlo",
                    Category = "Triathlon",
                    Subcategory = "Shorts",
                    Collection = "Mercury",
                    Gender = "Men",
                    Color = "Black",
                    IsActive = true,
                    IsFeatured = true,
                    StockQuantity = 75,
                    DonationPercentage = 10,
                    DonationAmount = 13.00m,
                    Tags = "triathlon,performance,shorts",
                    CreatedAt = DateTime.UtcNow.AddDays(-25)
                },
                new Product
                {
                    Id = 3,
                    Name = "Wildbloom Awareness Tee",
                    Description = "Everyday comfort with mission-driven design",
                    Price = 45.99m,
                    SalePrice = 35.99m,
                    Brand = "241RA x Varlo",
                    Category = "Everyday",
                    Subcategory = "Tees",
                    Collection = "Wildbloom",
                    Gender = "Unisex",
                    Color = "White",
                    IsActive = true,
                    IsOnSale = true,
                    StockQuantity = 100,
                    DonationPercentage = 20,
                    DonationAmount = 7.20m,
                    AwarenessType = "Special Needs",
                    MissionStatement = "Supporting families of missing persons with special needs",
                    Tags = "everyday,awareness,comfort",
                    CreatedAt = DateTime.UtcNow.AddDays(-20)
                },
                new Product
                {
                    Id = 4,
                    Name = "Law Enforcement Support Jersey",
                    Description = "Dedicated to those who protect and serve",
                    Price = 79.99m,
                    Brand = "241RA",
                    Category = "Everyday",
                    Subcategory = "Jerseys",
                    Collection = "Law Enforcement Support",
                    Gender = "Unisex",
                    Color = "Dark Blue",
                    IsActive = true,
                    StockQuantity = 60,
                    DonationPercentage = 25,
                    DonationAmount = 20.00m,
                    AwarenessType = "Law Enforcement",
                    MissionStatement = "Supporting law enforcement in missing persons cases",
                    Tags = "law enforcement,support,jersey",
                    CreatedAt = DateTime.UtcNow.AddDays(-15)
                }
            };
        }

        private Order GenerateMockOrder(string orderNumber)
        {
            return new Order
            {
                Id = 1,
                OrderNumber = orderNumber,
                CustomerEmail = "customer@example.com",
                CustomerName = "John Doe",
                CustomerPhone = "555-123-4567",
                Subtotal = 159.98m,
                Tax = 12.80m,
                Shipping = 8.99m,
                Total = 181.77m,
                DonationAmount = 17.70m,
                Status = "Paid",
                PaymentMethod = "Credit Card",
                ShippingAddress = "123 Main St",
                ShippingCity = "Houston",
                ShippingState = "TX",
                ShippingZipCode = "77001",
                ShippingCountry = "US",
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            };
        }

        private string GenerateOrderNumber()
        {
            return $"241RA-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(1000, 9999)}";
        }

        #endregion
    }

    #region Request Models

    public class CreateOrderRequest
    {
        public string CustomerEmail { get; set; }
        public string CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public List<OrderItemRequest> Items { get; set; } = new List<OrderItemRequest>();
        public decimal ShippingCost { get; set; }
        public string? ShippingAddress { get; set; }
        public string? ShippingCity { get; set; }
        public string? ShippingState { get; set; }
        public string? ShippingZipCode { get; set; }
        public string? ShippingCountry { get; set; }
    }

    public class OrderItemRequest
    {
        public int ProductId { get; set; }
        public int? ProductVariantId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DonationAmount { get; set; }
    }

    #endregion
} 