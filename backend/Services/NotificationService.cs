using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using System.Linq;

namespace _241RunnersAwareness.BackendAPI.Services
{
    public interface INotificationService
    {
        Task SendUrgentAlertAsync(string caseId, string individualName, string location, string description);
        Task SendNewCaseNotificationAsync(string caseId, string individualName, string reportedBy);
        Task SendCaseUpdateNotificationAsync(string caseId, string status, string updatedBy);
        Task SendEmergencyContactAlertAsync(string contactEmail, string contactPhone, string individualName, string caseId);
        Task SendLawEnforcementAlertAsync(string caseId, string individualName, string location, string specialNeeds);
        Task SendMediaAlertAsync(string caseId, string individualName, string location, string description);
        Task SendFoundNotificationAsync(string caseId, string individualName, string foundLocation);
        Task SendDailyDigestAsync(List<string> caseIds);
    }

    public class NotificationService : INotificationService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<NotificationService> _logger;
        private readonly SendGridClient _sendGridClient;
        private readonly TwilioRestClient _twilioClient;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly string _fromPhone;

        public NotificationService(IConfiguration configuration, ILogger<NotificationService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            
            // Initialize SendGrid
            var sendGridApiKey = _configuration["SendGrid:ApiKey"];
            _sendGridClient = new SendGridClient(sendGridApiKey);
            _fromEmail = _configuration["SendGrid:FromEmail"] ?? "alerts@241runnersawareness.org";
            _fromName = _configuration["SendGrid:FromName"] ?? "241 Runners Awareness";

            // Initialize Twilio
            var accountSid = _configuration["Twilio:AccountSid"];
            var authToken = _configuration["Twilio:AuthToken"];
            _twilioClient = new TwilioRestClient(accountSid, authToken);
            _fromPhone = _configuration["Twilio:FromPhoneNumber"];
        }

        /// <summary>
        /// Sends URGENT real-time alert for missing person cases
        /// This is the most critical notification - sent immediately when a case is reported
        /// </summary>
        public async Task SendUrgentAlertAsync(string caseId, string individualName, string location, string description)
        {
            try
            {
                _logger.LogWarning($"🚨 URGENT ALERT: Case {caseId} - {individualName} reported missing at {location}");

                var subject = $"🚨 URGENT: Missing Person Alert - {individualName}";
                var htmlContent = GenerateUrgentAlertEmail(caseId, individualName, location, description);

                // Send to all emergency contacts, law enforcement, and media
                await SendBulkUrgentNotificationsAsync(subject, htmlContent, caseId);

                // Send SMS alerts for immediate notification
                await SendUrgentSMSAlertsAsync(caseId, individualName, location);

                _logger.LogInformation($"Urgent alert sent successfully for case {caseId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send urgent alert for case {caseId}");
                throw;
            }
        }

        /// <summary>
        /// Sends notification when a new case is created
        /// </summary>
        public async Task SendNewCaseNotificationAsync(string caseId, string individualName, string reportedBy)
        {
            try
            {
                var subject = $"New Case Reported - {individualName}";
                var htmlContent = GenerateNewCaseEmail(caseId, individualName, reportedBy);

                // Send to admins and case managers
                await SendToAdminsAsync(subject, htmlContent);

                _logger.LogInformation($"New case notification sent for case {caseId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send new case notification for case {caseId}");
                throw;
            }
        }

        /// <summary>
        /// Sends notification when case status is updated
        /// </summary>
        public async Task SendCaseUpdateNotificationAsync(string caseId, string status, string updatedBy)
        {
            try
            {
                var subject = $"Case Update - {caseId} - Status: {status}";
                var htmlContent = GenerateCaseUpdateEmail(caseId, status, updatedBy);

                // Send to case stakeholders
                await SendToCaseStakeholdersAsync(caseId, subject, htmlContent);

                _logger.LogInformation($"Case update notification sent for case {caseId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send case update notification for case {caseId}");
                throw;
            }
        }

