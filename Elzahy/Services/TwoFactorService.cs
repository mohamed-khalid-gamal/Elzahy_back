using QRCoder;
using System.Security.Cryptography;
using System.Text;
using OtpNet;

namespace Elzahy.Services
{
    public interface ITwoFactorService
    {
        string GenerateSecret();
        string GenerateCode(string purpose = "Login");
        bool ValidateCode(string inputCode, string storedCode);
        string GenerateQrCodeUrl(string userEmail, string secret, string issuer);
        byte[] GenerateQrCodeImage(string qrCodeUrl);
        bool ValidateTotp(string secret, string code);
        string GenerateTotp(string secret);
        string FormatSecretForDisplay(string secret);
    }

    public class TwoFactorService : ITwoFactorService
    {
        private const int CodeLength = 6;
        private const int CodeValidityMinutes = 5;
        private readonly IConfiguration _configuration;

        public TwoFactorService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateSecret()
        {
            var key = new byte[20];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(key);
            }
            return Base32Encoding.ToString(key);
        }

        public string GenerateCode(string purpose = "Login")
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var bytes = new byte[4];
                rng.GetBytes(bytes);
                var number = Math.Abs(BitConverter.ToInt32(bytes, 0));
                return (number % (int)Math.Pow(10, CodeLength)).ToString().PadLeft(CodeLength, '0');
            }
        }

        public bool ValidateCode(string inputCode, string storedCode)
        {
            return !string.IsNullOrEmpty(inputCode) && 
                   !string.IsNullOrEmpty(storedCode) && 
                   inputCode.Equals(storedCode, StringComparison.OrdinalIgnoreCase);
        }

        public string GenerateQrCodeUrl(string userEmail, string secret, string issuer)
        {
            return $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(userEmail)}?secret={secret}&issuer={Uri.EscapeDataString(issuer)}";
        }

        public byte[] GenerateQrCodeImage(string qrCodeUrl)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(qrCodeUrl, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            return qrCode.GetGraphic(20);
        }

        public bool ValidateTotp(string secret, string code)
        {
            try
            {
                var secretBytes = Base32Encoding.ToBytes(secret);
                var totp = new Totp(secretBytes, step: 30);
                
                // Check current window and previous/next windows for clock skew tolerance
                var currentTime = DateTime.UtcNow;
                for (int i = -1; i <= 1; i++)
                {
                    var timeStep = currentTime.AddSeconds(i * 30);
                    var expectedCode = totp.ComputeTotp(timeStep);
                    if (expectedCode == code)
                        return true;
                }
                
                return false;
            }
            catch
            {
                return false;
            }
        }

        public string GenerateTotp(string secret)
        {
            try
            {
                var secretBytes = Base32Encoding.ToBytes(secret);
                var totp = new Totp(secretBytes, step: 30);
                return totp.ComputeTotp();
            }
            catch
            {
                return string.Empty;
            }
        }

        public string FormatSecretForDisplay(string secret)
        {
            // Format secret in groups of 4 characters for easier manual entry
            var formatted = new StringBuilder();
            for (int i = 0; i < secret.Length; i += 4)
            {
                if (i > 0) formatted.Append(" ");
                formatted.Append(secret.Substring(i, Math.Min(4, secret.Length - i)));
            }
            return formatted.ToString();
        }
    }

    // Helper class for Base32 encoding (keeping for compatibility if needed)
    public static class Base32Helper
    {
        private const string Base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

        public static string ToBase32String(byte[] input)
        {
            if (input == null || input.Length == 0)
                return string.Empty;

            var sb = new StringBuilder();
            var index = 0;
            var digits = 0;
            var currentByte = 0;

            foreach (var b in input)
            {
                currentByte = (currentByte << 8) | b;
                digits += 8;

                while (digits >= 5)
                {
                    var mask = (1 << 5) - 1;
                    sb.Append(Base32Chars[(currentByte >> (digits - 5)) & mask]);
                    digits -= 5;
                }
            }

            if (digits > 0)
            {
                var mask = (1 << 5) - 1;
                sb.Append(Base32Chars[(currentByte << (5 - digits)) & mask]);
            }

            return sb.ToString();
        }

        public static byte[] FromBase32String(string input)
        {
            if (string.IsNullOrEmpty(input))
                return Array.Empty<byte>();

            var output = new List<byte>();
            var accumulator = 0;
            var bits = 0;

            foreach (var c in input.ToUpperInvariant())
            {
                var value = Base32Chars.IndexOf(c);
                if (value < 0) continue;

                accumulator = (accumulator << 5) | value;
                bits += 5;

                if (bits >= 8)
                {
                    output.Add((byte)(accumulator >> (bits - 8)));
                    bits -= 8;
                }
            }

            return output.ToArray();
        }
    }
}