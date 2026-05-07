using B2BAdmin.ApiDocument.Domains.Models;
using B2BAdmin.ApiDocument.Infrastructure;
using FluentValidation;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace B2BAdmin.ApiDocument.Application
{
    public class GetDocumentMenuAdmin : IRequest<List<DocumentMenu>>
    {}

    public class GetDocumentMenuAdminValidator : AbstractValidator<GetDocumentMenuAdmin>
    {
        public GetDocumentMenuAdminValidator()
        {
        }
    }

    public class GetDocumentMenuAdminHandler : IRequestHandler<GetDocumentMenuAdmin, List<DocumentMenu>>
    {
        private readonly ApiDocumentDbContext _ApiDocumentDbContext;

        public GetDocumentMenuAdminHandler(ApiDocumentDbContext ApiDocumentDbContext)
        {
            _ApiDocumentDbContext = ApiDocumentDbContext;
        }

        public async Task<List<DocumentMenu>> Handle(GetDocumentMenuAdmin request, CancellationToken cancellationToken)
        {
            // DocumentMenus
            var builder = Builders<DocumentMenu>.Filter;
            var filter = builder.Where(x => true);           
            return await _ApiDocumentDbContext.DocumentMenus.Find(filter).ToListAsync();
            
        }
    }
}
