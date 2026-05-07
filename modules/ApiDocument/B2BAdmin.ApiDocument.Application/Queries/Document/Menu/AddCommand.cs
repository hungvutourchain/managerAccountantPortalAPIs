using B2BAdmin.ApiDocument.Domains.Models;
using B2BAdmin.ApiDocument.Infrastructure;
using FluentValidation;
using Mapster;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace B2BAdmin.ApiDocument.Application
{

    public class AddDocumentMenuCommand : DocumentMenu, IRequest<bool>
    {
        
    }

    public class AddDocumentMenuValidator : AbstractValidator<AddDocumentMenuCommand>
    {
        public AddDocumentMenuValidator()
        {
        }
    }

    public class AddDocumentMenuCommandsHandler : IRequestHandler<AddDocumentMenuCommand, bool>
    {
        private readonly ApiDocumentDbContext _ApiDocumentDbContext;

        public AddDocumentMenuCommandsHandler(ApiDocumentDbContext ApiDocumentDbContext)
        {
            _ApiDocumentDbContext = ApiDocumentDbContext;
        }

        public async Task<bool> Handle(AddDocumentMenuCommand request, CancellationToken cancellationToken)
        {
            var temp = request.Adapt<DocumentMenu>();
            await _ApiDocumentDbContext.DocumentMenus.InsertOneAsync(temp);
            return true;
        }
    }
}
