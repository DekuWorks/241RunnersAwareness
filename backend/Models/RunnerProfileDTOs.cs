using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace _241RunnersAwareness.BackendAPI.Models
{
    // Individual DTOs
    public class RunnerProfileDto
    {
        public int Id { get; set; }
        public string? RunnerId { get; set; }
        public string FullName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? MiddleName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public int Age { get; set; }
        public string? Gender { get; set; }
        public string Status { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public DateTime? LastSeenDate { get; set; }
        public string? LastSeenLocation { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int CasesCount { get; set; }
        public PhotoDto? PrimaryPhoto { get; set; }
        public Guid? OwnerUserId { get; set; }
        public string? OwnerName { get; set; }
    }

    public class IndividualDetailDto : RunnerProfileDto
    {
        public string? Address { get; set; }
        public string? ZipCode { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Height { get; set; }
        public string? Weight { get; set; }
        public string? HairColor { get; set; }
        public string? EyeColor { get; set; }
        public string? DistinguishingFeatures { get; set; }
        public string? PrimaryDisability { get; set; }
        public string? DisabilityDescription { get; set; }
        public string? MedicalConditions { get; set; }
        public string? Medications { get; set; }
        public string? Allergies { get; set; }
        public string? EmergencyResponseInstructions { get; set; }
        public string? PreferredEmergencyContact { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public List<PhotoDto> Photos { get; set; } = new List<PhotoDto>();
        public List<ActivityDto> RecentActivities { get; set; } = new List<ActivityDto>();
        public List<CaseSummaryDto> Cases { get; set; } = new List<CaseSummaryDto>();
    }

    public class CreateIndividualRequest
    {
        [Required]
        public string FirstName { get; set; }
        
        [Required]
        public string LastName { get; set; }
        
        public string? MiddleName { get; set; }
        
        [Required]
        public DateTime DateOfBirth { get; set; }
        
        public string? Gender { get; set; }
        public string? RunnerId { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? PrimaryDisability { get; set; }
        public string? DisabilityDescription { get; set; }
        public string? MedicalConditions { get; set; }
        public string? EmergencyResponseInstructions { get; set; }
        public string? PreferredEmergencyContact { get; set; }
    }

    public class UpdateIndividualRequest
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? MiddleName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? RunnerId { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Height { get; set; }
        public string? Weight { get; set; }
        public string? HairColor { get; set; }
        public string? EyeColor { get; set; }
        public string? DistinguishingFeatures { get; set; }
        public string? PrimaryDisability { get; set; }
        public string? DisabilityDescription { get; set; }
        public string? MedicalConditions { get; set; }
        public string? Medications { get; set; }
        public string? Allergies { get; set; }
        public string? EmergencyResponseInstructions { get; set; }
        public string? PreferredEmergencyContact { get; set; }
        public string? Status { get; set; }
        public DateTime? LastSeenDate { get; set; }
        public string? LastSeenLocation { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

    // Photo DTOs
    public class PhotoDto
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public string? Caption { get; set; }
        public string? ImageType { get; set; }
        public bool IsPrimary { get; set; }
        public DateTime UploadedAt { get; set; }
        public string? UploadedBy { get; set; }
        public string? FileName { get; set; }
        public long? FileSize { get; set; }
    }

    public class UploadPhotoRequest
    {
        [Required]
        public string ImageUrl { get; set; }
        
        public string? Caption { get; set; }
        public string? ImageType { get; set; }
        public bool SetAsPrimary { get; set; }
        public string? FileName { get; set; }
        public long? FileSize { get; set; }
        public string? ContentType { get; set; }
    }

    public class UpdatePhotoRequest
    {
        public string? Caption { get; set; }
        public string? ImageType { get; set; }
        public bool? SetAsPrimary { get; set; }
    }

    // Activity DTOs
    public class ActivityDto
    {
        public int Id { get; set; }
        public string ActivityType { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string? Location { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public int? RelatedCaseId { get; set; }
        public int? RelatedPhotoId { get; set; }
    }

    // Case DTOs
    public class CaseSummaryDto
    {
        public int Id { get; set; }
        public string CaseNumber { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public string RiskLevel { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public int UpdateCount { get; set; }
    }

    // Search and Filter DTOs
    public class IndividualSearchRequest
    {
        public string? Q { get; set; } // Search query for name
        public string? RunnerId { get; set; }
        public string? Status { get; set; } // Missing, Found, Urgent, Resolved
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; } // name, createdAt, lastSeenDate
        public string? SortOrder { get; set; } // asc, desc
    }

    public class IndividualSearchResponse
    {
        public List<RunnerProfileDto> Individuals { get; set; } = new List<RunnerProfileDto>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    // API Response DTOs
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }

    public class PaginatedResponse<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}
