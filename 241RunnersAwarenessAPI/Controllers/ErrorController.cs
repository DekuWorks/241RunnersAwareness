using Microsoft.AspNetCore.Mvc;
using _241RunnersAwarenessAPI.Models;

namespace _241RunnersAwarenessAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ErrorController : ControllerBase
    {
        private readonly ILogger<ErrorController> _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Receive client-side error reports
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> ReportError([FromBody] ClientErrorReport errorReport)
        {
            try
            {
                // Log the error with structured logging
                _logger.LogError("Client Error: {ErrorId} - {Message} - {Severity} - {Url} - {UserAgent} - {Context}",
                    errorReport.Id,
                    errorReport.Message,
                    errorReport.Severity,
                    errorReport.Url,
                    errorReport.UserAgent,
                    errorReport.Context);

                // Log stack trace if available
                if (!string.IsNullOrEmpty(errorReport.Stack))
                {
                    _logger.LogError("Stack Trace: {Stack}", errorReport.Stack);
                }

                // You could also store in database for analysis
                // await _context.ClientErrors.AddAsync(new ClientError { ... });
                // await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Error reported successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process client error report");
                return StatusCode(500, new { success = false, message = "Failed to process error report" });
            }
        }

        /// <summary>
        /// Get error statistics (admin only)
        /// </summary>
        [HttpGet("stats")]
        [Authorize(Roles = "admin")]
        public ActionResult GetErrorStats()
        {
            try
            {
                // This would typically query a database of stored errors
                // For now, return mock data
                var stats = new
                {
                    totalErrors = 0,
                    errorsBySeverity = new
                    {
                        low = 0,
                        medium = 0,
                        high = 0,
                        critical = 0
                    },
                    errorsByType = new
                    {
                        uncaught = 0,
                        unhandledRejection = 0,
                        fetch = 0,
                        other = 0
                    },
                    recentErrors = new object[0]
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get error statistics");
                return StatusCode(500, new { success = false, message = "Failed to get error statistics" });
            }
        }
    }

    /// <summary>
    /// Client error report model
    /// </summary>
    public class ClientErrorReport
    {
        public string Id { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Stack { get; set; }
        public string Url { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public string Severity { get; set; } = "medium";
        public string Timestamp { get; set; } = string.Empty;
        public object? Context { get; set; }
    }
}
