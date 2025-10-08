using _241RunnersAPI.Models;

namespace _241RunnersAPI.Services
{
    /// <summary>
    /// Interface for Firebase notification service
    /// </summary>
    public interface IFirebaseNotificationService
    {
        /// <summary>
        /// Send notification to a specific user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="notification">Notification data</param>
        /// <returns>Operation result</returns>
        Task<ServiceResult> SendNotificationToUserAsync(int userId, CreateNotificationDto notification);

        /// <summary>
        /// Send notification to multiple users
        /// </summary>
        /// <param name="userIds">List of user IDs</param>
        /// <param name="notification">Notification data</param>
        /// <returns>Operation result</returns>
        Task<ServiceResult> SendNotificationToUsersAsync(List<int> userIds, CreateNotificationDto notification);

        /// <summary>
        /// Send notification to a topic
        /// </summary>
        /// <param name="topic">Topic name</param>
        /// <param name="notification">Notification data</param>
        /// <returns>Operation result</returns>
        Task<ServiceResult> SendNotificationToTopicAsync(string topic, CreateNotificationDto notification);

        /// <summary>
        /// Send notification to multiple topics
        /// </summary>
        /// <param name="topics">List of topic names</param>
        /// <param name="notification">Notification data</param>
        /// <returns>Operation result</returns>
        Task<ServiceResult> SendNotificationToTopicsAsync(List<string> topics, CreateNotificationDto notification);

        /// <summary>
        /// Send notification to all users
        /// </summary>
        /// <param name="notification">Notification data</param>
        /// <returns>Operation result</returns>
        Task<ServiceResult> SendNotificationToAllUsersAsync(CreateNotificationDto notification);

        /// <summary>
        /// Send notification to admin users
        /// </summary>
        /// <param name="notification">Notification data</param>
        /// <returns>Operation result</returns>
        Task<ServiceResult> SendNotificationToAdminsAsync(CreateNotificationDto notification);

        /// <summary>
        /// Send case update notification
        /// </summary>
        /// <param name="caseId">Case ID</param>
        /// <param name="caseData">Case data</param>
        /// <returns>Operation result</returns>
        Task<ServiceResult> SendCaseUpdateNotificationAsync(int caseId, object caseData);

        /// <summary>
        /// Send new case notification
        /// </summary>
        /// <param name="caseId">Case ID</param>
        /// <param name="caseData">Case data</param>
        /// <returns>Operation result</returns>
        Task<ServiceResult> SendNewCaseNotificationAsync(int caseId, object caseData);

        /// <summary>
        /// Send admin notice notification
        /// </summary>
        /// <param name="message">Admin message</param>
        /// <param name="data">Additional data</param>
        /// <returns>Operation result</returns>
        Task<ServiceResult> SendAdminNoticeNotificationAsync(string message, object? data = null);

        /// <summary>
        /// Initialize Firebase Admin SDK
        /// </summary>
        /// <returns>Initialization result</returns>
        Task<ServiceResult> InitializeAsync();

        /// <summary>
        /// Test Firebase connection
        /// </summary>
        /// <returns>Test result</returns>
        Task<ServiceResult> TestConnectionAsync();
    }
}
