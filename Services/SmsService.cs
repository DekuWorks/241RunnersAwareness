using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace _241RunnersAwareness.BackendAPI.Services
{
    public interface ISmsService
    {
        Task<bool> SendVerificationCodeAsync(string phoneNumber, string code);
        Task<bool> SendWelcomeMessageAsync(string phoneNumber, string name);
    }

    public class SmsService : ISmsService
    {
        private readonly IConfiguration _configuration;

        public SmsService(IConfiguration configuration)
        {
            _configuration = configuration;
            var accountSid = _configuration["Twilio:AccountSid"];
            var authToken = _configuration["Twilio:AuthToken"];
            
            if (!string.IsNullOrEmpty(accountSid) && !string.IsNullOrEmpty(authToken))
            {
                TwilioClient.Init(accountSid, authToken);
            }
        }

        public async Task<bool> SendVerificationCodeAsync(string phoneNumber, string code)
        {
            try
            {
                var fromNumber = _configuration["Twilio:FromNumber"];
                if (string.IsNullOrEmpty(fromNumber))
                {
                    // For development/testing, just log the code
                    Console.WriteLine($"SMS Verification Code for {phoneNumber}: {code}");
                    return true;
                }

                var message = await MessageResource.CreateAsync(
                    body: $"Your 241 Runners Awareness verification code is: {code}. This code will expire in 10 minutes.",
                    from: new PhoneNumber(fromNumber),
                    to: new PhoneNumber(phoneNumber)
                );

                return message.Status != MessageResource.StatusEnum.Failed;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SMS sending failed: {ex.Message}");
                // For development, still return true to allow testing
                return true;
            }
        }

        public async Task<bool> SendWelcomeMessageAsync(string phoneNumber, string name)
        {
            try
            {
                var fromNumber = _configuration["Twilio:FromNumber"];
                if (string.IsNullOrEmpty(fromNumber))
                {
                    // For development/testing, just log the message
                    Console.WriteLine($"Welcome SMS for {phoneNumber}: Welcome {name} to 241 Runners Awareness!");
                    return true;
                }

                var message = await MessageResource.CreateAsync(
                    body: $"Welcome {name} to 241 Runners Awareness! Your account has been successfully verified.",
                    from: new PhoneNumber(fromNumber),
                    to: new PhoneNumber(phoneNumber)
                );

                return message.Status != MessageResource.StatusEnum.Failed;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SMS sending failed: {ex.Message}");
                // For development, still return true to allow testing
                return true;
            }
        }
    }
} 