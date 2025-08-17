using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using _241RunnersAwareness.BackendAPI.Services;

namespace _241RunnersAwareness.BackendAPI.Hubs
{
    /// <summary>
    /// SignalR Hub for real-time notifications
    /// </summary>
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;
        private readonly IRealTimeNotificationService _notificationService;

        public NotificationHub(ILogger<NotificationHub> logger, IRealTimeNotificationService notificationService)
        {
            _logger = logger;
            _notificationService = notificationService;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation($"Client connected: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _logger.LogInformation($"Client disconnected: {Context.ConnectionId}");
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Join law enforcement group
        /// </summary>
        public async Task JoinLawEnforcement()
        {
            await _notificationService.AddToGroupAsync(Context.ConnectionId, "law-enforcement");
            _logger.LogInformation($"Client {Context.ConnectionId} joined law-enforcement group");
        }

        /// <summary>
        /// Join emergency contacts group
        /// </summary>
        public async Task JoinEmergencyContacts()
        {
            await _notificationService.AddToGroupAsync(Context.ConnectionId, "emergency-contacts");
            _logger.LogInformation($"Client {Context.ConnectionId} joined emergency-contacts group");
        }

        /// <summary>
        /// Join media group
        /// </summary>
        public async Task JoinMedia()
        {
            await _notificationService.AddToGroupAsync(Context.ConnectionId, "media");
            _logger.LogInformation($"Client {Context.ConnectionId} joined media group");
        }

        /// <summary>
        /// Join case stakeholders group
        /// </summary>
        public async Task JoinCaseStakeholders(string caseId)
        {
            var groupName = $"case-{caseId}";
            await _notificationService.AddToGroupAsync(Context.ConnectionId, groupName);
            await _notificationService.AddToGroupAsync(Context.ConnectionId, "case-stakeholders");
            _logger.LogInformation($"Client {Context.ConnectionId} joined case stakeholders group for case {caseId}");
        }

        /// <summary>
        /// Join admins group
        /// </summary>
        public async Task JoinAdmins()
        {
            await _notificationService.AddToGroupAsync(Context.ConnectionId, "admins");
            _logger.LogInformation($"Client {Context.ConnectionId} joined admins group");
        }

        /// <summary>
        /// Leave a group
        /// </summary>
        public async Task LeaveGroup(string groupName)
        {
            await _notificationService.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation($"Client {Context.ConnectionId} left group {groupName}");
        }

        /// <summary>
        /// Send a test message
        /// </summary>
        public async Task SendTestMessage(string message)
        {
            await Clients.Caller.SendAsync("ReceiveTestMessage", new
            {
                Message = message,
                Timestamp = DateTime.UtcNow,
                ConnectionId = Context.ConnectionId
            });
        }
    }
}
