
using B2BAdmin.ApiDocument.Domains.Models;
using B2BAdmin.ApiDocument.Infrastructure;
using FluentValidation;
using MediatR;
using MongoDB.Driver;
using System.Threading;
using System.Threading.Tasks;

namespace B2BAdmin.ApiDocument.Application
{

    public class RemoveDocumentMenuCommand : DocumentMenu, IRequest<bool> 
    {
        
    }

    public class RemoveDocumentMenuValidator : AbstractValidator<RemoveDocumentMenuCommand>
    {
        public RemoveDocumentMenuValidator()
        {

        }
    }

    public class RemoveDocumentMenuCommandHandler : IRequestHandler<RemoveDocumentMenuCommand, bool>
    {
        private readonly ApiDocumentDbContext _ApiDocumentDbContext;
        public RemoveDocumentMenuCommandHandler(ApiDocumentDbContext ApiDocumentDbContext)
        {
            _ApiDocumentDbContext = ApiDocumentDbContext;
        }
        public async Task<bool> Handle(RemoveDocumentMenuCommand request, CancellationToken cancellationToken)
        {
            var x = await _ApiDocumentDbContext.DocumentMenus.DeleteOneAsync(x => x.Id == request.Id);
            return x.IsAcknowledged && x.DeletedCount > 0;
        }
    }

}