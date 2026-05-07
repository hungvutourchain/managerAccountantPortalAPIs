using B2BAdmin.ApiDocument.Domains.Models;
using B2BAdmin.ApiDocument.Infrastructure;
using MediatR;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace B2BAdmin.ApiDocument.Application
{
    public class GetCountriesQuery : IRequest<IReadOnlyList<lsNation>>
    {
    }

    public class GetCountriesQueryHandler : IRequestHandler<GetCountriesQuery, IReadOnlyList<lsNation>>
    {
        private readonly ApiDocumentDbContext _apiDocumentDbContext;

        public GetCountriesQueryHandler(ApiDocumentDbContext apiDocumentDbContext)
        {
            _apiDocumentDbContext = apiDocumentDbContext;
        }

        public async Task<IReadOnlyList<lsNation>> Handle(GetCountriesQuery request, CancellationToken cancellationToken)
        {
            return await _apiDocumentDbContext.Countries
                .Find(Builders<lsNation>.Filter.Empty)
                .SortBy(x => x.name)
                .ToListAsync(cancellationToken);
        }
    }
}