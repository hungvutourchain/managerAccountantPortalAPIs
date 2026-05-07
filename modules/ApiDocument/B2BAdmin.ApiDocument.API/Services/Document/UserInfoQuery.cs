using B2BAdmin.ApiDocument.Domains.Models;
using B2BAdmin.ApiDocument.Infrastructure;
using FluentValidation;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Mapster;

namespace B2BAdmin.ApiDocument.Services
{
    public class UserInfoQuery : IRequest<UserAdminTourchain>
    {
        public string? Id { get; set; }
    }

    public class GetlistServiceSuplierValidator : AbstractValidator<UserInfoQuery>
    {
        public GetlistServiceSuplierValidator()
        {
        }
    }

    public class GetlistServiceSuplierCommandHandler : IRequestHandler<UserInfoQuery, UserAdminTourchain>
    {
        private readonly sqlDbContext _b2bTourchainDbContext;

        public GetlistServiceSuplierCommandHandler(
             sqlDbContext dbContext)
        {
            _b2bTourchainDbContext = dbContext;
        }

        public async Task<UserAdminTourchain> Handle(UserInfoQuery request, CancellationToken cancellationToken)
        {
            var UserAdmin =  await _b2bTourchainDbContext.UserAdminTourchains.FirstOrDefaultAsync(x => x.Id == request.Id);
            return UserAdmin.Adapt<UserAdminTourchain>();
        }
    }
}
