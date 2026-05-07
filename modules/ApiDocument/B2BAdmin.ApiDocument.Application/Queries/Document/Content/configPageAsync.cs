using B2BAdmin.ApiDocument.Domains.Models;
using B2BAdmin.ApiDocument.Infrastructure;
using FluentValidation;
using MediatR;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace B2BAdmin.ApiDocument.Application
{
    public class configPageAsync : IRequest<siteTemplates>
    {
        public string? domain { get; set; }
    }

    public class configPageAsyncValidator : AbstractValidator<configPageAsync>
    {
        public configPageAsyncValidator()
        {
        }
    }

    public class configPageAsyncHandler : IRequestHandler<configPageAsync, siteTemplates>
    {
        private readonly ApiDocumentDbContext _ApiDocumentDbContext;

        public configPageAsyncHandler(ApiDocumentDbContext ApiDocumentDbContext)
        {
            _ApiDocumentDbContext = ApiDocumentDbContext;
        }

        public async Task<siteTemplates> Handle(configPageAsync request, CancellationToken cancellationToken)
        {
            // DocumentMenus
            var builder = Builders<siteTemplates>.Filter;
            var filter = builder.Where(x => true);
            return await _ApiDocumentDbContext.configPages.Find(filter)
                // .Project<siteTemplates>(
                //    Builders<siteTemplates>.Projection
                //    .Include(x => x.Id)
                //    .Include(x => x.imageLogo)
                //    .Include(x => x.header)
                //    .Include(x => x.nameCompany)
                //)
                 .FirstOrDefaultAsync();
        }
    }
}
