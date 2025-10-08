using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace _241RunnersAPI.Hubs
{
    /// <summary>
    /// SignalR Hub for real-time notifications
    /// Handles both user and admin notifications
    /// </summary>
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;
        private static readonly Dictionary<string, ConnectionInfo> _connections = new();
        private static readonly object _lockObject = new();

        public NotificationHub(ILogger<NotificationHub> logger)
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
                await Groups.AddToGroupAsync(connectionId, $"User_{userId}");
                
                // Join role-based groups
                if (userRole == "admin")
                {
                    await Groups.AddToGroupAsync(connectionId, "Admins");
                }
                else
                {
                    await Groups.AddToGroupAsync(connectionId, "Users");
                }

                _logger.LogInformation("User {UserName} ({UserEmail}) with role {UserRole} connected with connection ID {ConnectionId}", 
                    userName, userEmail, userRole, connectionId);

                // Send welcome message
                await Clients.Caller.SendAsync("Welcome", new
                {
                    Message = "Connected to real-time notifications",
                    UserId = userId,
                    UserName = userName,
                    UserRole = userRole,
                    ConnectedAt = DateTime.UtcNow
                });

                // If admin, notify other admins
                if (userRole == "admin")
                {
                    await Clients.OthersInGroup("Admins").SendAsync("AdminConnected", new
                    {
                        AdminId = userId,
                        AdminName = userName,
                        AdminEmail = userEmail,
                        ConnectedAt = DateTime.UtcNow,
                        ConnectionId = connectionId
                    });
                }

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

                    // If admin, notify other admins
                    if (connectionInfo.UserRole == "admin")
                    {
                        await Clients.OthersInGroup("Admins").SendAsync("AdminDisconnected", new
                        {
                            AdminId = connectionInfo.UserId,
                            AdminName = connectionInfo.UserName,
                            AdminEmail = connectionInfo.UserEmail,
                            DisconnectedAt = DateTime.UtcNow,
                            ConnectionId = connectionId
                        });
                    }
                }

                await base.OnDisconnectedAsync(exception);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnDisconnectedAsync");
            }
        }

        /// <summary>
        /// Send notification to specific user
        /// </summary>
        public async Task SendNotification(string userId, string type, object data)
        {
            try
            {
                var notification = new
                {
                    Type = type,
                    Data = data,
                    Timestamp = DateTime.UtcNow,
                    NotificationId = Guid.NewGuid().ToString()
                };

                await Clients.Group($"User_{userId}").SendAsync("Notification", notification);

                _logger.LogInformation("Notification sent to user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Send notification to all admins
        /// </summary>
        public async Task SendAdminNotification(string type, object data)
        {
            try
            {
                var notification = new
                {
                    Type = type,
                    Data = data,
                    Timestamp = DateTime.UtcNow,
                    NotificationId = Guid.NewGuid().ToString()
                };

                await Clients.Group("Admins").SendAsync("AdminNotification", notification);

                _logger.LogInformation("Admin notification sent");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending admin notification");
                throw;
            }
        }

        /// <summary>
        /// Send notification to all users
        /// </summary>
        public async Task SendBroadcastNotification(string type, object data)
        {
            try
            {
                var notification = new
                {
                    Type = type,
                    Data = data,
                    Timestamp = DateTime.UtcNow,
                    NotificationId = Guid.NewGuid().ToString()
                };

                await Clients.All.SendAsync("BroadcastNotification", notification);

                _logger.LogInformation("Broadcast notification sent");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending broadcast notification");
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

    /// <summary>
    /// Represents a connection info
    /// </summary>
    public class ConnectionInfo
    {
        public string ConnectionId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public DateTime ConnectedAt { get; set; }
        public DateTime LastActivity { get; set; }
    }
}
