using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using _241RunnersAwareness.BackendAPI.Hubs;

namespace _241RunnersAwareness.BackendAPI.Services
{
    public interface IRealTimeNotificationService
    {
        Task SendUrgentAlertToAllAsync(string caseId, string individualName, string location, string description);
        Task SendAlertToLawEnforcementAsync(string caseId, string individualName, string location, string specialNeeds);
        Task SendAlertToEmergencyContactsAsync(string caseId, string individualName, string contactInfo);
        Task SendCaseUpdateAsync(string caseId, string status, string updatedBy);
        Task SendFoundNotificationAsync(string caseId, string individualName, string foundLocation);
        Task SendToSpecificGroupAsync(string groupName, string message, object data);
        Task AddToGroupAsync(string connectionId, string groupName);
        Task RemoveFromGroupAsync(string connectionId, string groupName);
    }

    public class RealTimeNotificationService : IRealTimeNotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<RealTimeNotificationService> _logger;
        private readonly ConcurrentDictionary<string, HashSet<string>> _groupConnections;

        public RealTimeNotificationService(IHubContext<NotificationHub> hubContext, ILogger<RealTimeNotificationService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
            _groupConnections = new ConcurrentDictionary<string, HashSet<string>>();
        }

        /// <summary>
        /// Sends URGENT real-time alert to ALL connected clients
        /// This is the most critical notification - sent immediately when a case is reported
        /// </summary>
        public async Task SendUrgentAlertToAllAsync(string caseId, string individualName, string location, string description)
        {
            try
            {
                _logger.LogWarning($"🚨 REAL-TIME URGENT ALERT: Case {caseId} - {individualName} at {location}");

                var alertData = new
                {
                    Type = "URGENT_ALERT",
                    CaseId = caseId,
                    IndividualName = individualName,
                    Location = location,
                    Description = description,
                    Timestamp = DateTime.UtcNow,
                    Priority = "CRITICAL",
                    Message = $"🚨 URGENT: {individualName} reported missing at {location}. Case ID: {caseId}. Call 911 immediately if seen."
                };

                await _hubContext.Clients.All.SendAsync("ReceiveUrgentAlert", alertData);

                // Also send to specific groups
                await SendToSpecificGroupAsync("law-enforcement", "ReceiveLawEnforcementAlert", alertData);
                await SendToSpecificGroupAsync("emergency-contacts", "ReceiveEmergencyAlert", alertData);
                await SendToSpecificGroupAsync("media", "ReceiveMediaAlert", alertData);

                _logger.LogInformation($"Real-time urgent alert sent successfully for case {caseId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send real-time urgent alert for case {caseId}");
                throw;
            }
        }

        /// <summary>
        /// Sends immediate alert to law enforcement connections
        /// </summary>
        public async Task SendAlertToLawEnforcementAsync(string caseId, string individualName, string location, string specialNeeds)
        {
            try
            {
                var alertData = new
                {
                    Type = "LAW_ENFORCEMENT_ALERT",
                    CaseId = caseId,
                    IndividualName = individualName,
                    Location = location,
                    SpecialNeeds = specialNeeds,
                    Timestamp = DateTime.UtcNow,
                    Priority = "HIGH",
                    Message = $"LAW ENFORCEMENT ALERT: {individualName} missing at {location}. Special needs: {specialNeeds}. Case ID: {caseId}",
                    Actions = new[]
                    {
                        "Issue BOLO immediately",
                        "Check local hospitals",
                        "Monitor traffic cameras",
                        "Coordinate with other agencies"
                    }
                };

                await SendToSpecificGroupAsync("law-enforcement", "ReceiveLawEnforcementAlert", alertData);

                _logger.LogInformation($"Law enforcement alert sent for case {caseId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send law enforcement alert for case {caseId}");
                throw;
            }
        }

        /// <summary>
        /// Sends immediate alert to emergency contacts
        /// </summary>
        public async Task SendAlertToEmergencyContactsAsync(string caseId, string individualName, string contactInfo)
        {
            try
            {
                var alertData = new
                {
                    Type = "EMERGENCY_CONTACT_ALERT",
                    CaseId = caseId,
                    IndividualName = individualName,
                    ContactInfo = contactInfo,
                    Timestamp = DateTime.UtcNow,
                    Priority = "CRITICAL",
                    Message = $"🚨 EMERGENCY: {individualName} is missing. Case ID: {caseId}. Call 911 immediately.",
                    Actions = new[]
                    {
                        "Call 911 immediately",
                        "Check all known locations",
                        "Contact other family members",
                        "Monitor social media"
                    }
                };

                await SendToSpecificGroupAsync("emergency-contacts", "ReceiveEmergencyAlert", alertData);

                _logger.LogInformation($"Emergency contact alert sent for case {caseId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send emergency contact alert for case {caseId}");
                throw;
            }
        }

