
using B2BAdmin.ApiDocument.Domains.Models;
using B2BAdmin.ApiDocument.Infrastructure;
using FluentValidation;
using Mapster;
using MediatR;
using MongoDB.Driver;
using System.Threading;
using System.Threading.Tasks;

namespace B2BAdmin.ApiDocument.Application
{

    public class UpdateDocumentMenuCommand : DocumentMenu, IRequest<bool> 
    {
      
    }

    public class UpdateDocumentMenuValidator : AbstractValidator<UpdateDocumentMenuCommand>
    {
        public UpdateDocumentMenuValidator()
        {
        }
    }
    public class UpdateDocumentMenuCommandHandler : IRequestHandler<UpdateDocumentMenuCommand, bool>
    {
        private readonly ApiDocumentDbContext _ApiDocumentDbContext;

        public UpdateDocumentMenuCommandHandler(ApiDocumentDbContext ApiDocumentDbContext)
        {
            _ApiDocumentDbContext = ApiDocumentDbContext;
        }

        public async Task<bool> Handle(UpdateDocumentMenuCommand request, CancellationToken cancellationToken)
        {
            var temp = request.Adapt<DocumentMenu>();
            var x = await _ApiDocumentDbContext.DocumentMenus.ReplaceOneAsync(x => x.Id == request.Id, temp);
            return x.IsAcknowledged;
        }
    }
}