using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace _241RunnersAwareness.BackendAPI.Services
{
    public interface ITwoFactorService
    {
        string GenerateSecret();
        string GenerateQrCodeUrl(string email, string secret, string issuer = "241 Runners Awareness");
        bool ValidateTotp(string secret, string totp);
        string GenerateBackupCodes();
        bool ValidateBackupCode(string backupCodes, string code);
        string RemoveUsedBackupCode(string backupCodes, string usedCode);
    }

    public class TwoFactorService : ITwoFactorService
    {
        private const int TotpDigits = 6;
        private const int TotpPeriod = 30; // 30 seconds
        private const int BackupCodeLength = 8;
        private const int BackupCodeCount = 10;

        public string GenerateSecret()
        {
            var random = new byte[20];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(random);
            }
            return Convert.ToBase64String(random);
        }

        public string GenerateQrCodeUrl(string email, string secret, string issuer = "241 Runners Awareness")
        {
            var encodedIssuer = Uri.EscapeDataString(issuer);
            var encodedEmail = Uri.EscapeDataString(email);
            var encodedSecret = Uri.EscapeDataString(secret);
            
            return $"otpauth://totp/{encodedIssuer}:{encodedEmail}?secret={encodedSecret}&issuer={encodedIssuer}&algorithm=SHA1&digits={TotpDigits}&period={TotpPeriod}";
        }

        public bool ValidateTotp(string secret, string totp)
        {
            if (string.IsNullOrEmpty(totp) || totp.Length != TotpDigits)
                return false;

            var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var timeStep = currentTime / TotpPeriod;

            // Check current time step and adjacent ones (for clock skew)
            for (int i = -1; i <= 1; i++)
            {
                var checkTime = timeStep + i;
                var expectedTotp = GenerateTotp(secret, checkTime);
                if (totp == expectedTotp)
                    return true;
            }

            return false;
        }

        private string GenerateTotp(string secret, long timeStep)
        {
            var timeBytes = BitConverter.GetBytes(timeStep);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(timeBytes);

            var secretBytes = Convert.FromBase64String(secret);
            using (var hmac = new HMACSHA1(secretBytes))
            {
                var hash = hmac.ComputeHash(timeBytes);
                var offset = hash[hash.Length - 1] & 0xf;
                var binary = ((hash[offset] & 0x7f) << 24) |
                           ((hash[offset + 1] & 0xff) << 16) |
                           ((hash[offset + 2] & 0xff) << 8) |
                           (hash[offset + 3] & 0xff);

                var totp = binary % (int)Math.Pow(10, TotpDigits);
                return totp.ToString().PadLeft(TotpDigits, '0');
            }
        }

        public string GenerateBackupCodes()
        {
            var codes = new List<string>();
            using (var rng = RandomNumberGenerator.Create())
            {
                for (int i = 0; i < BackupCodeCount; i++)
                {
                    var bytes = new byte[BackupCodeLength];
                    rng.GetBytes(bytes);
                    var code = Convert.ToBase64String(bytes)
                        .Replace("+", "")
                        .Replace("/", "")
                        .Replace("=", "")
                        .Substring(0, BackupCodeLength);
                    codes.Add(code);
                }
            }
            return JsonSerializer.Serialize(codes);
        }

        public bool ValidateBackupCode(string backupCodes, string code)
        {
            if (string.IsNullOrEmpty(backupCodes) || string.IsNullOrEmpty(code))
                return false;

            try
            {
                var codes = JsonSerializer.Deserialize<List<string>>(backupCodes);
                return codes?.Contains(code) ?? false;
            }
            catch
            {
                return false;
            }
        }

        public string RemoveUsedBackupCode(string backupCodes, string usedCode)
        {
            if (string.IsNullOrEmpty(backupCodes))
                return backupCodes;

            try
            {
                var codes = JsonSerializer.Deserialize<List<string>>(backupCodes);
                codes?.Remove(usedCode);
                return codes != null ? JsonSerializer.Serialize(codes) : "[]";
            }
            catch
            {
                return backupCodes;
            }
        }
    }
} 