        /// <summary>
        /// Sends real-time case status updates
        /// </summary>
        public async Task SendCaseUpdateAsync(string caseId, string status, string updatedBy)
        {
            try
            {
                var updateData = new
                {
                    Type = "CASE_UPDATE",
                    CaseId = caseId,
                    Status = status,
                    UpdatedBy = updatedBy,
                    Timestamp = DateTime.UtcNow,
                    Message = $"Case {caseId} status updated to: {status} by {updatedBy}"
                };

                await SendToSpecificGroupAsync("case-stakeholders", "ReceiveCaseUpdate", updateData);
                await SendToSpecificGroupAsync("admins", "ReceiveCaseUpdate", updateData);

                _logger.LogInformation($"Case update sent for case {caseId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send case update for case {caseId}");
                throw;
            }
        }

        /// <summary>
        /// Sends notification when person is found
        /// </summary>
        public async Task SendFoundNotificationAsync(string caseId, string individualName, string foundLocation)
        {
            try
            {
                var foundData = new
                {
                    Type = "PERSON_FOUND",
                    CaseId = caseId,
                    IndividualName = individualName,
                    FoundLocation = foundLocation,
                    Timestamp = DateTime.UtcNow,
                    Priority = "SUCCESS",
                    Message = $"✅ GREAT NEWS: {individualName} has been found at {foundLocation}!",
                    Actions = new[]
                    {
                        "Contact family immediately",
                        "Update case status",
                        "Notify law enforcement",
                        "Send media update"
                    }
                };

                // Send to all groups
                await _hubContext.Clients.All.SendAsync("ReceiveFoundNotification", foundData);
                await SendToSpecificGroupAsync("case-stakeholders", "ReceiveFoundNotification", foundData);
                await SendToSpecificGroupAsync("law-enforcement", "ReceiveFoundNotification", foundData);
                await SendToSpecificGroupAsync("media", "ReceiveFoundNotification", foundData);

                _logger.LogInformation($"Found notification sent for case {caseId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send found notification for case {caseId}");
                throw;
            }
        }

        /// <summary>
        /// Sends message to a specific group
        /// </summary>
        public async Task SendToSpecificGroupAsync(string groupName, string method, object data)
        {
            try
            {
                await _hubContext.Clients.Group(groupName).SendAsync(method, data);
                _logger.LogInformation($"Message sent to group {groupName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send message to group {groupName}");
                throw;
            }
        }

        /// <summary>
        /// Adds a connection to a group
        /// </summary>
        public async Task AddToGroupAsync(string connectionId, string groupName)
        {
            try
            {
                await _hubContext.Groups.AddToGroupAsync(connectionId, groupName);
                
                _groupConnections.AddOrUpdate(groupName, 
                    new HashSet<string> { connectionId },
                    (key, existing) =>
                    {
                        existing.Add(connectionId);
                        return existing;
                    });

                _logger.LogInformation($"Connection {connectionId} added to group {groupName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to add connection {connectionId} to group {groupName}");
                throw;
            }
        }

        /// <summary>
        /// Removes a connection from a group
        /// </summary>
        public async Task RemoveFromGroupAsync(string connectionId, string groupName)
        {
            try
            {
                await _hubContext.Groups.RemoveFromGroupAsync(connectionId, groupName);
                
                if (_groupConnections.TryGetValue(groupName, out var connections))
                {
                    connections.Remove(connectionId);
                }

                _logger.LogInformation($"Connection {connectionId} removed from group {groupName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to remove connection {connectionId} from group {groupName}");
                throw;
            }
        }

        /// <summary>
        /// Gets all connections in a group
        /// </summary>
        public HashSet<string> GetGroupConnections(string groupName)
        {
            return _groupConnections.TryGetValue(groupName, out var connections) 
                ? connections 
                : new HashSet<string>();
        }

        /// <summary>
        /// Gets all active groups
        /// </summary>
        public IEnumerable<string> GetActiveGroups()
        {
            return _groupConnections.Keys;
        }
    }


} 