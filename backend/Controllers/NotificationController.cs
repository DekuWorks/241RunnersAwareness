using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using _241RunnersAwareness.BackendAPI.Services;
using _241RunnersAwareness.BackendAPI.DBContext.Models;
using System.Collections.Generic; // Added missing import for List

namespace _241RunnersAwareness.BackendAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly IRealTimeNotificationService _realTimeService;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(
            INotificationService notificationService,
            IRealTimeNotificationService realTimeService,
            ILogger<NotificationController> logger)
        {
            _notificationService = notificationService;
            _realTimeService = realTimeService;
            _logger = logger;
        }

        /// <summary>
        /// Sends URGENT real-time alert for missing person
        /// This is the most critical endpoint - called immediately when a case is reported
        /// </summary>
        [HttpPost("urgent-alert")]
        [Authorize(Roles = "Admin,Caregiver,Parent")]
        public async Task<IActionResult> SendUrgentAlert([FromBody] UrgentAlertRequest request)
        {
            try
            {
                _logger.LogWarning($"🚨 URGENT ALERT REQUESTED: Case {request.CaseId} - {request.IndividualName}");

                // Send real-time alert to all connected clients
                await _realTimeService.SendUrgentAlertToAllAsync(
                    request.CaseId,
                    request.IndividualName,
                    request.Location,
                    request.Description
                );

                // Send email/SMS notifications
                await _notificationService.SendUrgentAlertAsync(
                    request.CaseId,
                    request.IndividualName,
                    request.Location,
                    request.Description
                );

                // Send specific alerts to law enforcement
                await _realTimeService.SendAlertToLawEnforcementAsync(
                    request.CaseId,
                    request.IndividualName,
                    request.Location,
                    request.SpecialNeeds
                );

                // Send alerts to emergency contacts
                if (!string.IsNullOrEmpty(request.EmergencyContactInfo))
                {
                    await _realTimeService.SendAlertToEmergencyContactsAsync(
                        request.CaseId,
                        request.IndividualName,
                        request.EmergencyContactInfo
                    );
                }

                return Ok(new
                {
                    Success = true,
                    Message = "Urgent alert sent successfully",
                    CaseId = request.CaseId,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send urgent alert for case {request.CaseId}");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Failed to send urgent alert",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Sends law enforcement specific alert
        /// </summary>
        [HttpPost("law-enforcement-alert")]
        [Authorize(Roles = "Admin,Caregiver")]
        public async Task<IActionResult> SendLawEnforcementAlert([FromBody] LawEnforcementAlertRequest request)
        {
            try
            {
                _logger.LogInformation($"Law enforcement alert requested for case {request.CaseId}");

                await _realTimeService.SendAlertToLawEnforcementAsync(
                    request.CaseId,
                    request.IndividualName,
                    request.Location,
                    request.SpecialNeeds
                );

                await _notificationService.SendLawEnforcementAlertAsync(
                    request.CaseId,
                    request.IndividualName,
                    request.Location,
                    request.SpecialNeeds
                );

                return Ok(new
                {
                    Success = true,
                    Message = "Law enforcement alert sent successfully",
                    CaseId = request.CaseId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send law enforcement alert for case {request.CaseId}");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Failed to send law enforcement alert",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Sends emergency contact alert
        /// </summary>
        [HttpPost("emergency-contact-alert")]
        [Authorize(Roles = "Admin,Caregiver,Parent")]
        public async Task<IActionResult> SendEmergencyContactAlert([FromBody] EmergencyContactAlertRequest request)
        {
            try
            {
                _logger.LogInformation($"Emergency contact alert requested for case {request.CaseId}");

                await _realTimeService.SendAlertToEmergencyContactsAsync(
                    request.CaseId,
                    request.IndividualName,
                    request.ContactInfo
                );

                await _notificationService.SendEmergencyContactAlertAsync(
                    request.ContactEmail,
                    request.ContactPhone,
                    request.IndividualName,
                    request.CaseId
                );

                return Ok(new
                {
                    Success = true,
                    Message = "Emergency contact alert sent successfully",
                    CaseId = request.CaseId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send emergency contact alert for case {request.CaseId}");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Failed to send emergency contact alert",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Sends media alert for public awareness
        /// </summary>
        [HttpPost("media-alert")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SendMediaAlert([FromBody] MediaAlertRequest request)
        {
            try
            {
                _logger.LogInformation($"Media alert requested for case {request.CaseId}");

                await _notificationService.SendMediaAlertAsync(
                    request.CaseId,
                    request.IndividualName,
                    request.Location,
                    request.Description
                );

                return Ok(new
                {
                    Success = true,
                    Message = "Media alert sent successfully",
                    CaseId = request.CaseId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send media alert for case {request.CaseId}");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Failed to send media alert",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Sends case status update notification
        /// </summary>
        [HttpPost("case-update")]
        [Authorize(Roles = "Admin,Caregiver,Parent")]
        public async Task<IActionResult> SendCaseUpdate([FromBody] CaseUpdateRequest request)
        {
            try
            {
                _logger.LogInformation($"Case update notification requested for case {request.CaseId}");

                await _realTimeService.SendCaseUpdateAsync(
                    request.CaseId,
                    request.Status,
                    request.UpdatedBy
                );

                await _notificationService.SendCaseUpdateNotificationAsync(
                    request.CaseId,
                    request.Status,
                    request.UpdatedBy
                );

                return Ok(new
                {
                    Success = true,
                    Message = "Case update notification sent successfully",
                    CaseId = request.CaseId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send case update notification for case {request.CaseId}");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Failed to send case update notification",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Sends notification when person is found
        /// </summary>
        [HttpPost("found-notification")]
        [Authorize(Roles = "Admin,Caregiver,Parent")]
        public async Task<IActionResult> SendFoundNotification([FromBody] FoundNotificationRequest request)
        {
            try
            {
                _logger.LogInformation($"Found notification requested for case {request.CaseId}");

                await _realTimeService.SendFoundNotificationAsync(
                    request.CaseId,
                    request.IndividualName,
                    request.FoundLocation
                );

                await _notificationService.SendFoundNotificationAsync(
                    request.CaseId,
                    request.IndividualName,
                    request.FoundLocation
                );

                return Ok(new
                {
                    Success = true,
                    Message = "Found notification sent successfully",
                    CaseId = request.CaseId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send found notification for case {request.CaseId}");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Failed to send found notification",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Sends daily digest of all active cases
        /// </summary>
        [HttpPost("daily-digest")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SendDailyDigest([FromBody] DailyDigestRequest request)
        {
            try
            {
                _logger.LogInformation("Daily digest requested");

                await _notificationService.SendDailyDigestAsync(request.CaseIds);

                return Ok(new
                {
                    Success = true,
                    Message = "Daily digest sent successfully",
                    CaseCount = request.CaseIds.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send daily digest");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Failed to send daily digest",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Tests the notification system
        /// </summary>
        [HttpPost("test")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> TestNotification([FromBody] TestNotificationRequest request)
        {
            try
            {
                _logger.LogInformation("Test notification requested");

                await _realTimeService.SendUrgentAlertToAllAsync(
                    "TEST-001",
                    "Test Individual",
                    "Test Location",
                    "This is a test notification"
                );

                return Ok(new
                {
                    Success = true,
                    Message = "Test notification sent successfully",
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send test notification");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Failed to send test notification",
                    Error = ex.Message
                });
            }
        }
    }

    #region Request Models

    public class UrgentAlertRequest
    {
        public string CaseId { get; set; }
        public string IndividualName { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public string SpecialNeeds { get; set; }
        public string EmergencyContactInfo { get; set; }
    }

    public class LawEnforcementAlertRequest
    {
        public string CaseId { get; set; }
        public string IndividualName { get; set; }
        public string Location { get; set; }
        public string SpecialNeeds { get; set; }
    }

    public class EmergencyContactAlertRequest
    {
        public string CaseId { get; set; }
        public string IndividualName { get; set; }
        public string ContactInfo { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
    }

    public class MediaAlertRequest
    {
        public string CaseId { get; set; }
        public string IndividualName { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
    }

    public class CaseUpdateRequest
    {
        public string CaseId { get; set; }
        public string Status { get; set; }
        public string UpdatedBy { get; set; }
    }

    public class FoundNotificationRequest
    {
        public string CaseId { get; set; }
        public string IndividualName { get; set; }
        public string FoundLocation { get; set; }
    }

    public class DailyDigestRequest
    {
        public List<string> CaseIds { get; set; }
    }

    public class TestNotificationRequest
    {
        public string Message { get; set; }
    }

    #endregion
} 