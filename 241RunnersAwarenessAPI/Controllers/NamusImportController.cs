using _241RunnersAwarenessAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace _241RunnersAwarenessAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "admin")]
    public class NamusImportController : ControllerBase
    {
        private readonly INamusDataService _namusDataService;
        private readonly ILogger<NamusImportController> _logger;

        public NamusImportController(INamusDataService namusDataService, ILogger<NamusImportController> logger)
        {
            _namusDataService = namusDataService;
            _logger = logger;
        }

        /// <summary>
        /// Import NamUs data from CSV file
        /// </summary>
        [HttpPost("csv")]
        public async Task<ActionResult> ImportFromCsv(IFormFile csvFile)
        {
            try
            {
                if (csvFile == null)
                {
                    return BadRequest(new { message = "CSV file is required" });
                }

                var result = await _namusDataService.ImportFromCsvAsync(csvFile);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing NamUs CSV data");
                return StatusCode(500, new { message = "An error occurred while importing NamUs CSV data" });
            }
        }

        /// <summary>
        /// Import NamUs data from JSON
        /// </summary>
        [HttpPost("json")]
        public async Task<ActionResult> ImportFromJson([FromBody] string jsonData)
        {
            try
            {
                if (string.IsNullOrEmpty(jsonData))
                {
                    return BadRequest(new { message = "JSON data is required" });
                }

                var result = await _namusDataService.ImportFromJsonAsync(jsonData);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing NamUs JSON data");
                return StatusCode(500, new { message = "An error occurred while importing NamUs JSON data" });
            }
        }

        /// <summary>
        /// Fetch NamUs data from external source
        /// </summary>
        [HttpPost("fetch")]
        public async Task<ActionResult> FetchNamUsData([FromBody] NamUsFetchRequest request)
        {
            try
            {
                var result = await _namusDataService.FetchNamUsDataAsync(
                    request.State ?? "TX", 
                    request.City ?? "Houston", 
                    request.MaxResults ?? 100);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching NamUs data");
                return StatusCode(500, new { message = "An error occurred while fetching NamUs data" });
            }
        }

        /// <summary>
        /// Sync existing cases with NamUs
        /// </summary>
        [HttpPost("sync")]
        public async Task<ActionResult> SyncExistingCases()
        {
            try
            {
                var result = await _namusDataService.SyncExistingCasesAsync();
                
                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing existing NamUs cases");
                return StatusCode(500, new { message = "An error occurred while syncing cases" });
            }
        }

        /// <summary>
        /// Get NamUs import statistics
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult> GetImportStats()
        {
            try
            {
                var stats = await _namusDataService.GetImportStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting NamUs import stats");
                return StatusCode(500, new { message = "An error occurred while getting import stats" });
            }
        }

        /// <summary>
        /// Get sample CSV template for NamUs data import
        /// </summary>
        [HttpGet("template")]
        public ActionResult GetCsvTemplate()
        {
            try
            {
                var csvContent = "Case Number,Full Name,Sex,Age at Missing,Date Missing,City,County,State,Agency\n" +
                               "MP12345,John Doe,M,25,2024-01-15,Houston,Harris,Texas,Houston Police Department\n" +
                               "MP12346,Jane Smith,F,30,2024-02-20,Houston,Harris,Texas,Harris County Sheriff";

                var bytes = System.Text.Encoding.UTF8.GetBytes(csvContent);
                return File(bytes, "text/csv", "namus_import_template.csv");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating CSV template");
                return StatusCode(500, new { message = "An error occurred while generating CSV template" });
            }
        }
    }

    public class NamUsFetchRequest
    {
        public string? State { get; set; }
        public string? City { get; set; }
        public int? MaxResults { get; set; }
    }
} 