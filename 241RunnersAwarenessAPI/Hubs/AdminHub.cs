using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace _241RunnersAwarenessAPI.Hubs
{
    /// <summary>
    /// SignalR Hub for real-time admin updates
    /// P1 Implementation: Real-Time Admin Updates
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class AdminHub : Hub
    {
        private readonly ILogger<AdminHub> _logger;
        private static readonly Dictionary<string, string> _adminConnections = new();

        public AdminHub(ILogger<AdminHub> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Join the admin group for receiving updates
        /// </summary>
        public async Task JoinAdminGroup()
        {
            try
            {
                var userEmail = GetUserEmail();
                var connectionId = Context.ConnectionId;

                // Store connection mapping
                _adminConnections[connectionId] = userEmail;

                // Join the admin group
                await Groups.AddToGroupAsync(connectionId, "Admins");

                _logger.LogInformation("Admin {Email} joined admin group with connection {ConnectionId}", 
                    userEmail, connectionId);

                // Notify other admins
                await Clients.OthersInGroup("Admins").SendAsync("AdminJoined", new
                {
                    email = userEmail,
                    connectionId = connectionId,
                    timestamp = DateTime.UtcNow
                });

                // Send current connection count
                var connectionCount = _adminConnections.Count;
                await Clients.Caller.SendAsync("ConnectionInfo", new
                {
                    connectionId = connectionId,
                    totalConnections = connectionCount,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining admin group for connection {ConnectionId}", Context.ConnectionId);
                throw;
            }
        }

        /// <summary>
        /// Leave the admin group
        /// </summary>
        public async Task LeaveAdminGroup()
        {
            try
            {
                var userEmail = GetUserEmail();
                var connectionId = Context.ConnectionId;

                // Remove from connection mapping
                _adminConnections.Remove(connectionId);

                // Leave the admin group
                await Groups.RemoveFromGroupAsync(connectionId, "Admins");

                _logger.LogInformation("Admin {Email} left admin group with connection {ConnectionId}", 
                    userEmail, connectionId);

                // Notify other admins
                await Clients.OthersInGroup("Admins").SendAsync("AdminLeft", new
                {
                    email = userEmail,
                    connectionId = connectionId,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error leaving admin group for connection {ConnectionId}", Context.ConnectionId);
            }
        }

        /// <summary>
        /// Broadcast user changes to all admins
        /// </summary>
        public async Task BroadcastUserChange(string operation, object userData)
        {
            try
            {
                var userEmail = GetUserEmail();
                
                _logger.LogInformation("Admin {Email} broadcasting user {Operation}", userEmail, operation);

                await Clients.Group("Admins").SendAsync("UserChanged", new
                {
                    operation = operation,
                    user = userData,
                    changedBy = userEmail,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting user change for connection {ConnectionId}", Context.ConnectionId);
            }
        }

        /// <summary>
        /// Broadcast runner changes to all admins
        /// </summary>
        public async Task BroadcastRunnerChange(string operation, object runnerData)
        {
            try
            {
                var userEmail = GetUserEmail();
                
                _logger.LogInformation("Admin {Email} broadcasting runner {Operation}", userEmail, operation);

                await Clients.Group("Admins").SendAsync("RunnerChanged", new
                {
                    operation = operation,
                    runner = runnerData,
                    changedBy = userEmail,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting runner change for connection {ConnectionId}", Context.ConnectionId);
            }
        }

        /// <summary>
        /// Broadcast admin changes to all admins
        /// </summary>
        public async Task BroadcastAdminChange(string operation, object adminData)
        {
            try
            {
                var userEmail = GetUserEmail();
                
                _logger.LogInformation("Admin {Email} broadcasting admin {Operation}", userEmail, operation);

                await Clients.Group("Admins").SendAsync("AdminChanged", new
                {
                    operation = operation,
                    admin = adminData,
                    changedBy = userEmail,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting admin change for connection {ConnectionId}", Context.ConnectionId);
            }
        }

        /// <summary>
        /// Broadcast public case changes to all admins
        /// </summary>
        public async Task BroadcastPublicCaseChange(string operation, object caseData)
        {
            try
            {
                var userEmail = GetUserEmail();
                
                _logger.LogInformation("Admin {Email} broadcasting public case {Operation}", userEmail, operation);

                await Clients.Group("Admins").SendAsync("PublicCaseChanged", new
                {
                    operation = operation,
                    publicCase = caseData,
                    changedBy = userEmail,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting public case change for connection {ConnectionId}", Context.ConnectionId);
            }
        }

        /// <summary>
        /// Broadcast system status changes
        /// </summary>
        public async Task BroadcastSystemStatusChange(object statusData)
        {
            try
            {
                _logger.LogInformation("Broadcasting system status change");

                await Clients.Group("Admins").SendAsync("SystemStatusChanged", new
                {
                    status = statusData,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting system status change for connection {ConnectionId}", Context.ConnectionId);
            }
        }

        /// <summary>
        /// Broadcast data version change
        /// </summary>
        public async Task BroadcastDataVersionChange(string version)
        {
            try
            {
                _logger.LogInformation("Broadcasting data version change: {Version}", version);

                await Clients.Group("Admins").SendAsync("DataVersionChanged", new
                {
                    version = version,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting data version change for connection {ConnectionId}", Context.ConnectionId);
            }
        }

        /// <summary>
        /// Get current user email from claims
        /// </summary>
        private string GetUserEmail()
        {
            return Context.User?.FindFirst(ClaimTypes.Email)?.Value ?? 
                   Context.User?.FindFirst("email")?.Value ?? 
                   "Unknown";
        }

        /// <summary>
        /// Handle connection established
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            try
            {
                var userEmail = GetUserEmail();
                var connectionId = Context.ConnectionId;

                _logger.LogInformation("Admin {Email} connected with connection {ConnectionId}", userEmail, connectionId);

                // Store connection mapping
                _adminConnections[connectionId] = userEmail;

                await base.OnConnectedAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnConnectedAsync for connection {ConnectionId}", Context.ConnectionId);
                await base.OnConnectedAsync();
            }
        }

        /// <summary>
        /// Handle connection terminated
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            try
            {
                var connectionId = Context.ConnectionId;
                var userEmail = _adminConnections.GetValueOrDefault(connectionId, "Unknown");

                _logger.LogInformation("Admin {Email} disconnected with connection {ConnectionId}", userEmail, connectionId);

                // Remove from connection mapping
                _adminConnections.Remove(connectionId);

                // Notify other admins
                await Clients.OthersInGroup("Admins").SendAsync("AdminLeft", new
                {
                    email = userEmail,
                    connectionId = connectionId,
                    timestamp = DateTime.UtcNow
                });

                await base.OnDisconnectedAsync(exception);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnDisconnectedAsync for connection {ConnectionId}", Context.ConnectionId);
                await base.OnDisconnectedAsync(exception);
            }
        }

        /// <summary>
        /// Get current admin connections (for debugging)
        /// </summary>
        public static Dictionary<string, string> GetAdminConnections()
        {
            return new Dictionary<string, string>(_adminConnections);
        }

        /// <summary>
        /// Get connection count
        /// </summary>
        public static int GetConnectionCount()
        {
            return _adminConnections.Count;
        }
    }
}
