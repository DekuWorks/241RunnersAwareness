using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace _241RunnersAwareness.BackendAPI.Services
{
    public interface IEmailService
    {
        Task<bool> SendVerificationEmailAsync(string email, string name, string token);
        Task<bool> SendWelcomeEmailAsync(string email, string name);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ISendGridClient _sendGridClient;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
            var apiKey = _configuration["SendGrid:ApiKey"];
            _sendGridClient = new SendGridClient(apiKey);
        }

        public async Task<bool> SendVerificationEmailAsync(string email, string name, string token)
        {
            try
            {
                var from = new EmailAddress(_configuration["SendGrid:FromEmail"] ?? "noreply@241runnersawareness.com", "241 Runners Awareness");
                var to = new EmailAddress(email, name);
                var subject = "Verify Your Email - 241 Runners Awareness";
                
                var verificationUrl = $"{_configuration["App:BaseUrl"]}/verify-email?token={token}";
                
                var htmlContent = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                        <h2 style='color: #333;'>Welcome to 241 Runners Awareness!</h2>
                        <p>Hi {name},</p>
                        <p>Thank you for registering with 241 Runners Awareness. To complete your registration, please verify your email address by clicking the button below:</p>
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{verificationUrl}' style='background-color: #007bff; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; display: inline-block;'>Verify Email</a>
                        </div>
                        <p>If the button doesn't work, you can copy and paste this link into your browser:</p>
                        <p style='word-break: break-all; color: #666;'>{verificationUrl}</p>
                        <p>This link will expire in 24 hours.</p>
                        <p>If you didn't create an account with us, please ignore this email.</p>
                        <hr style='margin: 30px 0; border: none; border-top: 1px solid #eee;'>
                        <p style='color: #666; font-size: 12px;'>241 Runners Awareness Team</p>
                    </div>";

                var msg = MailHelper.CreateSingleEmail(from, to, subject, "", htmlContent);
                var response = await _sendGridClient.SendEmailAsync(msg);
                
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> SendWelcomeEmailAsync(string email, string name)
        {
            try
            {
                var from = new EmailAddress(_configuration["SendGrid:FromEmail"] ?? "noreply@241runnersawareness.com", "241 Runners Awareness");
                var to = new EmailAddress(email, name);
                var subject = "Welcome to 241 Runners Awareness!";
                
                var htmlContent = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                        <h2 style='color: #333;'>Welcome to 241 Runners Awareness!</h2>
                        <p>Hi {name},</p>
                        <p>Your account has been successfully verified and you're now ready to use 241 Runners Awareness!</p>
                        <p>You can now:</p>
                        <ul>
                            <li>Access your profile</li>
                            <li>View and manage your information</li>
                            <li>Update your emergency contacts</li>
                        </ul>
                        <p>If you have any questions or need assistance, please don't hesitate to contact us.</p>
                        <hr style='margin: 30px 0; border: none; border-top: 1px solid #eee;'>
                        <p style='color: #666; font-size: 12px;'>241 Runners Awareness Team</p>
                    </div>";

                var msg = MailHelper.CreateSingleEmail(from, to, subject, "", htmlContent);
                var response = await _sendGridClient.SendEmailAsync(msg);
                
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
} 