using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using B2BAdmin.ApiDocument.API.Services;
using B2BAdmin.ApiDocument.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace B2BAdmin.ApiDocument.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ManagerUserController : ControllerBase
    {
        private readonly TwoFactorStateStore _stateStore;
        private readonly ApiDocumentDbContext _apiDocumentDbContext;
        private readonly ILogger<ManagerUserController> _logger;
        private readonly IConfiguration _configuration;
        private const int MaxVerifyAttempts = 5;
        private static readonly TimeSpan VerifyLockDuration = TimeSpan.FromMinutes(10);
        private static readonly TimeSpan SecondFactorPassTtl = TimeSpan.FromMinutes(5);

        public ManagerUserController(
            TwoFactorStateStore stateStore,
            ApiDocumentDbContext apiDocumentDbContext,
            ILogger<ManagerUserController> logger,
            IConfiguration configuration)
        {
            _stateStore = stateStore;
            _apiDocumentDbContext = apiDocumentDbContext;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet("CheckEmailUser")]
        public IActionResult CheckEmailUser([FromQuery] string email, [FromQuery] string nation = "")
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return Ok(false);
            }

            // Minimal validation. Project-specific lookup can be plugged in later.
            return Ok(email.Contains("@", StringComparison.Ordinal));
        }

        [HttpPost("SendEmailVerify")]
        public IActionResult SendEmailVerify([FromBody] SendEmailVerifyRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.UserId) || string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(new { message = "userId and email are required" });
            }

            var code = GenerateNumericCode(4);
            _stateStore.SaveEmailOtp(request.UserId.Trim(), request.Email.Trim(), code, TimeSpan.FromMinutes(30));

            // Email delivery integration point.
            // request.Content can be used with your SMTP provider and replace {{NumberVerify}} with code.

            return Ok(new
            {
                success = true,
                message = "Verification code generated",
                expiresInMinutes = 30
            });
        }

        [HttpPost("userVerify")]
        public IActionResult UserVerifyPost([FromBody] UserVerifyRequest request)
        {
            return VerifyUserInternal(
                request?.UserId,
                request?.NumberVerify,
                request?.IsGoogle ?? false,
                request?.NumberGoogleVerify);
        }

        [HttpGet("userVerify")]
        public IActionResult UserVerify(
            [FromQuery] string userId,
            [FromQuery] string numberVerify,
            [FromQuery] bool isGoogle,
            [FromQuery] string numberGoogleVerify = "")
        {
            return VerifyUserInternal(userId, numberVerify, isGoogle, numberGoogleVerify);
        }

        private IActionResult VerifyUserInternal(
            string userId,
            string numberVerify,
            bool isGoogle,
            string numberGoogleVerify = "")
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Ok(false);
            }

            var normalizedUserId = userId.Trim();
            var lockState = _stateStore.IsLockedOut(normalizedUserId);
            if (lockState.locked)
            {
                _logger.LogWarning("2FA verify blocked due to lockout for user {UserId}. Remaining seconds: {RemainingSeconds}", normalizedUserId, lockState.secondsRemaining);
                return Ok(false);
            }

            if (!isGoogle)
            {
                var validEmailOtp = _stateStore.VerifyEmailOtp(normalizedUserId, (numberVerify ?? string.Empty).Trim());
                if (!validEmailOtp)
                {
                    var failed = _stateStore.RegisterFailedAttempt(normalizedUserId, MaxVerifyAttempts, VerifyLockDuration);
                    _logger.LogWarning("2FA email OTP failed for user {UserId}. Failed attempts: {FailedAttempts}", normalizedUserId, failed);
                    return Ok(false);
                }

                _stateStore.RegisterVerifySuccess(normalizedUserId);
                _stateStore.MarkSecondFactorVerified(normalizedUserId, SecondFactorPassTtl);
                _logger.LogInformation("2FA email OTP verified successfully for user {UserId}", normalizedUserId);
                return Ok(validEmailOtp);
            }

            var secret = _stateStore.GetUserSecret(normalizedUserId);
            if (string.IsNullOrWhiteSpace(secret))
            {
                var user = _apiDocumentDbContext.AdminUsers.Find(x => x.Id == normalizedUserId).FirstOrDefault();
                if (user != null && !string.IsNullOrWhiteSpace(user.SecretKey))
                {
                    secret = user.SecretKey;
                    _stateStore.SaveUserSecret(normalizedUserId, secret, true);
                }
            }

            if (string.IsNullOrWhiteSpace(secret))
            {
                var failed = _stateStore.RegisterFailedAttempt(normalizedUserId, MaxVerifyAttempts, VerifyLockDuration);
                _logger.LogWarning("2FA google OTP failed due to missing secret for user {UserId}. Failed attempts: {FailedAttempts}", normalizedUserId, failed);
                return Ok(false);
            }

            var otp = string.IsNullOrWhiteSpace(numberGoogleVerify) ? numberVerify : numberGoogleVerify;
            var validGoogleOtp = VerifyTotp(secret, otp);

            if (!validGoogleOtp)
            {
                var failed = _stateStore.RegisterFailedAttempt(normalizedUserId, MaxVerifyAttempts, VerifyLockDuration);
                _logger.LogWarning("2FA google OTP failed for user {UserId}. Failed attempts: {FailedAttempts}", normalizedUserId, failed);
                return Ok(false);
            }

            _stateStore.RegisterVerifySuccess(normalizedUserId);
            _stateStore.MarkSecondFactorVerified(normalizedUserId, SecondFactorPassTtl);
            _logger.LogInformation("2FA google OTP verified successfully for user {UserId}", normalizedUserId);

            return Ok(validGoogleOtp);
        }

        [HttpPost("generateQrCode")]
        public IActionResult GenerateQrCode([FromBody] GenerateQrCodeCompatRequest request)
        {
            var email = request?.Email?.Trim();
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(new { message = "email is required" });
            }

            // Delegate format to AuthServiceGoogle contract.
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

        [HttpGet("CheckQRLoginStatus")]
        public IActionResult CheckQrLoginStatus([FromQuery] string qrData)
        {
            if (_stateStore.TryGetQrChallenge(qrData, out var challenge))
            {
                return Ok(new
                {
                    success = true,
                    approved = challenge.IsApproved,
                    user = challenge.IsApproved ? challenge.UserPayload : null,
                });
            }

            return Ok(new { success = false, approved = false, user = (object)null });
        }

        [HttpPost("AuthenticateQR")]
        public IActionResult AuthenticateQr([FromBody] AuthenticateQrRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.QrData))
            {
                return BadRequest(new { message = "qrData is required" });
            }

            _stateStore.ApproveQrChallenge(request.QrData.Trim(), request.Credentials);
            return Ok(new { success = true });
        }

        private static string GenerateNumericCode(int length)
        {
            var min = (int)Math.Pow(10, length - 1);
            var max = (int)Math.Pow(10, length) - 1;
            var value = Random.Shared.Next(min, max + 1);
            return value.ToString();
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

    public class SendEmailVerifyRequest
    {
        public string Content { get; set; }
        public string UserId { get; set; }
        public string Email { get; set; }
    }

    public class GenerateQrCodeCompatRequest
    {
        public string Email { get; set; }
    }

    public class UserVerifyRequest
    {
        public string UserId { get; set; }
        public string NumberVerify { get; set; }
        public bool IsGoogle { get; set; }
        public string NumberGoogleVerify { get; set; }
    }

    public class AuthenticateQrRequest
    {
        public string QrData { get; set; }
        public object Credentials { get; set; }
    }

}
