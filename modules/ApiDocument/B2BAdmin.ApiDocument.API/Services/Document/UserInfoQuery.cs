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
    public class UserInfoQuery : IRequest<UserAdmin>
    {
        public string? Id { get; set; }
    }

    public class GetlistServiceSuplierValidator : AbstractValidator<UserInfoQuery>
    {
        public GetlistServiceSuplierValidator()
        {
        }
    }

    public class GetlistServiceSuplierCommandHandler : IRequestHandler<UserInfoQuery, UserAdmin>
    {
        private readonly sqlDbContext _b2bTourchainDbContext;

        public GetlistServiceSuplierCommandHandler(
             sqlDbContext dbContext)
        {
            _b2bTourchainDbContext = dbContext;
        }

        public async Task<UserAdmin> Handle(UserInfoQuery request, CancellationToken cancellationToken)
        {
            var UserAdmin =  await _b2bTourchainDbContext.UserAdmins.FirstOrDefaultAsync(x => x.Id == request.Id);
            return UserAdmin.Adapt<UserAdmin>();
        }
    }
}
