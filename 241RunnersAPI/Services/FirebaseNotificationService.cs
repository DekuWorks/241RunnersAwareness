using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;
using _241RunnersAPI.Data;
using _241RunnersAPI.Models;

namespace _241RunnersAPI.Services
{
    /// <summary>
    /// Service for sending Firebase push notifications
    /// </summary>
    public class FirebaseNotificationService : IFirebaseNotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FirebaseNotificationService> _logger;
        private readonly ITopicService _topicService;
        private FirebaseMessaging? _messaging;
        private bool _isInitialized = false;

        public FirebaseNotificationService(
            ApplicationDbContext context,
            ILogger<FirebaseNotificationService> logger,
            ITopicService topicService)
        {
            _context = context;
            _logger = logger;
            _topicService = topicService;
        }

        /// <summary>
        /// Initialize Firebase Admin SDK
        /// </summary>
        public async Task<ServiceResult> InitializeAsync()
        {
            try
            {
                if (_isInitialized)
                {
                    return ServiceResult.CreateSuccess("Firebase already initialized");
                }

                // Get Firebase service account JSON from configuration
                var serviceAccountJson = Environment.GetEnvironmentVariable("FIREBASE_SERVICE_ACCOUNT_JSON");
                if (string.IsNullOrEmpty(serviceAccountJson))
                {
                    return ServiceResult.CreateFailure("Firebase service account JSON not configured");
                }

                // Initialize Firebase App
                if (FirebaseApp.DefaultInstance == null)
                {
                    var credential = GoogleCredential.FromJson(serviceAccountJson);
                    FirebaseApp.Create(new AppOptions
                    {
                        Credential = credential
                    });
                }

                _messaging = FirebaseMessaging.DefaultInstance;
                _isInitialized = true;

                _logger.LogInformation("Firebase Admin SDK initialized successfully");
                return ServiceResult.CreateSuccess("Firebase initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing Firebase Admin SDK");
                return ServiceResult.CreateFailure($"Failed to initialize Firebase: {ex.Message}");
            }
        }

