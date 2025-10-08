using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace _241RunnersAPI.Hubs
{
    /// <summary>
    /// SignalR Hub for real-time admin dashboard communication
    /// Handles cross-admin synchronization and real-time updates
    /// </summary>
    [Authorize(Roles = "admin")]
    public class AdminHub : Hub
    {
        private readonly ILogger<AdminHub> _logger;
        private static readonly Dictionary<string, AdminConnection> _adminConnections = new();
        private static readonly object _lockObject = new();

        public AdminHub(ILogger<AdminHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            try
            {
                var adminId = GetAdminId();
                var adminEmail = GetAdminEmail();
                var adminName = GetAdminName();

                if (string.IsNullOrEmpty(adminId) || string.IsNullOrEmpty(adminEmail))
                {
                    _logger.LogWarning("Admin connection attempt without valid authentication");
                    Context.Abort();
                    return;
                }

                var connectionId = Context.ConnectionId;
                
                lock (_lockObject)
                {
                    _adminConnections[connectionId] = new AdminConnection
                    {
                        ConnectionId = connectionId,
                        AdminId = adminId,
                        AdminEmail = adminEmail,
                        AdminName = adminName,
                        ConnectedAt = DateTime.UtcNow,
                        LastActivity = DateTime.UtcNow
                    };
                }

                // Join admin group for broadcast messages
                await Groups.AddToGroupAsync(connectionId, "Admins");
                
                // Notify other admins about new connection
                await Clients.OthersInGroup("Admins").SendAsync("AdminConnected", new
                {
                    AdminId = adminId,
                    AdminName = adminName,
                    AdminEmail = adminEmail,
                    ConnectedAt = DateTime.UtcNow,
                    ConnectionId = connectionId
                });

                // Send current admin list to the newly connected admin
                var currentAdmins = GetCurrentAdmins();
                await Clients.Caller.SendAsync("CurrentAdmins", currentAdmins);

                _logger.LogInformation("Admin {AdminName} ({AdminEmail}) connected with connection ID {ConnectionId}", 
                    adminName, adminEmail, connectionId);

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
                AdminConnection? adminConnection = null;

                lock (_lockObject)
                {
                    if (_adminConnections.TryGetValue(connectionId, out adminConnection))
                    {
                        _adminConnections.Remove(connectionId);
                    }
                }

                if (adminConnection != null)
                {
                    // Notify other admins about disconnection
                    await Clients.OthersInGroup("Admins").SendAsync("AdminDisconnected", new
                    {
                        AdminId = adminConnection.AdminId,
                        AdminName = adminConnection.AdminName,
                        AdminEmail = adminConnection.AdminEmail,
                        DisconnectedAt = DateTime.UtcNow,
                        ConnectionId = connectionId
                    });

                    _logger.LogInformation("Admin {AdminName} ({AdminEmail}) disconnected", 
                        adminConnection.AdminName, adminConnection.AdminEmail);
                }

                await base.OnDisconnectedAsync(exception);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnDisconnectedAsync");
            }
        }

        /// <summary>
        /// Handle user data changes and broadcast to all admins
        /// </summary>
        public async Task UserChanged(string operation, object userData, string changedBy)
        {
            try
            {
                var adminName = GetAdminName();
                var adminEmail = GetAdminEmail();

                var changeData = new
                {
                    Operation = operation, // "created", "updated", "deleted", "activated", "deactivated"
                    UserData = userData,
                    ChangedBy = adminName,
                    ChangedByEmail = adminEmail,
                    Timestamp = DateTime.UtcNow,
                    ChangeId = Guid.NewGuid().ToString()
                };

                // Broadcast to all connected admins
                await Clients.Group("Admins").SendAsync("UserChanged", changeData);

                _logger.LogInformation("User {Operation} by admin {AdminName} ({AdminEmail})", 
                    operation, adminName, adminEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting user change");
                throw;
            }
        }

        /// <summary>
        /// Handle runner data changes and broadcast to all admins
        /// </summary>
        public async Task RunnerChanged(string operation, object runnerData, string changedBy)
        {
            try
            {
                var adminName = GetAdminName();
                var adminEmail = GetAdminEmail();

                var changeData = new
                {
                    Operation = operation, // "created", "updated", "deleted", "activated", "deactivated"
                    RunnerData = runnerData,
                    ChangedBy = adminName,
                    ChangedByEmail = adminEmail,
                    Timestamp = DateTime.UtcNow,
                    ChangeId = Guid.NewGuid().ToString()
                };

                // Broadcast to all connected admins
                await Clients.Group("Admins").SendAsync("RunnerChanged", changeData);

                _logger.LogInformation("Runner {Operation} by admin {AdminName} ({AdminEmail})", 
                    operation, adminName, adminEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting runner change");
                throw;
            }
        }

        /// <summary>
        /// Handle admin profile changes and broadcast to all admins
        /// </summary>
        public async Task AdminProfileChanged(string operation, object adminData, string changedBy)
        {
            try
            {
                var adminName = GetAdminName();
                var adminEmail = GetAdminEmail();

                var changeData = new
                {
                    Operation = operation, // "profile_updated", "password_changed", "role_changed"
                    AdminData = adminData,
                    ChangedBy = adminName,
                    ChangedByEmail = adminEmail,
                    Timestamp = DateTime.UtcNow,
                    ChangeId = Guid.NewGuid().ToString()
                };

                // Broadcast to all connected admins
                await Clients.Group("Admins").SendAsync("AdminProfileChanged", changeData);

                _logger.LogInformation("Admin profile {Operation} by admin {AdminName} ({AdminEmail})", 
                    operation, adminName, adminEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting admin profile change");
                throw;
            }
        }

        /// <summary>
        /// Handle system-wide data changes (like database updates)
        /// </summary>
        public async Task DataVersionChanged(string dataType, int version, string changedBy)
        {
            try
            {
                var adminName = GetAdminName();
                var adminEmail = GetAdminEmail();

                var changeData = new
                {
                    DataType = dataType, // "users", "runners", "admins", "public_cases"
                    Version = version,
                    ChangedBy = adminName,
                    ChangedByEmail = adminEmail,
                    Timestamp = DateTime.UtcNow,
                    ChangeId = Guid.NewGuid().ToString()
                };

                // Broadcast to all connected admins
                await Clients.Group("Admins").SendAsync("DataVersionChanged", changeData);

                _logger.LogInformation("Data version changed for {DataType} to version {Version} by admin {AdminName}", 
                    dataType, version, adminName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting data version change");
                throw;
            }
        }

        /// <summary>
        /// Send admin activity notification
        /// </summary>
        public async Task AdminActivity(string activity, object? data = null)
        {
            try
            {
                var adminName = GetAdminName();
                var adminEmail = GetAdminEmail();

                var activityData = new
                {
                    Activity = activity, // "login", "logout", "dashboard_view", "user_management", etc.
                    Data = data,
                    AdminName = adminName,
                    AdminEmail = adminEmail,
                    Timestamp = DateTime.UtcNow
                };

                // Broadcast to all connected admins
                await Clients.Group("Admins").SendAsync("AdminActivity", activityData);

                _logger.LogInformation("Admin activity: {Activity} by {AdminName} ({AdminEmail})", 
                    activity, adminName, adminEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting admin activity");
                throw;
            }
        }

        /// <summary>
        /// Notify admins about new user registration
        /// </summary>
        public async Task NotifyNewUserRegistration(object userData)
        {
            try
            {
                var notificationData = new
                {
                    Type = "new_user_registration",
                    UserData = userData,
                    Timestamp = DateTime.UtcNow,
                    NotificationId = Guid.NewGuid().ToString()
                };

                // Broadcast to all connected admins
                await Clients.Group("Admins").SendAsync("NewUserRegistration", notificationData);

                _logger.LogInformation("New user registration notification sent to admins");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting new user registration notification");
                throw;
            }
        }

        /// <summary>
        /// Get current online admins
        /// </summary>
        public async Task GetOnlineAdmins()
        {
            try
            {
                var currentAdmins = GetCurrentAdmins();
                await Clients.Caller.SendAsync("OnlineAdmins", currentAdmins);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting online admins");
                throw;
            }
        }

        /// <summary>
        /// Get current admins (alias for GetOnlineAdmins for frontend compatibility)
        /// </summary>
        public async Task CurrentAdmins()
        {
            try
            {
                var currentAdmins = GetCurrentAdmins();
                await Clients.Caller.SendAsync("CurrentAdmins", currentAdmins);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current admins");
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
                    if (_adminConnections.TryGetValue(connectionId, out var adminConnection))
                    {
                        adminConnection.LastActivity = DateTime.UtcNow;
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

        private string? GetAdminId()
        {
            return Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        private string? GetAdminEmail()
        {
            return Context.User?.FindFirst(ClaimTypes.Email)?.Value;
        }

        private string? GetAdminName()
        {
            var firstName = Context.User?.FindFirst("firstName")?.Value;
            var lastName = Context.User?.FindFirst("lastName")?.Value;
            
            if (!string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName))
            {
                return $"{firstName} {lastName}";
            }
            
            return Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown Admin";
        }

        private List<object> GetCurrentAdmins()
        {
            lock (_lockObject)
            {
                return _adminConnections.Values
                    .Where(conn => conn.LastActivity > DateTime.UtcNow.AddMinutes(-5)) // Only active connections
                    .Select(conn => new
                    {
                        AdminId = conn.AdminId,
                        AdminName = conn.AdminName,
                        AdminEmail = conn.AdminEmail,
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
    /// Represents an admin connection
    /// </summary>
    public class AdminConnection
    {
        public string ConnectionId { get; set; } = string.Empty;
        public string AdminId { get; set; } = string.Empty;
        public string AdminEmail { get; set; } = string.Empty;
        public string AdminName { get; set; } = string.Empty;
        public DateTime ConnectedAt { get; set; }
        public DateTime LastActivity { get; set; }
    }
}
