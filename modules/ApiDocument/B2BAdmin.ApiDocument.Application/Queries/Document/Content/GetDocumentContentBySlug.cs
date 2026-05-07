using B2BAdmin.ApiDocument.Domains.Models;
using B2BAdmin.ApiDocument.Infrastructure;
using FluentValidation;
using MediatR;
using MongoDB.Driver;
using System.Threading;
using System.Threading.Tasks;

namespace B2BAdmin.ApiDocument.Application
{
    public class GetDocumentContentBySlug : IRequest<DocumentMenuchildren>
    {
        public string? url { get; set; }
    }

    public class GetDocumentContentBySlugValidator : AbstractValidator<GetDocumentContentBySlug>
    {
        public GetDocumentContentBySlugValidator()
        {
        }
    }

    public class GetDocumentContentBySlugHandler : IRequestHandler<GetDocumentContentBySlug, DocumentMenuchildren>
    {
        private readonly ApiDocumentDbContext _ApiDocumentDbContext;

        public GetDocumentContentBySlugHandler(ApiDocumentDbContext ApiDocumentDbContext)
        {
            _ApiDocumentDbContext = ApiDocumentDbContext;
        }

        public async Task<DocumentMenuchildren> Handle(GetDocumentContentBySlug request, CancellationToken cancellationToken)
        {
            // DocumentMenus
            var builder = Builders<DocumentMenuchildren>.Filter;
            var filter = builder.Where(x => x.active == true && x.url == request.url);
            return await _ApiDocumentDbContext.Api_document_menus_view_clients.Find(filter).FirstOrDefaultAsync();
        }
    }
}
