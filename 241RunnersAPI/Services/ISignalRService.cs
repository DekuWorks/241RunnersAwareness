using _241RunnersAPI.Models;

namespace _241RunnersAPI.Services
{
    /// <summary>
    /// Interface for SignalR real-time communication service
    /// </summary>
    public interface ISignalRService
    {
        /// <summary>
        /// Broadcast case update to relevant users
        /// </summary>
        /// <param name="caseId">Case ID</param>
        /// <param name="caseData">Case data</param>
        /// <returns>Operation result</returns>
        Task<ServiceResult> BroadcastCaseUpdatedAsync(int caseId, object caseData);

        /// <summary>
        /// Broadcast new case to all users
        /// </summary>
        /// <param name="caseId">Case ID</param>
        /// <param name="caseData">Case data</param>
        /// <returns>Operation result</returns>
        Task<ServiceResult> BroadcastNewCaseAsync(int caseId, object caseData);

        /// <summary>
        /// Broadcast admin notice to admin users
        /// </summary>
        /// <param name="message">Admin message</param>
        /// <param name="data">Additional data</param>
        /// <returns>Operation result</returns>
        Task<ServiceResult> BroadcastAdminNoticeAsync(string message, object? data = null);

        /// <summary>
        /// Send notification to specific user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="type">Notification type</param>
        /// <param name="data">Notification data</param>
        /// <returns>Operation result</returns>
        Task<ServiceResult> SendToUserAsync(int userId, string type, object data);

        /// <summary>
        /// Send notification to all connected users
        /// </summary>
        /// <param name="type">Notification type</param>
        /// <param name="data">Notification data</param>
        /// <returns>Operation result</returns>
        Task<ServiceResult> SendToAllAsync(string type, object data);

        /// <summary>
        /// Send notification to admin users
        /// </summary>
        /// <param name="type">Notification type</param>
        /// <param name="data">Notification data</param>
        /// <returns>Operation result</returns>
        Task<ServiceResult> SendToAdminsAsync(string type, object data);

        /// <summary>
        /// Send notification to users subscribed to a topic
        /// </summary>
        /// <param name="topic">Topic name</param>
        /// <param name="type">Notification type</param>
        /// <param name="data">Notification data</param>
        /// <returns>Operation result</returns>
        Task<ServiceResult> SendToTopicAsync(string topic, string type, object data);

        /// <summary>
        /// Get connection statistics
        /// </summary>
        /// <returns>Connection statistics</returns>
        Task<object> GetConnectionStatsAsync();
    }
}
