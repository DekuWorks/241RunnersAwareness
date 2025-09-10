using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace _241RunnersAPI.Hubs
{
    /// <summary>
    /// SignalR Hub for real-time user notifications and updates
    /// Handles user-specific alerts, case updates, and system notifications
    /// </summary>
    [Authorize]
    public class UserHub : Hub
    {
        private readonly ILogger<UserHub> _logger;
        private static readonly Dictionary<string, UserConnection> _userConnections = new();
        private static readonly object _lockObject = new();

        public UserHub(ILogger<UserHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            try
            {
                var userId = GetUserId();
                var userEmail = GetUserEmail();
                var userName = GetUserName();

                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userEmail))
                {
                    _logger.LogWarning("User connection attempt without valid authentication");
                    Context.Abort();
                    return;
                }

                var connectionId = Context.ConnectionId;
                
                lock (_lockObject)
                {
                    _userConnections[connectionId] = new UserConnection
                    {
                        ConnectionId = connectionId,
                        UserId = userId,
                        UserEmail = userEmail,
                        UserName = userName,
                        ConnectedAt = DateTime.UtcNow,
                        LastActivity = DateTime.UtcNow
                    };
                }

                // Join user-specific group for targeted notifications
                await Groups.AddToGroupAsync(connectionId, $"User_{userId}");
                
                // Join general user group for broadcast messages
                await Groups.AddToGroupAsync(connectionId, "Users");

                _logger.LogInformation("User {UserName} ({UserEmail}) connected with connection ID {ConnectionId}", 
                    userName, userEmail, connectionId);

                // Send welcome message with current status
                await Clients.Caller.SendAsync("Welcome", new
                {
                    Message = "Connected to real-time notifications",
                    UserId = userId,
                    UserName = userName,
                    ConnectedAt = DateTime.UtcNow
                });

                await base.OnConnectedAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnConnectedAsync");
                throw;
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            try
            {
                var connectionId = Context.ConnectionId;
                UserConnection? userConnection = null;

                lock (_lockObject)
                {
                    if (_userConnections.TryGetValue(connectionId, out userConnection))
                    {
                        _userConnections.Remove(connectionId);
                    }
                }

                if (userConnection != null)
                {
                    _logger.LogInformation("User {UserName} ({UserEmail}) disconnected", 
                        userConnection.UserName, userConnection.UserEmail);
                }

                await base.OnDisconnectedAsync(exception);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnDisconnectedAsync");
            }
        }

        /// <summary>
        /// Send case status update to specific user
        /// </summary>
        public async Task SendCaseUpdate(string userId, object caseData)
        {
            try
            {
                var updateData = new
                {
                    Type = "case_update",
                    CaseData = caseData,
                    Timestamp = DateTime.UtcNow,
                    UpdateId = Guid.NewGuid().ToString()
                };

                await Clients.Group($"User_{userId}").SendAsync("CaseUpdate", updateData);

                _logger.LogInformation("Case update sent to user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending case update to user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Send emergency alert to specific user
        /// </summary>
        public async Task SendEmergencyAlert(string userId, object alertData)
        {
            try
            {
                var alert = new
                {
                    Type = "emergency_alert",
                    AlertData = alertData,
                    Timestamp = DateTime.UtcNow,
                    AlertId = Guid.NewGuid().ToString(),
                    Priority = "high"
                };

                await Clients.Group($"User_{userId}").SendAsync("EmergencyAlert", alert);

                _logger.LogInformation("Emergency alert sent to user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending emergency alert to user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Send profile update notification to user
        /// </summary>
        public async Task SendProfileUpdate(string userId, object profileData)
        {
            try
            {
                var update = new
                {
                    Type = "profile_update",
                    ProfileData = profileData,
                    Timestamp = DateTime.UtcNow,
                    UpdateId = Guid.NewGuid().ToString()
                };

                await Clients.Group($"User_{userId}").SendAsync("ProfileUpdate", update);

                _logger.LogInformation("Profile update sent to user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending profile update to user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Send system notification to all users
        /// </summary>
        public async Task SendSystemNotification(object notificationData)
        {
            try
            {
                var notification = new
                {
                    Type = "system_notification",
                    NotificationData = notificationData,
                    Timestamp = DateTime.UtcNow,
                    NotificationId = Guid.NewGuid().ToString()
                };

                await Clients.Group("Users").SendAsync("SystemNotification", notification);

                _logger.LogInformation("System notification sent to all users");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending system notification");
                throw;
            }
        }

        /// <summary>
        /// Send runner safety check reminder
        /// </summary>
        public async Task SendSafetyCheckReminder(string userId, object reminderData)
        {
            try
            {
                var reminder = new
                {
                    Type = "safety_check_reminder",
                    ReminderData = reminderData,
                    Timestamp = DateTime.UtcNow,
                    ReminderId = Guid.NewGuid().ToString(),
                    Priority = "medium"
                };

                await Clients.Group($"User_{userId}").SendAsync("SafetyCheckReminder", reminder);

                _logger.LogInformation("Safety check reminder sent to user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending safety check reminder to user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Send weather alert to users in specific area
        /// </summary>
        public async Task SendWeatherAlert(string area, object weatherData)
        {
            try
            {
                var alert = new
                {
                    Type = "weather_alert",
                    Area = area,
                    WeatherData = weatherData,
                    Timestamp = DateTime.UtcNow,
                    AlertId = Guid.NewGuid().ToString(),
                    Priority = "medium"
                };

                // Send to all users (in a real implementation, you'd filter by location)
                await Clients.Group("Users").SendAsync("WeatherAlert", alert);

                _logger.LogInformation("Weather alert sent for area {Area}", area);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending weather alert for area {Area}", area);
                throw;
            }
        }

        /// <summary>
        /// Ping to keep connection alive and update last activity
        /// </summary>
        public async Task Ping()
        {
            try
            {
                var connectionId = Context.ConnectionId;
                
                lock (_lockObject)
                {
                    if (_userConnections.TryGetValue(connectionId, out var userConnection))
                    {
                        userConnection.LastActivity = DateTime.UtcNow;
                    }
                }

                await Clients.Caller.SendAsync("Pong", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ping");
                throw;
            }
        }

        #region Helper Methods

        private string? GetUserId()
        {
            return Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        private string? GetUserEmail()
        {
            return Context.User?.FindFirst(ClaimTypes.Email)?.Value;
        }

        private string? GetUserName()
        {
            var firstName = Context.User?.FindFirst("firstName")?.Value;
            var lastName = Context.User?.FindFirst("lastName")?.Value;
            
            if (!string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName))
            {
                return $"{firstName} {lastName}";
            }
            
            return Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "User";
        }

        private List<object> GetCurrentUsers()
        {
            lock (_lockObject)
            {
                return _userConnections.Values
                    .Where(conn => conn.LastActivity > DateTime.UtcNow.AddMinutes(-5)) // Only active connections
                    .Select(conn => new
                    {
                        UserId = conn.UserId,
                        UserName = conn.UserName,
                        UserEmail = conn.UserEmail,
                        ConnectedAt = conn.ConnectedAt,
                        LastActivity = conn.LastActivity,
                        IsOnline = true
                    })
                    .Cast<object>()
                    .ToList();
            }
        }

        #endregion
    }

    /// <summary>
    /// Represents a user connection
    /// </summary>
    public class UserConnection
    {
        public string ConnectionId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime ConnectedAt { get; set; }
        public DateTime LastActivity { get; set; }
    }
}
