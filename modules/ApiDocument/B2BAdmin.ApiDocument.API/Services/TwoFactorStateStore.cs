using System;
using System.Collections.Concurrent;

namespace B2BAdmin.ApiDocument.API.Services
{
    public sealed class TwoFactorStateStore
    {
        private readonly ConcurrentDictionary<string, EmailOtpChallenge> _emailOtpChallenges = new ConcurrentDictionary<string, EmailOtpChallenge>();
        private readonly ConcurrentDictionary<string, UserTwoFactorSecret> _userSecrets = new ConcurrentDictionary<string, UserTwoFactorSecret>();
        private readonly ConcurrentDictionary<string, QrLoginChallenge> _qrChallenges = new ConcurrentDictionary<string, QrLoginChallenge>();
        private readonly ConcurrentDictionary<string, SecondFactorPassTicket> _secondFactorPassTickets = new ConcurrentDictionary<string, SecondFactorPassTicket>();
        private readonly ConcurrentDictionary<string, VerifyAttemptState> _verifyAttemptStates = new ConcurrentDictionary<string, VerifyAttemptState>();

        public void SaveEmailOtp(string userId, string email, string otp, TimeSpan ttl)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(otp))
            {
                return;
            }

            _emailOtpChallenges[userId] = new EmailOtpChallenge
            {
                UserId = userId,
                Email = email,
                Otp = otp,
                ExpiresAtUtc = DateTime.UtcNow.Add(ttl)
            };
        }

        public bool VerifyEmailOtp(string userId, string otp)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(otp))
            {
                return false;
            }

            if (!_emailOtpChallenges.TryGetValue(userId, out var challenge))
            {
                return false;
            }

            if (challenge.ExpiresAtUtc < DateTime.UtcNow)
            {
                _emailOtpChallenges.TryRemove(userId, out _);
                return false;
            }

            var isMatch = string.Equals(challenge.Otp, otp, StringComparison.Ordinal);
            if (isMatch)
            {
                _emailOtpChallenges.TryRemove(userId, out _);
            }

            return isMatch;
        }

        public void SaveUserSecret(string userId, string secret, bool enabled)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return;
            }

            if (!enabled || string.IsNullOrWhiteSpace(secret))
            {
                _userSecrets.TryRemove(userId, out _);
                return;
            }

            _userSecrets[userId] = new UserTwoFactorSecret
            {
                UserId = userId,
                Secret = secret,
                Enabled = true,
                UpdatedAtUtc = DateTime.UtcNow
            };
        }

        public string GetUserSecret(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return string.Empty;
            }

            return _userSecrets.TryGetValue(userId, out var state) && state.Enabled
                ? state.Secret
                : string.Empty;
        }

        public void MarkSecondFactorVerified(string userId, TimeSpan ttl)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return;
            }

            _secondFactorPassTickets[userId] = new SecondFactorPassTicket
            {
                UserId = userId,
                ExpiresAtUtc = DateTime.UtcNow.Add(ttl)
            };
        }

        public bool ConsumeSecondFactorVerified(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return false;
            }

            if (!_secondFactorPassTickets.TryGetValue(userId, out var ticket))
            {
                return false;
            }

            if (ticket.ExpiresAtUtc < DateTime.UtcNow)
            {
                _secondFactorPassTickets.TryRemove(userId, out _);
                return false;
            }

            _secondFactorPassTickets.TryRemove(userId, out _);
            return true;
        }

        public (bool locked, int secondsRemaining) IsLockedOut(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return (false, 0);
            }

            if (!_verifyAttemptStates.TryGetValue(userId, out var state))
            {
                return (false, 0);
            }

            if (!state.LockedUntilUtc.HasValue)
            {
                return (false, 0);
            }

            if (state.LockedUntilUtc.Value <= DateTime.UtcNow)
            {
                _verifyAttemptStates.TryRemove(userId, out _);
                return (false, 0);
            }

            var seconds = (int)Math.Ceiling((state.LockedUntilUtc.Value - DateTime.UtcNow).TotalSeconds);
            return (true, Math.Max(seconds, 1));
        }

        public int RegisterFailedAttempt(string userId, int maxAttempts, TimeSpan lockDuration)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return 0;
            }

            var state = _verifyAttemptStates.AddOrUpdate(
                userId,
                _ => new VerifyAttemptState
                {
                    UserId = userId,
                    FailedAttempts = 1,
                    LastFailedAtUtc = DateTime.UtcNow,
                    LockedUntilUtc = maxAttempts <= 1 ? DateTime.UtcNow.Add(lockDuration) : (DateTime?)null,
                },
                (_, current) =>
                {
                    if (current.LockedUntilUtc.HasValue && current.LockedUntilUtc.Value > DateTime.UtcNow)
                    {
                        return current;
                    }

                    current.FailedAttempts += 1;
                    current.LastFailedAtUtc = DateTime.UtcNow;
                    if (current.FailedAttempts >= maxAttempts)
                    {
                        current.LockedUntilUtc = DateTime.UtcNow.Add(lockDuration);
                    }

                    return current;
                });

            return state.FailedAttempts;
        }

        public void RegisterVerifySuccess(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return;
            }

            _verifyAttemptStates.TryRemove(userId, out _);
        }

        public void SaveQrChallenge(string sessionId, string secret)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                return;
            }

            _qrChallenges[sessionId] = new QrLoginChallenge
            {
                SessionId = sessionId,
                Secret = secret,
                CreatedAtUtc = DateTime.UtcNow,
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(5),
                IsApproved = false,
                UserPayload = null,
            };
        }

        public bool TryGetQrChallenge(string sessionId, out QrLoginChallenge challenge)
        {
            challenge = null;
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                return false;
            }

            if (!_qrChallenges.TryGetValue(sessionId, out challenge))
            {
                return false;
            }

            if (challenge.ExpiresAtUtc < DateTime.UtcNow)
            {
                _qrChallenges.TryRemove(sessionId, out _);
                challenge = null;
                return false;
            }

            return true;
        }

        public void ApproveQrChallenge(string sessionId, object userPayload)
        {
            if (TryGetQrChallenge(sessionId, out var challenge))
            {
                challenge.IsApproved = true;
                challenge.UserPayload = userPayload;
            }
        }
    }

    public class EmailOtpChallenge
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string Otp { get; set; }
        public DateTime ExpiresAtUtc { get; set; }
    }

    public class UserTwoFactorSecret
    {
        public string UserId { get; set; }
        public string Secret { get; set; }
        public bool Enabled { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }

    public class QrLoginChallenge
    {
        public string SessionId { get; set; }
        public string Secret { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime ExpiresAtUtc { get; set; }
        public bool IsApproved { get; set; }
        public object UserPayload { get; set; }
    }

    public class SecondFactorPassTicket
    {
        public string UserId { get; set; }
        public DateTime ExpiresAtUtc { get; set; }
    }

    public class VerifyAttemptState
    {
        public string UserId { get; set; }
        public int FailedAttempts { get; set; }
        public DateTime LastFailedAtUtc { get; set; }
        public DateTime? LockedUntilUtc { get; set; }
    }
}
