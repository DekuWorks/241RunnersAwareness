using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using _241RunnersAwareness.BackendAPI.Services;

namespace _241RunnersAwareness.BackendAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DNAController : ControllerBase
    {
        private readonly ILogger<DNAController> _logger;
        private readonly IDNAService _dnaService;

        public DNAController(ILogger<DNAController> logger, IDNAService dnaService)
        {
            _logger = logger;
            _dnaService = dnaService;
        }

        /// <summary>
        /// Store DNA sample for an individual
        /// </summary>
        [HttpPost("store")]
        [Authorize(Roles = "Admin,LawEnforcement")]
        public async Task<IActionResult> StoreDNASample([FromBody] StoreDNASampleRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var success = await _dnaService.StoreDNASampleAsync(
                    request.IndividualId,
                    request.DNASequence,
                    request.SampleType,
                    request.LabReference);

                if (success)
                {
                    return Ok(new
                    {
                        Success = true,
                        Message = "DNA sample stored successfully",
                        IndividualId = request.IndividualId,
                        SampleType = request.SampleType,
                        LabReference = request.LabReference,
                        StoredAt = DateTime.UtcNow
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Failed to store DNA sample. Please verify the DNA sequence format."
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to store DNA sample for individual {request.IndividualId}");
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }

        /// <summary>
        /// Get DNA sample for an individual
        /// </summary>
        [HttpGet("sample/{individualId}")]
        [Authorize(Roles = "Admin,LawEnforcement")]
        public async Task<IActionResult> GetDNASample(int individualId)
        {
            try
            {
                var dnaSample = await _dnaService.GetDNASampleAsync(individualId);

                if (dnaSample == null)
                    return NotFound(new { Error = "No DNA sample found for this individual" });

                return Ok(new
                {
                    IndividualId = individualId,
                    DNASequence = dnaSample,
                    RetrievedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to retrieve DNA sample for individual {individualId}");
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }

        /// <summary>
        /// Compare two DNA samples
        /// </summary>
        [HttpPost("compare")]
        [Authorize(Roles = "Admin,LawEnforcement")]
        public async Task<IActionResult> CompareDNASamples([FromBody] CompareDNASamplesRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var isMatch = await _dnaService.CompareDNASamplesAsync(
                    request.Sample1,
                    request.Sample2);

                return Ok(new
                {
                    IsMatch = isMatch,
                    Sample1Length = request.Sample1.Length,
                    Sample2Length = request.Sample2.Length,
                    ComparedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to compare DNA samples");
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }

        /// <summary>
        /// Search for individuals by DNA sequence
        /// </summary>
        [HttpPost("search")]
        [Authorize(Roles = "Admin,LawEnforcement")]
        public async Task<IActionResult> SearchByDNA([FromBody] SearchByDNARequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var matches = await _dnaService.SearchByDNAAsync(request.DNASequence);

                return Ok(new
                {
                    DNASequence = request.DNASequence,
                    SequenceLength = request.DNASequence.Length,
                    MatchesFound = matches.Count,
                    Matches = matches,
                    SearchedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to search by DNA");
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }

        /// <summary>
        /// Validate DNA sequence format
        /// </summary>
        [HttpPost("validate")]
        public async Task<IActionResult> ValidateDNASequence([FromBody] ValidateDNARequest request)
        {
            try
            {
                var isValid = await _dnaService.ValidateDNASequenceAsync(request.DNASequence);

                return Ok(new
                {
                    DNASequence = request.DNASequence,
                    IsValid = isValid,
                    SequenceLength = request.DNASequence.Length,
                    ValidatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate DNA sequence");
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }

        /// <summary>
        /// Generate DNA report for an individual
        /// </summary>
        [HttpGet("report/{individualId}")]
        [Authorize(Roles = "Admin,LawEnforcement")]
        public async Task<IActionResult> GenerateDNAReport(int individualId)
        {
            try
            {
                var report = await _dnaService.GenerateDNAReportAsync(individualId);

                return Ok(new
                {
                    IndividualId = individualId,
                    Report = report,
                    GeneratedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to generate DNA report for individual {individualId}");
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }

        /// <summary>
        /// Export DNA data to NAMUS
        /// </summary>
        [HttpPost("export/namus/{individualId}")]
        [Authorize(Roles = "Admin,LawEnforcement")]
        public async Task<IActionResult> ExportToNAMUS(int individualId)
        {
            try
            {
                var success = await _dnaService.ExportToNAMUSAsync(individualId);

                if (success)
                {
                    return Ok(new
                    {
                        Success = true,
                        Message = "DNA data exported to NAMUS successfully",
                        IndividualId = individualId,
                        ExportedAt = DateTime.UtcNow
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Failed to export DNA data to NAMUS"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to export DNA to NAMUS for individual {individualId}");
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }

        /// <summary>
        /// Export DNA data to CODIS
        /// </summary>
        [HttpPost("export/codis/{individualId}")]
        [Authorize(Roles = "Admin,LawEnforcement")]
        public async Task<IActionResult> ExportToCODIS(int individualId)
        {
            try
            {
                var success = await _dnaService.ExportToCODISAsync(individualId);

                if (success)
                {
                    return Ok(new
                    {
                        Success = true,
                        Message = "DNA data exported to CODIS successfully",
                        IndividualId = individualId,
                        ExportedAt = DateTime.UtcNow
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Failed to export DNA data to CODIS"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to export DNA to CODIS for individual {individualId}");
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }

        /// <summary>
        /// Get DNA lab partners
        /// </summary>
        [HttpGet("labs")]
        public async Task<IActionResult> GetDNALabPartners()
        {
            try
            {
                var labs = await _dnaService.GetDNALabPartnersAsync();

                return Ok(new
                {
                    LabPartners = labs,
                    RetrievedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get DNA lab partners");
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }

        /// <summary>
        /// Get DNA statistics
        /// </summary>
        [HttpGet("statistics")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetDNAStatistics()
        {
            try
            {
                var statistics = await _dnaService.GetDNAStatisticsAsync();

                return Ok(new
                {
                    Statistics = statistics,
                    RetrievedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get DNA statistics");
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }
    }

    #region Request Models

    public class StoreDNASampleRequest
    {
        public int IndividualId { get; set; }
        public string DNASequence { get; set; }
        public string SampleType { get; set; }
        public string LabReference { get; set; }
    }

    public class CompareDNASamplesRequest
    {
        public string Sample1 { get; set; }
        public string Sample2 { get; set; }
    }

    public class SearchByDNARequest
    {
        public string DNASequence { get; set; }
    }

    public class ValidateDNARequest
    {
        public string DNASequence { get; set; }
    }

    #endregion
} 