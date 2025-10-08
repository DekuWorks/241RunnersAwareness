using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using _241RunnersAPI.Services;

namespace _241RunnersAPI.Hubs
{
    /// <summary>
    /// SignalR Hub for real-time alerts and notifications
    /// This hub matches the mobile app's expectations for real-time updates
    /// </summary>
    [Authorize]
    public class AlertsHub : Hub
    {
        private readonly ILogger<AlertsHub> _logger;
        private readonly IFirebaseNotificationService _notificationService;
        private readonly ITopicService _topicService;
        private static readonly Dictionary<string, ConnectionInfo> _connections = new();
        private static readonly object _lockObject = new();

        public AlertsHub(
            ILogger<AlertsHub> logger,
            IFirebaseNotificationService notificationService,
            ITopicService topicService)
        {
            _logger = logger;
            _notificationService = notificationService;
            _topicService = topicService;
        }

        public override async Task OnConnectedAsync()
        {
            try
            {
                var userId = GetUserId();
                var userEmail = GetUserEmail();
                var userName = GetUserName();
                var userRole = GetUserRole();

                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userEmail))
                {
                    _logger.LogWarning("Connection attempt without valid authentication");
                    Context.Abort();
                    return;
                }

                var connectionId = Context.ConnectionId;
                
                lock (_lockObject)
                {
                    _connections[connectionId] = new ConnectionInfo
                    {
                        ConnectionId = connectionId,
                        UserId = userId,
                        UserEmail = userEmail,
                        UserName = userName,
                        UserRole = userRole,
                        ConnectedAt = DateTime.UtcNow,
                        LastActivity = DateTime.UtcNow
                    };
                }

                // Join user-specific group for targeted notifications
                await Groups.AddToGroupAsync(connectionId, $"user:{userId}");
                
                // Join role-based groups
                if (userRole == "admin")
                {
                    await Groups.AddToGroupAsync(connectionId, "role:admin");
                }
                else
                {
                    await Groups.AddToGroupAsync(connectionId, "role:user");
                }

                // Join topic-based groups based on user subscriptions
                var subscriptions = await _topicService.GetUserSubscriptionsAsync(int.Parse(userId));
                foreach (var subscription in subscriptions.Where(s => s.IsSubscribed))
                {
                    await Groups.AddToGroupAsync(connectionId, $"topic:{subscription.Topic}");
                }

                _logger.LogInformation("User {UserName} ({UserEmail}) with role {UserRole} connected with connection ID {ConnectionId}", 
                    userName, userEmail, userRole, connectionId);

                // Send welcome message
                await Clients.Caller.SendAsync("Welcome", new
                {
                    Message = "Connected to real-time alerts",
                    UserId = userId,
                    UserName = userName,
                    UserRole = userRole,
                    ConnectedAt = DateTime.UtcNow,
                    Subscriptions = subscriptions.Count
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
                ConnectionInfo? connectionInfo = null;

                lock (_lockObject)
                {
                    if (_connections.TryGetValue(connectionId, out connectionInfo))
                    {
                        _connections.Remove(connectionId);
                    }
                }

                if (connectionInfo != null)
                {
                    _logger.LogInformation("User {UserName} ({UserEmail}) with role {UserRole} disconnected", 
                        connectionInfo.UserName, connectionInfo.UserEmail, connectionInfo.UserRole);
                }

                await base.OnDisconnectedAsync(exception);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnDisconnectedAsync");
            }
        }

        /// <summary>
        /// Join a group (for topic subscriptions)
        /// </summary>
        public async Task JoinGroup(string groupName)
        {
            try
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
                _logger.LogInformation("User {UserId} joined group {GroupName}", GetUserId(), groupName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining group {GroupName}", groupName);
                throw;
            }
        }

        /// <summary>
        /// Leave a group (for topic unsubscriptions)
        /// </summary>
        public async Task LeaveGroup(string groupName)
        {
            try
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
                _logger.LogInformation("User {UserId} left group {GroupName}", GetUserId(), groupName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error leaving group {GroupName}", groupName);
                throw;
            }
        }

        /// <summary>
        /// Ping to keep connection alive
        /// </summary>
        public async Task Ping()
        {
            try
            {
                var connectionId = Context.ConnectionId;
                
                lock (_lockObject)
                {
                    if (_connections.TryGetValue(connectionId, out var connectionInfo))
                    {
                        connectionInfo.LastActivity = DateTime.UtcNow;
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

        private string? GetUserRole()
        {
            return Context.User?.FindFirst(ClaimTypes.Role)?.Value ?? "user";
        }

        #endregion
    }

}
