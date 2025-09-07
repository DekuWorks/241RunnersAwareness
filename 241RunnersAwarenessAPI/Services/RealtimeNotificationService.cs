using Microsoft.AspNetCore.SignalR;
using _241RunnersAwarenessAPI.Hubs;

namespace _241RunnersAwarenessAPI.Services
{
    /// <summary>
    /// Service for broadcasting real-time notifications to admin clients
    /// P1 Implementation: Real-Time Admin Updates
    /// </summary>
    public class RealtimeNotificationService
    {
        private readonly IHubContext<AdminHub> _hubContext;
        private readonly ILogger<RealtimeNotificationService> _logger;

        public RealtimeNotificationService(
            IHubContext<AdminHub> hubContext,
            ILogger<RealtimeNotificationService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        /// <summary>
        /// Broadcast user changes to all connected admins
        /// </summary>
        public async Task BroadcastUserChangeAsync(string operation, object userData, string changedBy = "System")
        {
            try
            {
                _logger.LogInformation("Broadcasting user {Operation} by {ChangedBy}", operation, changedBy);

                await _hubContext.Clients.Group("Admins").SendAsync("UserChanged", new
                {
                    operation = operation,
                    user = userData,
                    changedBy = changedBy,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting user change: {Operation}", operation);
            }
        }

        /// <summary>
        /// Broadcast runner changes to all connected admins
        /// </summary>
        public async Task BroadcastRunnerChangeAsync(string operation, object runnerData, string changedBy = "System")
        {
            try
            {
                _logger.LogInformation("Broadcasting runner {Operation} by {ChangedBy}", operation, changedBy);

                await _hubContext.Clients.Group("Admins").SendAsync("RunnerChanged", new
                {
                    operation = operation,
                    runner = runnerData,
                    changedBy = changedBy,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting runner change: {Operation}", operation);
            }
        }

        /// <summary>
        /// Broadcast admin changes to all connected admins
        /// </summary>
        public async Task BroadcastAdminChangeAsync(string operation, object adminData, string changedBy = "System")
        {
            try
            {
                _logger.LogInformation("Broadcasting admin {Operation} by {ChangedBy}", operation, changedBy);

                await _hubContext.Clients.Group("Admins").SendAsync("AdminChanged", new
                {
                    operation = operation,
                    admin = adminData,
                    changedBy = changedBy,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting admin change: {Operation}", operation);
            }
        }

        /// <summary>
        /// Broadcast public case changes to all connected admins
        /// </summary>
        public async Task BroadcastPublicCaseChangeAsync(string operation, object caseData, string changedBy = "System")
        {
            try
            {
                _logger.LogInformation("Broadcasting public case {Operation} by {ChangedBy}", operation, changedBy);

                await _hubContext.Clients.Group("Admins").SendAsync("PublicCaseChanged", new
                {
                    operation = operation,
                    publicCase = caseData,
                    changedBy = changedBy,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting public case change: {Operation}", operation);
            }
        }

        /// <summary>
        /// Broadcast system status changes
        /// </summary>
        public async Task BroadcastSystemStatusChangeAsync(object statusData)
        {
            try
            {
                _logger.LogInformation("Broadcasting system status change");

                await _hubContext.Clients.Group("Admins").SendAsync("SystemStatusChanged", new
                {
                    status = statusData,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting system status change");
            }
        }

        /// <summary>
        /// Broadcast data version change
        /// </summary>
        public async Task BroadcastDataVersionChangeAsync(string version)
        {
            try
            {
                _logger.LogInformation("Broadcasting data version change: {Version}", version);

                await _hubContext.Clients.Group("Admins").SendAsync("DataVersionChanged", new
                {
                    version = version,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting data version change: {Version}", version);
            }
        }

        /// <summary>
        /// Get current connection count
        /// </summary>
        public int GetConnectionCount()
        {
            return AdminHub.GetConnectionCount();
        }

        /// <summary>
        /// Get current admin connections
        /// </summary>
        public Dictionary<string, string> GetAdminConnections()
        {
            return AdminHub.GetAdminConnections();
        }
    }
}
