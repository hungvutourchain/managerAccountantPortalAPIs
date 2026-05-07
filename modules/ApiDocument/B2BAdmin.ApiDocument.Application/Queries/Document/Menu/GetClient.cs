using B2BAdmin.ApiDocument.Domains.Models;
using B2BAdmin.ApiDocument.Infrastructure;
using FluentValidation;
using MediatR;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
namespace B2BAdmin.ApiDocument.Application
{
    public class GetDocumentMenu : IRequest<List<DocumentMenu>>
    {}

    public class GetDocumentMenuValidator : AbstractValidator<GetDocumentMenu>
    {
        public GetDocumentMenuValidator()
        {
        }
    }

    public class GetDocumentMenuHandler : IRequestHandler<GetDocumentMenu, List<DocumentMenu>>
    {
        private readonly ApiDocumentDbContext _ApiDocumentDbContext;

        public GetDocumentMenuHandler(ApiDocumentDbContext ApiDocumentDbContext)
        {
            _ApiDocumentDbContext = ApiDocumentDbContext;
        }

        public async Task<List<DocumentMenu>> Handle(GetDocumentMenu request, CancellationToken cancellationToken)
        {
            // DocumentMenus
            var builder = Builders<DocumentMenu>.Filter;
            var filter = builder.Where(x => x.active == true);          
            var data = await _ApiDocumentDbContext.DocumentMenus.Find(filter).ToListAsync();
            if(data!= null)
            {
                foreach (var item in data)
                {
                    item.children = item.children.Where(x => x.active == true).ToList();
                }
            }
            return data;
        }
    }
}
