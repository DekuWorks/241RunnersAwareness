using Microsoft.EntityFrameworkCore;
using _241RunnersAPI.Data;
using _241RunnersAPI.Models;

namespace _241RunnersAPI.Services
{
    /// <summary>
    /// Background service to send photo update reminders every 6 months
    /// </summary>
    public class PhotoUpdateNotificationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PhotoUpdateNotificationService> _logger;
        private readonly TimeSpan _period = TimeSpan.FromHours(24); // Check daily

        public PhotoUpdateNotificationService(
            IServiceProvider serviceProvider,
            ILogger<PhotoUpdateNotificationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Photo Update Notification Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckAndSendPhotoUpdateReminders();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Photo Update Notification Service");
                }

                await Task.Delay(_period, stoppingToken);
            }

            _logger.LogInformation("Photo Update Notification Service stopped");
        }

        private async Task CheckAndSendPhotoUpdateReminders()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            try
            {
                // Find runners who need photo update reminders
                var runnersNeedingReminders = await context.Runners
                    .Where(r => r.NextPhotoReminder.HasValue && 
                               DateTime.UtcNow >= r.NextPhotoReminder.Value && 
                               !r.PhotoUpdateReminderSent &&
                               r.IsActive)
                    .Include(r => r.User)
                    .ToListAsync();

                _logger.LogInformation("Found {Count} runners needing photo update reminders", runnersNeedingReminders.Count);

                foreach (var runner in runnersNeedingReminders)
                {
                    try
                    {
                        // Send notification
                        await notificationService.SendPhotoUpdateReminderAsync(runner.UserId, runner.Id, runner.Name);

                        // Mark reminder as sent
                        runner.PhotoUpdateReminderSent = true;
                        runner.PhotoUpdateReminderCount++;
                        runner.UpdatedAt = DateTime.UtcNow;

                        _logger.LogInformation("Photo update reminder sent to user {UserId} for runner {RunnerId} ({RunnerName})", 
                            runner.UserId, runner.Id, runner.Name);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send photo update reminder to user {UserId} for runner {RunnerId}", 
                            runner.UserId, runner.Id);
                    }
                }

                // Save changes
                if (runnersNeedingReminders.Any())
                {
                    await context.SaveChangesAsync();
                    _logger.LogInformation("Updated {Count} runners with photo reminder status", runnersNeedingReminders.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking photo update reminders");
            }
        }
    }

    /// <summary>
    /// Enhanced notification service interface
    /// </summary>
    public interface INotificationService
    {
        Task SendPhotoUpdateNotificationAsync(int userId, int runnerId, int photoCount);
        Task SendPhotoUpdateReminderAsync(int userId, int runnerId, string runnerName);
    }

    /// <summary>
    /// Enhanced notification service implementation
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IPushNotificationService _pushNotificationService;

        public NotificationService(
            ILogger<NotificationService> logger, 
            ApplicationDbContext context,
            IEmailService emailService,
            IPushNotificationService pushNotificationService)
        {
            _logger = logger;
            _context = context;
            _emailService = emailService;
            _pushNotificationService = pushNotificationService;
        }

        public async Task SendPhotoUpdateNotificationAsync(int userId, int runnerId, int photoCount)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User {UserId} not found for photo update notification", userId);
                    return;
                }

                // Send email notification
                var emailSubject = "Runner Photos Updated Successfully";
                var emailBody = $@"
                    <h2>Runner Photos Updated</h2>
                    <p>Hello {user.FirstName},</p>
                    <p>Your runner profile photos have been successfully updated. {photoCount} new photo(s) have been uploaded.</p>
                    <p>Your next photo update reminder is scheduled for 6 months from now to keep your photos fresh and accurate.</p>
                    <p>Thank you for keeping your runner profile up to date!</p>
                    <p>Best regards,<br>241 Runners Awareness Team</p>";

                await _emailService.SendEmailAsync(user.Email, emailSubject, emailBody);

                // Send push notification
                await _pushNotificationService.SendNotificationAsync(userId, "Photo Update", 
                    $"Successfully uploaded {photoCount} new photo(s) to your runner profile");

                _logger.LogInformation("Photo update notification sent to user {UserEmail}", user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending photo update notification to user {UserId}", userId);
            }
        }

        public async Task SendPhotoUpdateReminderAsync(int userId, int runnerId, string runnerName)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User {UserId} not found for photo update reminder", userId);
                    return;
                }

                // Send email reminder
                var emailSubject = "Action Required: Update Your Runner Photos";
                var emailBody = $@"
                    <h2>Time to Update Your Runner Photos</h2>
                    <p>Hello {user.FirstName},</p>
                    <p>It's been 6 months since your last photo update for your runner profile: <strong>{runnerName}</strong></p>
                    <p>To keep your photos fresh and accurate for case tracking, please update your runner photos:</p>
                    <ul>
                        <li>Take new photos that clearly show your current appearance</li>
                        <li>Include both face and full-body photos</li>
                        <li>Ensure photos are well-lit and high quality</li>
                        <li>Update any changes in appearance (hair, weight, etc.)</li>
                    </ul>
                    <p><strong>Why is this important?</strong><br>
                    Fresh photos help emergency responders and community members identify you accurately if needed.</p>
                    <p>Please log into your account and update your photos as soon as possible.</p>
                    <p>Thank you for helping keep our community safe!</p>
                    <p>Best regards,<br>241 Runners Awareness Team</p>";

                await _emailService.SendEmailAsync(user.Email, emailSubject, emailBody);

                // Send push notification
                await _pushNotificationService.SendNotificationAsync(userId, "Photo Update Required", 
                    "It's time to update your runner photos - 6 months have passed since your last update");

                _logger.LogInformation("Photo update reminder sent to user {UserEmail} for runner {RunnerName}", user.Email, runnerName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending photo update reminder to user {UserId}", userId);
            }
        }
    }

    /// <summary>
    /// Email service interface
    /// </summary>
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }

    /// <summary>
    /// Email service implementation (placeholder - integrate with your email provider)
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                // TODO: Integrate with your email provider (SendGrid, SMTP, etc.)
                // For now, just log the email
                _logger.LogInformation("Email sent to {To}: {Subject}", to, subject);
                _logger.LogDebug("Email body: {Body}", body);

                // Simulate async operation
                await Task.Delay(100);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To}", to);
                throw;
            }
        }
    }

    /// <summary>
    /// Push notification service interface
    /// </summary>
    public interface IPushNotificationService
    {
        Task SendNotificationAsync(int userId, string title, string message);
    }

    /// <summary>
    /// Push notification service implementation (placeholder - integrate with your push provider)
    /// </summary>
    public class PushNotificationService : IPushNotificationService
    {
        private readonly ILogger<PushNotificationService> _logger;

        public PushNotificationService(ILogger<PushNotificationService> logger)
        {
            _logger = logger;
        }

        public async Task SendNotificationAsync(int userId, string title, string message)
        {
            try
            {
                // TODO: Integrate with your push notification provider (Firebase, Azure Notification Hub, etc.)
                // For now, just log the notification
                _logger.LogInformation("Push notification sent to user {UserId}: {Title} - {Message}", userId, title, message);

                // Simulate async operation
                await Task.Delay(100);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send push notification to user {UserId}", userId);
                throw;
            }
        }
    }
}