        /// <summary>
        /// Sends immediate alert to emergency contacts
        /// </summary>
        public async Task SendEmergencyContactAlertAsync(string contactEmail, string contactPhone, string individualName, string caseId)
        {
            try
            {
                var subject = $"🚨 EMERGENCY: {individualName} is missing";
                var htmlContent = GenerateEmergencyContactEmail(individualName, caseId);

                // Send email
                if (!string.IsNullOrEmpty(contactEmail))
                {
                    await SendEmailAsync(contactEmail, subject, htmlContent);
                }

                // Send SMS
                if (!string.IsNullOrEmpty(contactPhone))
                {
                    await SendSMSAsync(contactPhone, $"🚨 EMERGENCY: {individualName} is missing. Case ID: {caseId}. Call 911 immediately.");
                }

                _logger.LogInformation($"Emergency contact alert sent for case {caseId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send emergency contact alert for case {caseId}");
                throw;
            }
        }

        /// <summary>
        /// Sends immediate alert to law enforcement
        /// </summary>
        public async Task SendLawEnforcementAlertAsync(string caseId, string individualName, string location, string specialNeeds)
        {
            try
            {
                var subject = $"🚨 LAW ENFORCEMENT ALERT: Missing Person - {individualName}";
                var htmlContent = GenerateLawEnforcementEmail(caseId, individualName, location, specialNeeds);

                // Send to law enforcement contacts
                await SendToLawEnforcementAsync(subject, htmlContent, caseId);

                _logger.LogInformation($"Law enforcement alert sent for case {caseId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send law enforcement alert for case {caseId}");
                throw;
            }
        }

        /// <summary>
        /// Sends alert to media outlets for public awareness
        /// </summary>
        public async Task SendMediaAlertAsync(string caseId, string individualName, string location, string description)
        {
            try
            {
                var subject = $"MEDIA ALERT: Missing Person - {individualName}";
                var htmlContent = GenerateMediaEmail(caseId, individualName, location, description);

                // Send to media contacts
                await SendToMediaAsync(subject, htmlContent, caseId);

                _logger.LogInformation($"Media alert sent for case {caseId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send media alert for case {caseId}");
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
                var subject = $"✅ FOUND: {individualName} has been located";
                var htmlContent = GenerateFoundEmail(caseId, individualName, foundLocation);

                // Send to all stakeholders
                await SendBulkFoundNotificationsAsync(subject, htmlContent, caseId);

                _logger.LogInformation($"Found notification sent for case {caseId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send found notification for case {caseId}");
                throw;
            }
        }

        /// <summary>
        /// Sends daily digest of all active cases
        /// </summary>
        public async Task SendDailyDigestAsync(List<string> caseIds)
        {
            try
            {
                var subject = $"Daily Case Digest - {DateTime.Now:MM/dd/yyyy}";
                var htmlContent = GenerateDailyDigestEmail(caseIds);

                // Send to admins and case managers
                await SendToAdminsAsync(subject, htmlContent);

                _logger.LogInformation($"Daily digest sent with {caseIds.Count} cases");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send daily digest");
                throw;
            }
        }

        #region Private Methods

