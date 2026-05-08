


using B2BAdmin.ApiDocument.Domains.Models;

namespace  B2BAdmin.ApiDocument.Services
{
    public class AuthenticateResponseDocument
    {
        public string Id { get; set; }
        public string? IdUser { get; set; }
        public string? UserId { get; set; }
        public string Token { get; set; }
        public string Mes { get; set; }
        public bool Requires2FA { get; set; }
        public bool Require2FA { get; set; }
        public bool OtpRequired { get; set; }
        public bool TwoFAGoogle { get; set; }
        public bool RequireGoogle2FA { get; set; }
        public string? TwoFactorType { get; set; }
        public string? TwoFactorChallengeId { get; set; }

        public AuthenticateResponseDocument(UserAdmin user, string token, string mes)
        {
            IdUser = user!= null && user.Id != null ? user.Id : "";
            UserId = IdUser;
            Token = token ?? "";
            Mes = mes ?? "";
            Requires2FA = false;
            Require2FA = false;
            OtpRequired = false;
            TwoFAGoogle = false;
            RequireGoogle2FA = false;
            TwoFactorType = string.Empty;
            TwoFactorChallengeId = string.Empty;
        }
    }
}