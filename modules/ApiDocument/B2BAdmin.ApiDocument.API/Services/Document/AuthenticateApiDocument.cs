using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using B2BAdmin.ApiDocument.Infrastructure;
using B2BAdmin.ApiDocument.Domains.Models;
using Microsoft.EntityFrameworkCore;
using Mapster;
using B2BAdmin.ApiDocument.API.Services;

namespace B2BAdmin.ApiDocument.Services
{
    public class AuthenticateApiDocumentRequest : IRequestHandler<AuthenticateApiDocumentCommand, AuthenticateResponseDocument>
    {
        private readonly ApiDocumentDbContext _apiDocumentDbContext;
        private readonly IConfiguration _configuration;
        private readonly TwoFactorStateStore _twoFactorStateStore;
        public AuthenticateApiDocumentRequest(
            ApiDocumentDbContext dbContext,
            IConfiguration configuration,
            TwoFactorStateStore twoFactorStateStore
            )
        {
            _apiDocumentDbContext = dbContext;
            _configuration = configuration;
            _twoFactorStateStore = twoFactorStateStore;
        }

        public async Task<AuthenticateResponseDocument> Handle(AuthenticateApiDocumentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request == null)
                    return null;
                var passMD5 = MD5Hash(request.Password);
                var UserAdmin = await _apiDocumentDbContext.AdminUsers.Find(x => x.Username == request.Username && x.Password == passMD5).FirstOrDefaultAsync();
                var user = UserAdmin.Adapt<UserAdmin>();
                // return null if user not found
                if (user == null) return new AuthenticateResponseDocument
                (
                    new UserAdmin
                    {
                        Id = null,

                    }, "", "Your username or password is incorrect."
                );
                var requiresGoogle2FA = user.TwoFAGoogle;
                var requiresTwoFactor = user.TwoFactorEnabled || requiresGoogle2FA;

                if (requiresTwoFactor)
                {
                    if (requiresGoogle2FA && !string.IsNullOrWhiteSpace(user.SecretKey))
                    {
                        _twoFactorStateStore.SaveUserSecret(user.Id, user.SecretKey, true);
                    }

                    var secondFactorPassed = _twoFactorStateStore.ConsumeSecondFactorVerified(user.Id);
                    if (!secondFactorPassed)
                    {
                        return new AuthenticateResponseDocument(user, "", "Two-factor authentication required")
                        {
                            Requires2FA = true,
                            Require2FA = true,
                            OtpRequired = true,
                            TwoFAGoogle = requiresGoogle2FA,
                            RequireGoogle2FA = requiresGoogle2FA,
                            TwoFactorType = requiresGoogle2FA ? "google" : "email",
                            TwoFactorChallengeId = Guid.NewGuid().ToString("N")
                        };
                    }
                }

                // authentication successful and 2FA is satisfied, so generate jwt token
                var token = generateJwtToken(user);
                return new AuthenticateResponseDocument(user, token, "");
            }
            catch (Exception ex)
            {
                return null;
            }
            
        }
        public string generateJwtToken(UserAdmin user)
        {
            // generate token that is valid for 7 days
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JwtSecret"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("Id", user.Id),
                    new Claim("IdAgency", "false"),
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        public string MD5Hash(string input)
        {
            var hash = new StringBuilder();
            var md5provider = new MD5CryptoServiceProvider();
            byte[] bytes = md5provider.ComputeHash(new UTF8Encoding().GetBytes(input));

            for (int i = 0; i < bytes.Length; i++)
            {
                hash.Append(bytes[i].ToString("x2"));
            }
            return hash.ToString();
        }
    }

    public class AuthenticateApiDocumentCommand : IRequest<AuthenticateResponseDocument>
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class AuthenticateApiDocumentValidator : AbstractValidator<AuthenticateApiDocumentCommand>
    {
        public AuthenticateApiDocumentValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty()
                .NotNull();

            RuleFor(x => x.Password)
                .NotEmpty()
                .NotNull();
        }
    }
}