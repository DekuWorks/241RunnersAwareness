using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _241RunnersAPI.Data;
using _241RunnersAPI.Models;
using _241RunnersAPI.Services;

namespace _241RunnersAPI.Controllers
{
    /// <summary>
    /// Controller for topic subscription management
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TopicsController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TopicsController> _logger;
        private readonly ITopicService _topicService;

        public TopicsController(
            ApplicationDbContext context,
            ILogger<TopicsController> logger,
            ITopicService topicService)
        {
            _context = context;
            _logger = logger;
            _topicService = topicService;
        }

        /// <summary>
        /// Subscribe to a topic
        /// </summary>
        /// <param name="request">Topic subscription request</param>
        /// <returns>Subscription result</returns>
        [HttpPost("subscribe")]
        public async Task<IActionResult> SubscribeToTopic([FromBody] TopicSubscriptionDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUserIdAsInt();
                if (userId == null)
                {
                    return Unauthorized("User not authenticated");
                }

                var result = await _topicService.SubscribeToTopicAsync(userId.Value, request.Topic, request.SubscriptionReason);

                if (result.Success)
                {
                    _logger.LogInformation("User {UserId} subscribed to topic {Topic}", userId, request.Topic);
                    return Ok(new { 
                        success = true, 
                        message = $"Successfully subscribed to topic: {request.Topic}",
                        topic = request.Topic,
                        timestamp = DateTime.UtcNow
                    });
                }
                else
                {
                    return BadRequest(new { 
                        success = false, 
                        message = result.Message 
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error subscribing to topic {Topic} for user {UserId}", 
                    request.Topic, GetCurrentUserId());
                return StatusCode(500, new { 
                    success = false, 
                    message = "Internal server error" 
                });
            }
        }

        /// <summary>
        /// Unsubscribe from a topic
        /// </summary>
        /// <param name="request">Topic subscription request</param>
        /// <returns>Unsubscription result</returns>
        [HttpPost("unsubscribe")]
        public async Task<IActionResult> UnsubscribeFromTopic([FromBody] TopicSubscriptionDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUserIdAsInt();
                if (userId == null)
                {
                    return Unauthorized("User not authenticated");
                }

                var result = await _topicService.UnsubscribeFromTopicAsync(userId.Value, request.Topic);

                if (result.Success)
                {
                    _logger.LogInformation("User {UserId} unsubscribed from topic {Topic}", userId, request.Topic);
                    return Ok(new { 
                        success = true, 
                        message = $"Successfully unsubscribed from topic: {request.Topic}",
                        topic = request.Topic,
                        timestamp = DateTime.UtcNow
                    });
                }
                else
                {
                    return BadRequest(new { 
                        success = false, 
                        message = result.Message 
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unsubscribing from topic {Topic} for user {UserId}", 
                    request.Topic, GetCurrentUserId());
                return StatusCode(500, new { 
                    success = false, 
                    message = "Internal server error" 
                });
            }
        }

        /// <summary>
        /// Get user's topic subscriptions
        /// </summary>
        /// <returns>List of user's topic subscriptions</returns>
        [HttpGet("subscriptions")]
        public async Task<IActionResult> GetUserSubscriptions()
        {
            try
            {
                var userId = GetCurrentUserIdAsInt();
                if (userId == null)
                {
                    return Unauthorized("User not authenticated");
                }

                var subscriptions = await _context.TopicSubscriptions
                    .Where(ts => ts.UserId == userId)
                    .Select(ts => new TopicSubscriptionResponseDto
                    {
                        Id = ts.Id,
                        UserId = ts.UserId,
                        Topic = ts.Topic,
                        IsSubscribed = ts.IsSubscribed,
                        CreatedAt = ts.CreatedAt,
                        UpdatedAt = ts.UpdatedAt,
                        SubscriptionReason = ts.SubscriptionReason,
                        LastNotificationSent = ts.LastNotificationSent,
                        NotificationCount = ts.NotificationCount
                    })
                    .ToListAsync();

                return Ok(new { 
                    success = true, 
                    subscriptions = subscriptions,
                    count = subscriptions.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subscriptions for user {UserId}", GetCurrentUserId());
                return StatusCode(500, new { 
                    success = false, 
                    message = "Internal server error" 
                });
            }
        }

        /// <summary>
        /// Bulk subscribe to multiple topics
        /// </summary>
        /// <param name="request">Bulk subscription request</param>
        /// <returns>Bulk subscription result</returns>
        [HttpPost("bulk-subscribe")]
        public async Task<IActionResult> BulkSubscribeToTopics([FromBody] BulkTopicSubscriptionDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUserIdAsInt();
                if (userId == null)
                {
                    return Unauthorized("User not authenticated");
                }

                var results = new List<object>();
                var successCount = 0;
                var failureCount = 0;

                foreach (var topic in request.Topics)
                {
                    try
                    {
                        var result = await _topicService.SubscribeToTopicAsync(userId.Value, topic, request.SubscriptionReason);
                        if (result.Success)
                        {
                            successCount++;
                            results.Add(new { topic = topic, success = true, message = "Subscribed successfully" });
                        }
                        else
                        {
                            failureCount++;
                            results.Add(new { topic = topic, success = false, message = result.Message });
                        }
                    }
                    catch (Exception ex)
                    {
                        failureCount++;
                        results.Add(new { topic = topic, success = false, message = ex.Message });
                    }
                }

                _logger.LogInformation("Bulk subscription completed for user {UserId}: {SuccessCount} success, {FailureCount} failures", 
                    userId, successCount, failureCount);

                return Ok(new { 
                    success = true, 
                    message = $"Bulk subscription completed: {successCount} success, {failureCount} failures",
                    results = results,
                    summary = new {
                        total = request.Topics.Count,
                        success = successCount,
                        failures = failureCount
                    },
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk subscription for user {UserId}", GetCurrentUserId());
                return StatusCode(500, new { 
                    success = false, 
                    message = "Internal server error" 
                });
            }
        }

        /// <summary>
        /// Get available topics
        /// </summary>
        /// <returns>List of available topics</returns>
        [HttpGet("available")]
        public IActionResult GetAvailableTopics()
        {
            try
            {
                var predefinedTopics = Topics.GetAllPredefinedTopics();
                
                return Ok(new { 
                    success = true, 
                    topics = predefinedTopics,
                    categories = new {
                        global = new[] { Topics.OrgAll, Topics.OrgSystem },
                        roleBased = new[] { Topics.RoleAdmin, Topics.RoleParent, Topics.RoleModerator },
                        geographic = new[] { Topics.RegionTxHouston, Topics.RegionTxDallas },
                        priority = new[] { Topics.PriorityHigh, Topics.PriorityCritical }
                    },
                    count = predefinedTopics.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available topics");
                return StatusCode(500, new { 
                    success = false, 
                    message = "Internal server error" 
                });
            }
        }

        /// <summary>
        /// Get topic subscription status
        /// </summary>
        /// <param name="topic">Topic to check</param>
        /// <returns>Subscription status</returns>
        [HttpGet("status")]
        public async Task<IActionResult> GetTopicStatus([FromQuery] string topic)
        {
            try
            {
                if (string.IsNullOrEmpty(topic))
                {
                    return BadRequest("Topic is required");
                }

                var userId = GetCurrentUserIdAsInt();
                if (userId == null)
                {
                    return Unauthorized("User not authenticated");
                }

                var subscription = await _context.TopicSubscriptions
                    .FirstOrDefaultAsync(ts => ts.UserId == userId && ts.Topic == topic);

                var status = new TopicSubscriptionStatusDto
                {
                    Topic = topic,
                    IsSubscribed = subscription?.IsSubscribed ?? false,
                    SubscribedAt = subscription?.CreatedAt,
                    NotificationCount = subscription?.NotificationCount ?? 0,
                    LastNotificationSent = subscription?.LastNotificationSent
                };

                return Ok(new { 
                    success = true, 
                    status = status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting topic status for topic {Topic} and user {UserId}", 
                    topic, GetCurrentUserId());
                return StatusCode(500, new { 
                    success = false, 
                    message = "Internal server error" 
                });
            }
        }

        /// <summary>
        /// Get topic statistics (admin only)
        /// </summary>
        /// <returns>Topic statistics</returns>
        [HttpGet("stats")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetTopicStats()
        {
            try
            {
                var totalSubscriptions = await _context.TopicSubscriptions.CountAsync();
                var activeSubscriptions = await _context.TopicSubscriptions.CountAsync(ts => ts.IsSubscribed);
                
                var topicStats = await _context.TopicSubscriptions
                    .Where(ts => ts.IsSubscribed)
                    .GroupBy(ts => ts.Topic)
                    .Select(g => new {
                        topic = g.Key,
                        subscriberCount = g.Count(),
                        totalNotifications = g.Sum(ts => ts.NotificationCount)
                    })
                    .OrderByDescending(x => x.subscriberCount)
                    .ToListAsync();

                return Ok(new { 
                    success = true, 
                    stats = new {
                        totalSubscriptions = totalSubscriptions,
                        activeSubscriptions = activeSubscriptions,
                        topicStats = topicStats,
                        lastUpdated = DateTime.UtcNow
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting topic statistics");
                return StatusCode(500, new { 
                    success = false, 
                    message = "Internal server error" 
                });
            }
        }
    }
}
