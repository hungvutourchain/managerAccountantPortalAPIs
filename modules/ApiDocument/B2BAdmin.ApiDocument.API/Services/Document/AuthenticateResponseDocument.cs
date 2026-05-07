


using B2BAdmin.ApiDocument.Domains.Models;

namespace  B2BAdmin.ApiDocument.Services
{
    public class AuthenticateResponseDocument
    {
        public string Id { get; set; }
        public string? IdUser { get; set; }
        public string Token { get; set; }
        public string Mes { get; set; }
        public AuthenticateResponseDocument(UserAdmin user, string token, string mes)
        {
            IdUser = user!= null && user.Id != null ? user.Id : "";
            Token = token ?? "";
            Mes = mes ?? "";
        }
    }
}