        /// <summary>
        /// Send notification to a specific user
        /// </summary>
        public async Task<ServiceResult> SendNotificationToUserAsync(int userId, CreateNotificationDto notification)
        {
            try
            {
                await EnsureInitializedAsync();

                // Get user's active devices
                var devices = await _context.Devices
                    .Where(d => d.UserId == userId && d.IsActive)
                    .ToListAsync();

                if (!devices.Any())
                {
                    return ServiceResult.CreateFailure("No active devices found for user");
                }

                var results = new List<object>();
                var successCount = 0;
                var failureCount = 0;

                foreach (var device in devices)
                {
                    try
                    {
                        var message = CreateFirebaseMessage(notification, device.FcmToken);
                        var response = await _messaging!.SendAsync(message);
                        
                        // Save notification record
                        await SaveNotificationRecordAsync(userId, notification, response);

                        successCount++;
                        results.Add(new { deviceId = device.Id, platform = device.Platform, success = true, messageId = response });
                    }
                    catch (Exception ex)
                    {
                        failureCount++;
                        results.Add(new { deviceId = device.Id, platform = device.Platform, success = false, error = ex.Message });
                        _logger.LogError(ex, "Error sending notification to device {DeviceId}", device.Id);
                    }
                }

                _logger.LogInformation("Notification sent to user {UserId}: {SuccessCount} success, {FailureCount} failures", 
                    userId, successCount, failureCount);

                return ServiceResult.CreateSuccess(
                    $"Notification sent: {successCount} success, {failureCount} failures", 
                    results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to user {UserId}", userId);
                return ServiceResult.CreateFailure($"Failed to send notification: {ex.Message}");
            }
        }

        /// <summary>
        /// Send notification to multiple users
        /// </summary>
        public async Task<ServiceResult> SendNotificationToUsersAsync(List<int> userIds, CreateNotificationDto notification)
        {
            try
            {
                var results = new List<object>();
                var totalSuccess = 0;
                var totalFailure = 0;

                foreach (var userId in userIds)
                {
                    var result = await SendNotificationToUserAsync(userId, notification);
                    if (result.Success)
                    {
                        totalSuccess++;
                    }
                    else
                    {
                        totalFailure++;
                    }
                    results.Add(new { userId = userId, result = result });
                }

                return ServiceResult.CreateSuccess(
                    $"Bulk notification sent: {totalSuccess} success, {totalFailure} failures", 
                    results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending bulk notification to users");
                return ServiceResult.CreateFailure($"Failed to send bulk notification: {ex.Message}");
            }
        }

        /// <summary>
        /// Send notification to a topic
        /// </summary>
        public async Task<ServiceResult> SendNotificationToTopicAsync(string topic, CreateNotificationDto notification)
        {
            try
            {
                await EnsureInitializedAsync();

                var message = CreateFirebaseMessage(notification, topic: topic);
                var response = await _messaging!.SendAsync(message);

                // Get topic subscribers and save notification records
                var subscribers = await _topicService.GetTopicSubscribersAsync(topic);
                foreach (var userId in subscribers)
                {
                    await SaveNotificationRecordAsync(userId, notification, response);
                    await _topicService.UpdateNotificationCountAsync(userId, topic);
                }

                _logger.LogInformation("Notification sent to topic {Topic} with {SubscriberCount} subscribers", 
                    topic, subscribers.Count);

                return ServiceResult.CreateSuccess(
                    $"Notification sent to topic {topic} with {subscribers.Count} subscribers", 
                    new { topic = topic, subscriberCount = subscribers.Count, messageId = response });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to topic {Topic}", topic);
                return ServiceResult.CreateFailure($"Failed to send notification to topic: {ex.Message}");
            }
        }

        /// <summary>
        /// Send notification to multiple topics
        /// </summary>
        public async Task<ServiceResult> SendNotificationToTopicsAsync(List<string> topics, CreateNotificationDto notification)
        {
            try
            {
                var results = new List<object>();
                var totalSuccess = 0;
                var totalFailure = 0;

                foreach (var topic in topics)
                {
                    var result = await SendNotificationToTopicAsync(topic, notification);
                    if (result.Success)
                    {
                        totalSuccess++;
                    }
                    else
                    {
                        totalFailure++;
                    }
                    results.Add(new { topic = topic, result = result });
                }

                return ServiceResult.CreateSuccess(
                    $"Bulk topic notification sent: {totalSuccess} success, {totalFailure} failures", 
                    results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending bulk notification to topics");
                return ServiceResult.CreateFailure($"Failed to send bulk topic notification: {ex.Message}");
            }
        }

        /// <summary>
        /// Send notification to all users
        /// </summary>
        public async Task<ServiceResult> SendNotificationToAllUsersAsync(CreateNotificationDto notification)
        {
            return await SendNotificationToTopicAsync(Topics.OrgAll, notification);
        }

        /// <summary>
        /// Send notification to admin users
        /// </summary>
        public async Task<ServiceResult> SendNotificationToAdminsAsync(CreateNotificationDto notification)
        {
            return await SendNotificationToTopicAsync(Topics.RoleAdmin, notification);
        }

        /// <summary>
        /// Send case update notification
        /// </summary>
        public async Task<ServiceResult> SendCaseUpdateNotificationAsync(int caseId, object caseData)
        {
            try
            {
                var notification = new CreateNotificationDto
                {
                    Title = "Case Updated",
                    Body = "A case you're following has been updated",
                    Type = "case_updated",
                    Topic = Topics.GetCaseTopic(caseId),
                    Data = new Dictionary<string, object>
                    {
                        ["caseId"] = caseId,
                        ["caseData"] = caseData
                    },
                    RelatedCaseId = caseId,
                    Priority = "normal"
                };

                return await SendNotificationToTopicAsync(Topics.GetCaseTopic(caseId), notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending case update notification for case {CaseId}", caseId);
                return ServiceResult.CreateFailure($"Failed to send case update notification: {ex.Message}");
            }
        }

        /// <summary>
        /// Send new case notification
        /// </summary>
        public async Task<ServiceResult> SendNewCaseNotificationAsync(int caseId, object caseData)
        {
            try
            {
                var notification = new CreateNotificationDto
                {
                    Title = "New Case Reported",
                    Body = "A new case has been reported in your area",
                    Type = "new_case",
                    Topic = Topics.OrgAll,
                    Data = new Dictionary<string, object>
                    {
                        ["caseId"] = caseId,
                        ["caseData"] = caseData
                    },
                    RelatedCaseId = caseId,
                    Priority = "high"
                };

                return await SendNotificationToAllUsersAsync(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending new case notification for case {CaseId}", caseId);
                return ServiceResult.CreateFailure($"Failed to send new case notification: {ex.Message}");
            }
        }

        /// <summary>
        /// Send admin notice notification
        /// </summary>
        public async Task<ServiceResult> SendAdminNoticeNotificationAsync(string message, object? data = null)
        {
            try
            {
                var notification = new CreateNotificationDto
                {
                    Title = "Admin Notice",
                    Body = message,
                    Type = "admin_notice",
                    Topic = Topics.RoleAdmin,
                    Data = data != null ? new Dictionary<string, object> { ["data"] = data } : null,
                    Priority = "high"
                };

                return await SendNotificationToAdminsAsync(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending admin notice notification");
                return ServiceResult.CreateFailure($"Failed to send admin notice notification: {ex.Message}");
            }
        }

        /// <summary>
        /// Test Firebase connection
        /// </summary>
        public async Task<ServiceResult> TestConnectionAsync()
        {
            try
            {
                await EnsureInitializedAsync();

                // Try to send a test message to a non-existent token to test connection
                var testMessage = new Message
                {
                    Token = "test_token",
                    Notification = new FirebaseAdmin.Messaging.Notification
                    {
                        Title = "Test",
                        Body = "Test message"
                    }
                };

                try
                {
                    await _messaging!.SendAsync(testMessage);
                }
                catch (FirebaseMessagingException ex) when (ex.ErrorCode == ErrorCode.InvalidArgument)
                {
                    // This is expected for a test token
                    return ServiceResult.CreateSuccess("Firebase connection test successful");
                }

                return ServiceResult.CreateSuccess("Firebase connection test successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing Firebase connection");
                return ServiceResult.CreateFailure($"Firebase connection test failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Ensure Firebase is initialized
        /// </summary>
        private async Task EnsureInitializedAsync()
        {
            if (!_isInitialized)
            {
                var initResult = await InitializeAsync();
                if (!initResult.Success)
                {
                    throw new InvalidOperationException($"Firebase not initialized: {initResult.Message}");
                }
            }
        }

        /// <summary>
        /// Create Firebase message
        /// </summary>
        private Message CreateFirebaseMessage(CreateNotificationDto notification, string? token = null, string? topic = null)
        {
            var message = new Message
            {
                Notification = new FirebaseAdmin.Messaging.Notification
                {
                    Title = notification.Title,
                    Body = notification.Body
                },
                Data = notification.Data?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.ToString() ?? string.Empty) ?? new Dictionary<string, string>(),
                Android = new AndroidConfig
                {
                    Priority = Priority.High,
                    Notification = new AndroidNotification
                    {
                        Icon = "ic_notification",
                        Color = "#000000",
                        Sound = "default"
                    }
                },
                Apns = new ApnsConfig
                {
                    Aps = new Aps
                    {
                        Alert = new ApsAlert
                        {
                            Title = notification.Title,
                            Body = notification.Body
                        },
                        Badge = 1,
                        Sound = "default"
                    }
                }
            };

            if (!string.IsNullOrEmpty(token))
            {
                message.Token = token;
            }
            else if (!string.IsNullOrEmpty(topic))
            {
                message.Topic = topic;
            }

            return message;
        }

        /// <summary>
        /// Save notification record to database
        /// </summary>
        private async Task SaveNotificationRecordAsync(int userId, CreateNotificationDto notification, string messageId)
        {
            try
            {
                var notificationRecord = new _241RunnersAPI.Models.Notification
                {
                    UserId = userId,
                    Title = notification.Title,
                    Body = notification.Body,
                    Type = notification.Type,
                    Topic = notification.Topic,
                    DataJson = notification.Data != null ? System.Text.Json.JsonSerializer.Serialize(notification.Data) : null,
                    IsSent = true,
                    SentAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    RelatedCaseId = notification.RelatedCaseId,
                    RelatedUserId = notification.RelatedUserId,
                    Priority = notification.Priority,
                    ExpiresAt = notification.ExpiresAt
                };

                _context.Notifications.Add(notificationRecord);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving notification record for user {UserId}", userId);
            }
        }
    }
}
