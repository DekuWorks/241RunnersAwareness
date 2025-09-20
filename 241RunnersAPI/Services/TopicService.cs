using Microsoft.EntityFrameworkCore;
using _241RunnersAPI.Data;
using _241RunnersAPI.Models;

namespace _241RunnersAPI.Services
{
    /// <summary>
    /// Service for managing topic subscriptions
    /// </summary>
    public class TopicService : ITopicService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TopicService> _logger;

        public TopicService(ApplicationDbContext context, ILogger<TopicService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Subscribe to a topic
        /// </summary>
        public async Task<ServiceResult> SubscribeToTopicAsync(int userId, string topic, string? reason = null)
        {
            try
            {
                // Validate topic format
                if (!IsValidTopic(topic))
                {
                    return ServiceResult.CreateFailure($"Invalid topic format: {topic}");
                }

                // Check if subscription already exists
                var existingSubscription = await _context.TopicSubscriptions
                    .FirstOrDefaultAsync(ts => ts.UserId == userId && ts.Topic == topic);

                if (existingSubscription != null)
                {
                    if (existingSubscription.IsSubscribed)
                    {
                        return ServiceResult.CreateSuccess($"Already subscribed to topic: {topic}");
                    }
                    else
                    {
                        // Reactivate subscription
                        existingSubscription.IsSubscribed = true;
                        existingSubscription.UpdatedAt = DateTime.UtcNow;
                        existingSubscription.SubscriptionReason = reason ?? existingSubscription.SubscriptionReason;
                        
                        _context.TopicSubscriptions.Update(existingSubscription);
                    }
                }
                else
                {
                    // Create new subscription
                    var subscription = new TopicSubscription
                    {
                        UserId = userId,
                        Topic = topic,
                        IsSubscribed = true,
                        SubscriptionReason = reason ?? "user_requested",
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.TopicSubscriptions.Add(subscription);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("User {UserId} subscribed to topic {Topic}", userId, topic);
                return ServiceResult.CreateSuccess($"Successfully subscribed to topic: {topic}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error subscribing user {UserId} to topic {Topic}", userId, topic);
                return ServiceResult.CreateFailure("Failed to subscribe to topic");
            }
        }

        /// <summary>
        /// Unsubscribe from a topic
        /// </summary>
        public async Task<ServiceResult> UnsubscribeFromTopicAsync(int userId, string topic)
        {
            try
            {
                var subscription = await _context.TopicSubscriptions
                    .FirstOrDefaultAsync(ts => ts.UserId == userId && ts.Topic == topic);

                if (subscription == null)
                {
                    return ServiceResult.CreateFailure($"Not subscribed to topic: {topic}");
                }

                if (!subscription.IsSubscribed)
                {
                    return ServiceResult.CreateSuccess($"Already unsubscribed from topic: {topic}");
                }

                subscription.IsSubscribed = false;
                subscription.UpdatedAt = DateTime.UtcNow;

                _context.TopicSubscriptions.Update(subscription);
                await _context.SaveChangesAsync();

                _logger.LogInformation("User {UserId} unsubscribed from topic {Topic}", userId, topic);
                return ServiceResult.CreateSuccess($"Successfully unsubscribed from topic: {topic}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unsubscribing user {UserId} from topic {Topic}", userId, topic);
                return ServiceResult.CreateFailure("Failed to unsubscribe from topic");
            }
        }

        /// <summary>
        /// Subscribe to default topics based on user role
        /// </summary>
        public async Task<ServiceResult> SubscribeToDefaultTopicsAsync(int userId, string role)
        {
            try
            {
                var defaultTopics = Topics.GetDefaultTopics(role);
                var results = new List<string>();

                foreach (var topic in defaultTopics)
                {
                    var result = await SubscribeToTopicAsync(userId, topic, "auto_subscribed");
                    if (result.Success)
                    {
                        results.Add(topic);
                    }
                }

                _logger.LogInformation("User {UserId} with role {Role} subscribed to {Count} default topics", 
                    userId, role, results.Count);

                return ServiceResult.CreateSuccess(
                    $"Subscribed to {results.Count} default topics", 
                    results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error subscribing user {UserId} to default topics", userId);
                return ServiceResult.CreateFailure("Failed to subscribe to default topics");
            }
        }

        /// <summary>
        /// Get user's topic subscriptions
        /// </summary>
        public async Task<List<TopicSubscription>> GetUserSubscriptionsAsync(int userId)
        {
            try
            {
                return await _context.TopicSubscriptions
                    .Where(ts => ts.UserId == userId && ts.IsSubscribed)
                    .OrderBy(ts => ts.Topic)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subscriptions for user {UserId}", userId);
                return new List<TopicSubscription>();
            }
        }

        /// <summary>
        /// Check if user is subscribed to a topic
        /// </summary>
        public async Task<bool> IsSubscribedToTopicAsync(int userId, string topic)
        {
            try
            {
                return await _context.TopicSubscriptions
                    .AnyAsync(ts => ts.UserId == userId && ts.Topic == topic && ts.IsSubscribed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking subscription for user {UserId} to topic {Topic}", userId, topic);
                return false;
            }
        }

        /// <summary>
        /// Get subscribers for a topic
        /// </summary>
        public async Task<List<int>> GetTopicSubscribersAsync(string topic)
        {
            try
            {
                return await _context.TopicSubscriptions
                    .Where(ts => ts.Topic == topic && ts.IsSubscribed)
                    .Select(ts => ts.UserId)
                    .Distinct()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subscribers for topic {Topic}", topic);
                return new List<int>();
            }
        }

        /// <summary>
        /// Update notification count for a topic subscription
        /// </summary>
        public async Task<ServiceResult> UpdateNotificationCountAsync(int userId, string topic)
        {
            try
            {
                var subscription = await _context.TopicSubscriptions
                    .FirstOrDefaultAsync(ts => ts.UserId == userId && ts.Topic == topic && ts.IsSubscribed);

                if (subscription == null)
                {
                    return ServiceResult.CreateFailure("Subscription not found");
                }

                subscription.NotificationCount++;
                subscription.LastNotificationSent = DateTime.UtcNow;
                subscription.UpdatedAt = DateTime.UtcNow;

                _context.TopicSubscriptions.Update(subscription);
                await _context.SaveChangesAsync();

                return ServiceResult.CreateSuccess("Notification count updated");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notification count for user {UserId} topic {Topic}", userId, topic);
                return ServiceResult.CreateFailure("Failed to update notification count");
            }
        }

        /// <summary>
        /// Clean up inactive subscriptions
        /// </summary>
        public async Task<int> CleanupInactiveSubscriptionsAsync()
        {
            try
            {
                // Remove subscriptions that haven't been active for 90 days
                var cutoffDate = DateTime.UtcNow.AddDays(-90);
                var inactiveSubscriptions = await _context.TopicSubscriptions
                    .Where(ts => !ts.IsSubscribed && ts.UpdatedAt < cutoffDate)
                    .ToListAsync();

                if (inactiveSubscriptions.Any())
                {
                    _context.TopicSubscriptions.RemoveRange(inactiveSubscriptions);
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation("Cleaned up {Count} inactive subscriptions", inactiveSubscriptions.Count);
                return inactiveSubscriptions.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up inactive subscriptions");
                return 0;
            }
        }

        /// <summary>
        /// Validate topic format
        /// </summary>
        private static bool IsValidTopic(string topic)
        {
            if (string.IsNullOrWhiteSpace(topic))
                return false;

            // Check length
            if (topic.Length > 100)
                return false;

            // Check format (alphanumeric, underscores, and hyphens)
            return System.Text.RegularExpressions.Regex.IsMatch(topic, @"^[a-zA-Z0-9_-]+$");
        }
    }
}
