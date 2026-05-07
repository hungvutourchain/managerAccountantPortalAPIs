using MongoDB.Driver;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Configuration;
using B2BAdmin.ApiDocument.Domains.Models;
using B2BAdmin.ApiDocument.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;
using Mapster;

namespace B2BAdmin.ApiDocument.Services
{
    public interface IUserServiceDocument
    {
        //Task<AuthenticateResponse> Authenticate(AuthenticateRequest model);
        Task<UserAdminTourchain> GetByIdDocument(string id);
        Task<UserAdminTourchain> GetAgencyId(string id, string IdAgency);
    }

    public class UserServiceDocument : IUserServiceDocument
    {
        private readonly sqlDbContext _b2bTourchainDbContext;
        private readonly ApiDocumentDbContext _apiDocumentDbContext;
        public UserServiceDocument(
            sqlDbContext dbContext,
            ApiDocumentDbContext apiDocumentDbContext
        )
        {
            _b2bTourchainDbContext = dbContext;
            _apiDocumentDbContext = apiDocumentDbContext;
        }

        public async Task<UserAdminTourchain> GetByIdDocument(string id)
        {
            var UserAdmin = await _apiDocumentDbContext.AdminUsers.Find(x => x.Id == id).FirstOrDefaultAsync();
            return UserAdmin.Adapt<UserAdminTourchain>();
        }

        public async Task<UserAdminTourchain> GetAgencyId(string id, string IdAgency)
        {
            var GentOnlines = await _apiDocumentDbContext.GentOnlines.Find(x => x.Id == IdAgency)
                 .Project<GentOnlineObject>(
                    Builders<GentOnlineObject>.Projection
                    .Include(x => x.Id)
                )
                .FirstOrDefaultAsync();
            var Data = new ProCodeFilter();
            if (GentOnlines != null)
            {
                var Abouts = await _apiDocumentDbContext.ProCodes.Find(x => x.Gents_onlines == GentOnlines.Id && x.md5code == id)
                    .Project<ProCodes>(
                        Builders<ProCodes>.Projection
                        .Include(x => x.Id)
                        .Include(x => x.md5code)
                    )
                    .FirstOrDefaultAsync();
                if (Abouts != null)
                {
                    Data = Abouts.Adapt<ProCodeFilter>();
                }
            }
            var user = new UserAdminTourchain();
            user.Id = Data.md5code;
            user.IdAgency = GentOnlines.Id;
            return user;
        }
    }
}
