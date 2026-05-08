using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using B2BAdmin.ApiDocument.Domains.Models;
using B2BAdmin.ApiDocument.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace B2BAdmin.ApiDocument.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthServiceGoogleController : ControllerBase
    {
        private readonly TwoFactorStateStore _stateStore;
        private readonly IConfiguration _configuration;

        public AuthServiceGoogleController(TwoFactorStateStore stateStore, IConfiguration configuration)
        {
            _stateStore = stateStore;
            _configuration = configuration;
        }

        [HttpPost("generateQrCode")]
        public IActionResult GenerateQrCode([FromBody] GenerateQrCodeRequest request)
        {
            var email = request?.Email?.Trim();
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(new { message = "email is required" });
            }

            var secret = GenerateBase32Secret();
            var issuer = (_configuration["AccountantPortalIssuer"] ?? "Accountant Portal").Trim();
            var otpauth = $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(email)}?secret={secret}&issuer={Uri.EscapeDataString(issuer)}&algorithm=SHA1&digits=6&period=30";
            var qrUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=220x220&data={Uri.EscapeDataString(otpauth)}";

            return Ok(new
            {
                qrCodeBase64 = qrUrl,
                secretKey = secret,
                otpauthUrl = otpauth
            });
        }

        [HttpGet("generateQrCode")]
        public IActionResult GenerateQrCodeByQuery([FromQuery] string email)
        {
            return GenerateQrCode(new GenerateQrCodeRequest { Email = email });
        }

        [HttpPost("validateTotp")]
        public IActionResult ValidateTotp([FromBody] ValidateTotpRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Secret) || string.IsNullOrWhiteSpace(request.Otp))
            {
                return BadRequest(new { valid = false, message = "secret and otp are required" });
            }

            var valid = VerifyTotp(request.Secret.Trim(), request.Otp.Trim());
            return Ok(new { valid, success = valid });
        }

        [HttpPost("userVerify")]
        public IActionResult UserVerify([FromBody] UserVerifyGoogleRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Otp))
            {
                return Ok(false);
            }

            var providedOtp = request.Otp.Trim();
            var secret = (request.Key ?? string.Empty).Trim();

            // Backward compatibility: key can be userId instead of raw secret.
            if (!LooksLikeBase32(secret))
            {
                secret = _stateStore.GetUserSecret(secret);
            }

            if (string.IsNullOrWhiteSpace(secret))
            {
                return Ok(false);
            }

            return Ok(VerifyTotp(secret, providedOtp));
        }

        [HttpPost("bind-user-secret")]
        [DocumentAuthorize]
        public IActionResult BindUserSecret([FromBody] BindUserSecretRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.UserId))
            {
                return BadRequest(new { message = "userId is required" });
            }

            var currentUser = HttpContext?.Items?["UserAdmin"] as UserAdmin;
            var currentUserId = currentUser?.Id?.Trim();
            var requestedUserId = request.UserId.Trim();

            if (string.IsNullOrWhiteSpace(currentUserId) || !string.Equals(currentUserId, requestedUserId, StringComparison.Ordinal))
            {
                return Unauthorized(new { message = "Forbidden" });
            }

            _stateStore.SaveUserSecret(requestedUserId, request.Secret?.Trim(), request.Enabled);
            return Ok(new { success = true });
        }

        private static bool VerifyTotp(string secret, string otp)
        {
            if (string.IsNullOrWhiteSpace(secret) || string.IsNullOrWhiteSpace(otp) || otp.Length != 6 || !otp.All(char.IsDigit))
            {
                return false;
            }

            var key = Base32Decode(secret);
            if (key.Length == 0)
            {
                return false;
            }

            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 30;

            // Allow small clock drift: previous/current/next window.
            for (var offset = -1L; offset <= 1; offset++)
            {
                var code = ComputeTotpCode(key, timestamp + offset);
                if (code == otp)
                {
                    return true;
                }
            }

            return false;
        }

        private static string ComputeTotpCode(byte[] key, long timestep)
        {
            var counterBytes = BitConverter.GetBytes(timestep);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(counterBytes);
            }

            using var hmac = new HMACSHA1(key);
            var hash = hmac.ComputeHash(counterBytes);
            var offset = hash[^1] & 0x0F;
            var binaryCode = ((hash[offset] & 0x7F) << 24)
                | ((hash[offset + 1] & 0xFF) << 16)
                | ((hash[offset + 2] & 0xFF) << 8)
                | (hash[offset + 3] & 0xFF);

            var otp = binaryCode % 1_000_000;
            return otp.ToString("D6");
        }

        private static string GenerateBase32Secret(int length = 32)
        {
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
            var bytes = RandomNumberGenerator.GetBytes(length);
            var sb = new StringBuilder(length);
            foreach (var b in bytes)
            {
                sb.Append(alphabet[b % alphabet.Length]);
            }
            return sb.ToString();
        }

        private static bool LooksLikeBase32(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            return value.All(ch => (ch >= 'A' && ch <= 'Z') || (ch >= '2' && ch <= '7') || ch == '=');
        }

        private static byte[] Base32Decode(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return Array.Empty<byte>();
            }

            var cleaned = input.Trim().TrimEnd('=').ToUpperInvariant();
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
            var output = new byte[cleaned.Length * 5 / 8];

            var buffer = 0;
            var bitsLeft = 0;
            var index = 0;

            foreach (var c in cleaned)
            {
                var val = alphabet.IndexOf(c);
                if (val < 0)
                {
                    return Array.Empty<byte>();
                }

                buffer = (buffer << 5) | val;
                bitsLeft += 5;

                if (bitsLeft >= 8)
                {
                    output[index++] = (byte)((buffer >> (bitsLeft - 8)) & 0xFF);
                    bitsLeft -= 8;
                }
            }

            return output;
        }
    }

    public class GenerateQrCodeRequest
    {
        public string Email { get; set; }
    }

    public class ValidateTotpRequest
    {
        public string Secret { get; set; }
        public string Otp { get; set; }
    }

    public class UserVerifyGoogleRequest
    {
        public string Key { get; set; }
        public string Otp { get; set; }
    }

    public class BindUserSecretRequest
    {
        public string UserId { get; set; }
        public string Secret { get; set; }
        public bool Enabled { get; set; }
    }
}
