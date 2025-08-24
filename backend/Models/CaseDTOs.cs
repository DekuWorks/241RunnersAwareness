using System.ComponentModel.DataAnnotations;

namespace _241RunnersAwareness.BackendAPI.Models
{
    // Case DTOs
    public class CaseDto
    {
        public int Id { get; set; }
        public string CaseNumber { get; set; }
        public string PublicSlug { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public string RiskLevel { get; set; }
        public string? LastSeenLocation { get; set; }
        public DateTime? LastSeenDate { get; set; }
        public bool IsPublic { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public string? IndividualName { get; set; }
        public int UpdateCount { get; set; }
        public CaseUpdateDto? LatestUpdate { get; set; }
    }

    public class CaseDetailDto : CaseDto
    {
        public string? LawEnforcementCaseNumber { get; set; }
        public string? InvestigatingAgency { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public UserDto? Owner { get; set; }
        public CaseIndividualDto? Individual { get; set; }
        public List<CaseUpdateDto> Updates { get; set; } = new List<CaseUpdateDto>();
    }

    public class PublicCaseDto
    {
        public int Id { get; set; }
        public string CaseNumber { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string RiskLevel { get; set; }
        public string? LastSeenLocation { get; set; }
        public DateTime? LastSeenDate { get; set; }
        public string? InvestigatingAgency { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public CaseIndividualDto? Individual { get; set; }
        public List<PublicCaseUpdateDto> PublicUpdates { get; set; } = new List<PublicCaseUpdateDto>();
    }

    // Case Update DTOs
    public class CaseUpdateDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string UpdateType { get; set; }
        public bool IsPublic { get; set; }
        public bool IsUrgent { get; set; }
        public string? Location { get; set; }
        public DateTime UpdateDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public UserDto? CreatedBy { get; set; }
    }

    public class PublicCaseUpdateDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string UpdateType { get; set; }
        public bool IsUrgent { get; set; }
        public string? Location { get; set; }
        public DateTime UpdateDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Request DTOs
    public class CreateCaseRequest
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        [StringLength(2000)]
        public string Description { get; set; }

        public string? Status { get; set; } // Open, Closed, Under Investigation, Resolved
        public string? Priority { get; set; } // Low, Medium, High, Critical
        public string? RiskLevel { get; set; } // Low, Medium, High, Critical, Unknown
        public string? LastSeenLocation { get; set; }
        public DateTime? LastSeenDate { get; set; }
        public string? LawEnforcementCaseNumber { get; set; }
        public string? InvestigatingAgency { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public bool? IsPublic { get; set; }
    }

    public class UpdateCaseRequest
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
        public string? Priority { get; set; }
        public string? RiskLevel { get; set; }
        public string? LastSeenLocation { get; set; }
        public DateTime? LastSeenDate { get; set; }
        public string? LawEnforcementCaseNumber { get; set; }
        public string? InvestigatingAgency { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public bool? IsPublic { get; set; }
    }

    public class CreateCaseUpdateRequest
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        [StringLength(5000)]
        public string Content { get; set; }

        public string? UpdateType { get; set; } // General, Status Change, Location Update, Investigation, Media, Other
        public bool? IsPublic { get; set; }
        public bool? IsUrgent { get; set; }
        public string? Location { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public DateTime? UpdateDate { get; set; }
        public bool? RequiresNotification { get; set; }
    }

    // Extended Individual DTO for case management
    public class CaseIndividualDto
    {
        public int IndividualId { get; set; }
        public string FullName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }
    }
}
