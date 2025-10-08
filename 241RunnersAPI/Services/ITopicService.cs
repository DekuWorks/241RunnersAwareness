using _241RunnersAPI.Models;

namespace _241RunnersAPI.Services
{
    /// <summary>
    /// Interface for topic subscription service
    /// </summary>
    public interface ITopicService
    {
        /// <summary>
        /// Subscribe to a topic
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="topic">Topic name</param>
        /// <param name="reason">Subscription reason</param>
        /// <returns>Operation result</returns>
        Task<ServiceResult> SubscribeToTopicAsync(int userId, string topic, string? reason = null);

        /// <summary>
        /// Unsubscribe from a topic
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="topic">Topic name</param>
        /// <returns>Operation result</returns>
        Task<ServiceResult> UnsubscribeFromTopicAsync(int userId, string topic);

        /// <summary>
        /// Subscribe to default topics based on user role
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="role">User role</param>
        /// <returns>Operation result</returns>
        Task<ServiceResult> SubscribeToDefaultTopicsAsync(int userId, string role);

        /// <summary>
        /// Get user's topic subscriptions
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of topic subscriptions</returns>
        Task<List<TopicSubscription>> GetUserSubscriptionsAsync(int userId);

        /// <summary>
        /// Check if user is subscribed to a topic
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="topic">Topic name</param>
        /// <returns>True if subscribed</returns>
        Task<bool> IsSubscribedToTopicAsync(int userId, string topic);

        /// <summary>
        /// Get subscribers for a topic
        /// </summary>
        /// <param name="topic">Topic name</param>
        /// <returns>List of user IDs</returns>
        Task<List<int>> GetTopicSubscribersAsync(string topic);

        /// <summary>
        /// Update notification count for a topic subscription
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="topic">Topic name</param>
        /// <returns>Operation result</returns>
        Task<ServiceResult> UpdateNotificationCountAsync(int userId, string topic);

        /// <summary>
        /// Clean up inactive subscriptions
        /// </summary>
        /// <returns>Number of subscriptions cleaned up</returns>
        Task<int> CleanupInactiveSubscriptionsAsync();
    }

    /// <summary>
    /// Service result wrapper
    /// </summary>
    public class ServiceResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }

        public static ServiceResult CreateSuccess(string message = "Operation completed successfully", object? data = null)
        {
            return new ServiceResult { Success = true, Message = message, Data = data };
        }

        public static ServiceResult CreateFailure(string message = "Operation failed")
        {
            return new ServiceResult { Success = false, Message = message };
        }
    }
}