        private async Task SendBulkUrgentNotificationsAsync(string subject, string htmlContent, string caseId)
        {
            // Get all emergency contacts, law enforcement, and media contacts
            var recipients = await GetUrgentNotificationRecipientsAsync(caseId);

            foreach (var recipient in recipients)
            {
                try
                {
                    await SendEmailAsync(recipient.Email, subject, htmlContent);
                    
                    if (!string.IsNullOrEmpty(recipient.Phone))
                    {
                        await SendSMSAsync(recipient.Phone, $"🚨 URGENT: Missing person case {caseId}. Check email immediately.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to send urgent notification to {recipient.Email}");
                }
            }
        }

        private async Task SendUrgentSMSAlertsAsync(string caseId, string individualName, string location)
        {
            var message = $"🚨 URGENT: {individualName} reported missing at {location}. Case ID: {caseId}. Call 911 immediately if seen.";
            
            // Send to emergency contacts and law enforcement
            var phoneNumbers = await GetUrgentSMSRecipientsAsync(caseId);
            
            foreach (var phone in phoneNumbers)
            {
                try
                {
                    await SendSMSAsync(phone, message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to send SMS to {phone}");
                }
            }
        }

        private async Task SendEmailAsync(string toEmail, string subject, string htmlContent)
        {
            var from = new EmailAddress(_fromEmail, _fromName);
            var to = new EmailAddress(toEmail);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, "", htmlContent);
            
            var response = await _sendGridClient.SendEmailAsync(msg);
            
            if (response.StatusCode != System.Net.HttpStatusCode.OK && 
                response.StatusCode != System.Net.HttpStatusCode.Accepted)
            {
                throw new Exception($"Failed to send email: {response.StatusCode}");
            }
        }

        private async Task SendSMSAsync(string toPhone, string message)
        {
            var messageResource = await MessageResource.CreateAsync(
                body: message,
                from: new PhoneNumber(_fromPhone),
                to: new PhoneNumber(toPhone)
            );
        }

        private async Task SendToAdminsAsync(string subject, string htmlContent)
        {
            var adminEmails = await GetAdminEmailsAsync();
            
            foreach (var email in adminEmails)
            {
                await SendEmailAsync(email, subject, htmlContent);
            }
        }

        private async Task SendToCaseStakeholdersAsync(string caseId, string subject, string htmlContent)
        {
            var stakeholders = await GetCaseStakeholdersAsync(caseId);
            
            foreach (var stakeholder in stakeholders)
            {
                await SendEmailAsync(stakeholder.Email, subject, htmlContent);
            }
        }

        private async Task SendToLawEnforcementAsync(string subject, string htmlContent, string caseId)
        {
            var lawEnforcementContacts = await GetLawEnforcementContactsAsync();
            
            foreach (var contact in lawEnforcementContacts)
            {
                await SendEmailAsync(contact.Email, subject, htmlContent);
            }
        }

        private async Task SendToMediaAsync(string subject, string htmlContent, string caseId)
        {
            var mediaContacts = await GetMediaContactsAsync();
            
            foreach (var contact in mediaContacts)
            {
                await SendEmailAsync(contact.Email, subject, htmlContent);
            }
        }

        private async Task SendBulkFoundNotificationsAsync(string subject, string htmlContent, string caseId)
        {
            var recipients = await GetFoundNotificationRecipientsAsync(caseId);
            
            foreach (var recipient in recipients)
            {
                await SendEmailAsync(recipient.Email, subject, htmlContent);
            }
        }

        #endregion

        #region Email Templates

        private string GenerateUrgentAlertEmail(string caseId, string individualName, string location, string description)
        {
            return $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background-color: #dc2626; color: white; padding: 20px; text-align: center;'>
                        <h1 style='margin: 0;'>🚨 URGENT MISSING PERSON ALERT</h1>
                    </div>
                    
                    <div style='padding: 20px; background-color: #fef2f2; border-left: 4px solid #dc2626;'>
                        <h2 style='color: #dc2626; margin-top: 0;'>Case ID: {caseId}</h2>
                        <p><strong>Name:</strong> {individualName}</p>
                        <p><strong>Last Seen:</strong> {location}</p>
                        <p><strong>Description:</strong> {description}</p>
                        <p><strong>Reported:</strong> {DateTime.Now:MM/dd/yyyy HH:mm}</p>
                    </div>
                    
                    <div style='padding: 20px; background-color: #f0f9ff; border-left: 4px solid #3b82f6;'>
                        <h3 style='color: #1e40af;'>IMMEDIATE ACTION REQUIRED</h3>
                        <ul>
                            <li>Call 911 immediately if you see this person</li>
                            <li>Do not approach if they appear distressed</li>
                            <li>Contact law enforcement with any information</li>
                            <li>Share this alert on social media</li>
                        </ul>
                    </div>
                    
                    <div style='padding: 20px; text-align: center;'>
                        <p style='color: #6b7280; font-size: 12px;'>
                            This alert is sent by 241 Runners Awareness in memory of Israel Thomas.<br>
                            Every minute counts in finding missing individuals with special needs.
                        </p>
                    </div>
                </div>";
        }

        private string GenerateNewCaseEmail(string caseId, string individualName, string reportedBy)
        {
            return $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background-color: #3b82f6; color: white; padding: 20px; text-align: center;'>
                        <h1 style='margin: 0;'>New Case Reported</h1>
                    </div>
                    
                    <div style='padding: 20px;'>
                        <h2>Case Details</h2>
                        <p><strong>Case ID:</strong> {caseId}</p>
                        <p><strong>Individual:</strong> {individualName}</p>
                        <p><strong>Reported By:</strong> {reportedBy}</p>
                        <p><strong>Reported:</strong> {DateTime.Now:MM/dd/yyyy HH:mm}</p>
                    </div>
                    
                    <div style='padding: 20px; background-color: #f0f9ff;'>
                        <h3>Next Steps</h3>
                        <ul>
                            <li>Review case details</li>
                            <li>Contact emergency contacts</li>
                            <li>Alert law enforcement</li>
                            <li>Prepare media release</li>
                        </ul>
                    </div>
                </div>";
        }

        private string GenerateCaseUpdateEmail(string caseId, string status, string updatedBy)
        {
            return $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background-color: #10b981; color: white; padding: 20px; text-align: center;'>
                        <h1 style='margin: 0;'>Case Status Update</h1>
                    </div>
                    
                    <div style='padding: 20px;'>
                        <h2>Update Details</h2>
                        <p><strong>Case ID:</strong> {caseId}</p>
                        <p><strong>New Status:</strong> {status}</p>
                        <p><strong>Updated By:</strong> {updatedBy}</p>
                        <p><strong>Updated:</strong> {DateTime.Now:MM/dd/yyyy HH:mm}</p>
                    </div>
                </div>";
        }

        private string GenerateEmergencyContactEmail(string individualName, string caseId)
        {
            return $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background-color: #dc2626; color: white; padding: 20px; text-align: center;'>
                        <h1 style='margin: 0;'>🚨 EMERGENCY ALERT</h1>
                    </div>
                    
                    <div style='padding: 20px; background-color: #fef2f2;'>
                        <h2 style='color: #dc2626;'>{individualName} is missing</h2>
                        <p><strong>Case ID:</strong> {caseId}</p>
                        <p><strong>Alert Time:</strong> {DateTime.Now:MM/dd/yyyy HH:mm}</p>
                        
                        <div style='background-color: #fecaca; padding: 15px; border-radius: 5px; margin: 15px 0;'>
                            <h3 style='color: #dc2626; margin-top: 0;'>IMMEDIATE ACTION REQUIRED</h3>
                            <ul>
                                <li>Call 911 immediately</li>
                                <li>Check all known locations</li>
                                <li>Contact other family members</li>
                                <li>Monitor social media</li>
                            </ul>
                        </div>
                    </div>
                </div>";
        }

        private string GenerateLawEnforcementEmail(string caseId, string individualName, string location, string specialNeeds)
        {
            return $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background-color: #1e40af; color: white; padding: 20px; text-align: center;'>
                        <h1 style='margin: 0;'>LAW ENFORCEMENT ALERT</h1>
                    </div>
                    
                    <div style='padding: 20px;'>
                        <h2>Missing Person Details</h2>
                        <p><strong>Case ID:</strong> {caseId}</p>
                        <p><strong>Name:</strong> {individualName}</p>
                        <p><strong>Last Known Location:</strong> {location}</p>
                        <p><strong>Special Needs:</strong> {specialNeeds}</p>
                        <p><strong>Reported:</strong> {DateTime.Now:MM/dd/yyyy HH:mm}</p>
                    </div>
                    
                    <div style='padding: 20px; background-color: #eff6ff;'>
                        <h3>Law Enforcement Action Required</h3>
                        <ul>
                            <li>Issue BOLO (Be On Look Out)</li>
                            <li>Check local hospitals and shelters</li>
                            <li>Monitor traffic cameras</li>
                            <li>Coordinate with other agencies</li>
                        </ul>
                    </div>
                </div>";
        }

        private string GenerateMediaEmail(string caseId, string individualName, string location, string description)
        {
            return $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background-color: #7c3aed; color: white; padding: 20px; text-align: center;'>
                        <h1 style='margin: 0;'>MEDIA ALERT</h1>
                    </div>
                    
                    <div style='padding: 20px;'>
                        <h2>Missing Person Information</h2>
                        <p><strong>Case ID:</strong> {caseId}</p>
                        <p><strong>Name:</strong> {individualName}</p>
                        <p><strong>Last Seen:</strong> {location}</p>
                        <p><strong>Description:</strong> {description}</p>
                        <p><strong>Reported:</strong> {DateTime.Now:MM/dd/yyyy HH:mm}</p>
                    </div>
                    
                    <div style='padding: 20px; background-color: #f3f4f6;'>
                        <h3>Media Contact Information</h3>
                        <p><strong>241 Runners Awareness</strong></p>
                        <p>Email: media@241runnersawareness.org</p>
                        <p>Phone: [Contact number]</p>
                        <p>Website: www.241runnersawareness.org</p>
                    </div>
                </div>";
        }

        private string GenerateFoundEmail(string caseId, string individualName, string foundLocation)
        {
            return $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background-color: #10b981; color: white; padding: 20px; text-align: center;'>
                        <h1 style='margin: 0;'>✅ PERSON FOUND</h1>
                    </div>
                    
                    <div style='padding: 20px; background-color: #f0fdf4;'>
                        <h2 style='color: #059669;'>Great News!</h2>
                        <p><strong>Case ID:</strong> {caseId}</p>
                        <p><strong>Name:</strong> {individualName}</p>
                        <p><strong>Found Location:</strong> {foundLocation}</p>
                        <p><strong>Found:</strong> {DateTime.Now:MM/dd/yyyy HH:mm}</p>
                    </div>
                    
                    <div style='padding: 20px; text-align: center;'>
                        <p style='color: #059669; font-weight: bold;'>
                            Thank you to everyone who helped in the search!
                        </p>
                    </div>
                </div>";
        }

        private string GenerateDailyDigestEmail(List<string> caseIds)
        {
            var caseList = string.Join("", caseIds.Select(id => $"<li>Case ID: {id}</li>"));
            
            return $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background-color: #6b7280; color: white; padding: 20px; text-align: center;'>
                        <h1 style='margin: 0;'>Daily Case Digest</h1>
                        <p>{DateTime.Now:MM/dd/yyyy}</p>
                    </div>
                    
                    <div style='padding: 20px;'>
                        <h2>Active Cases: {caseIds.Count}</h2>
                        <ul>
                            {caseList}
                        </ul>
                    </div>
                    
                    <div style='padding: 20px; background-color: #f9fafb;'>
                        <h3>Summary</h3>
                        <p>Total active cases: {caseIds.Count}</p>
                        <p>Cases requiring immediate attention: {caseIds.Count(c => c.StartsWith("URGENT"))}</p>
                    </div>
                </div>";
        }

        #endregion

        #region Data Access Methods (to be implemented with actual database queries)

        private async Task<List<NotificationRecipient>> GetUrgentNotificationRecipientsAsync(string caseId)
        {
            // TODO: Implement database query to get all emergency contacts, law enforcement, and media contacts
            return new List<NotificationRecipient>
            {
                new NotificationRecipient { Email = "emergency@241runnersawareness.org", Phone = null },
                new NotificationRecipient { Email = "lawenforcement@241runnersawareness.org", Phone = null },
                new NotificationRecipient { Email = "media@241runnersawareness.org", Phone = null }
            };
        }

        private async Task<List<string>> GetUrgentSMSRecipientsAsync(string caseId)
        {
            // TODO: Implement database query to get emergency contact phone numbers
            return new List<string>
            {
                "+1234567890", // Emergency contact
                "+1234567891"  // Law enforcement
            };
        }

        private async Task<List<string>> GetAdminEmailsAsync()
        {
            // TODO: Implement database query to get admin emails
            return new List<string>
            {
                "admin@241runnersawareness.org",
                "lisa@241runnersawareness.org"
            };
        }

        private async Task<List<NotificationRecipient>> GetCaseStakeholdersAsync(string caseId)
        {
            // TODO: Implement database query to get case stakeholders
            return new List<NotificationRecipient>
            {
                new NotificationRecipient { Email = "caseworker@241runnersawareness.org", Phone = null }
            };
        }

        private async Task<List<NotificationRecipient>> GetLawEnforcementContactsAsync()
        {
            // TODO: Implement database query to get law enforcement contacts
            return new List<NotificationRecipient>
            {
                new NotificationRecipient { Email = "police@houston.gov", Phone = null },
                new NotificationRecipient { Email = "sheriff@harriscounty.gov", Phone = null }
            };
        }

        private async Task<List<NotificationRecipient>> GetMediaContactsAsync()
        {
            // TODO: Implement database query to get media contacts
            return new List<NotificationRecipient>
            {
                new NotificationRecipient { Email = "news@khou.com", Phone = null },
                new NotificationRecipient { Email = "news@abc13.com", Phone = null }
            };
        }

        private async Task<List<NotificationRecipient>> GetFoundNotificationRecipientsAsync(string caseId)
        {
            // TODO: Implement database query to get all stakeholders for found notification
            return new List<NotificationRecipient>
            {
                new NotificationRecipient { Email = "family@241runnersawareness.org", Phone = null },
                new NotificationRecipient { Email = "lawenforcement@241runnersawareness.org", Phone = null }
            };
        }

        #endregion
    }

    public class NotificationRecipient
    {
        public string Email { get; set; }
        public string Phone { get; set; }
    }
} 