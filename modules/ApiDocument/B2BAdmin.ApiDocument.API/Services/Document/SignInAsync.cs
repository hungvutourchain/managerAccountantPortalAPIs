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
using System.Collections.Generic;
using Mapster;
using System.Data;

namespace B2BAdmin.ApiDocument.Services
{
    public class SignInAsyncRequest : IRequestHandler<SignInAsync, AuthenticateResponseDocument>
    {
        private readonly ApiDocumentDbContext _apiDocumentDbContext;
        private readonly IConfiguration _configuration;
        public SignInAsyncRequest(
            ApiDocumentDbContext apiDocumentDbContext,
            IConfiguration configuration
            )
        {
            _apiDocumentDbContext = apiDocumentDbContext;
            _configuration = configuration;
        }

        public async Task<AuthenticateResponseDocument> Handle(SignInAsync request, CancellationToken cancellationToken)
        {
            if (request == null)
                return null;
            var GentOnlines = await _apiDocumentDbContext.GentOnlines.Find(x => x.Code.ToUpper() == request.passCode.ToUpper())
                 .Project<GentOnlineObject>(
                    Builders<GentOnlineObject>.Projection
                    .Include(x => x.Id)
                )
                .FirstOrDefaultAsync();
            var Data = new ProCodeFilter();
            if (GentOnlines != null)
            {
                var proCode = await _apiDocumentDbContext.ProCodes.Find(x => x.Gents_onlines == GentOnlines.Id && x.Nation == request.country)
                    .Project<ProCodes>(
                        Builders<ProCodes>.Projection
                        .Include(x => x.Id)
                        .Include(x => x.md5code)
                        .Include(x => x.Nation)
                    )
                    .FirstOrDefaultAsync();
                if (proCode != null)
                {
                    Data = proCode.Adapt<ProCodeFilter>();
                }
            }
            else return null;
            var configPages = await _apiDocumentDbContext.configPages.Find(x => x.defaultCountry == request.country)
                 .Project<siteTemplates>(
                    Builders<siteTemplates>.Projection
                    .Include(x => x.Id)
                    .Include(x => x.CurrencyRounding)
                )
                .FirstOrDefaultAsync();
            var user = new UserAdmin();
            user.Id = Data.md5code;
            // return null if user not found
            if (user == null) return new AuthenticateResponseDocument
            (
                new UserAdmin
                {
                    Id = null,
                    
                }, "","Your username or password is incorrect."
            );          
            // authentication successful so generate jwt token
            var token = generateJwtToken(user);
            return new AuthenticateResponseDocument(user, token, "");
        }
        public string generateJwtToken(UserAdmin user)
        {
            // generate token that is valid for 7 days
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JwtSecret"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("Id", user.Id),
                    new Claim("Id", user.Id),
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

    public class SignInAsync : IRequest<AuthenticateResponseDocument>
    {
        public string passCode { get; set; }
        public string country { get; set; }
    }

    public class SignInAsyncValidator : AbstractValidator<SignInAsync>
    {
        public SignInAsyncValidator()
        {
            RuleFor(x => x.passCode)
                .NotEmpty()
                .NotNull();

            RuleFor(x => x.country)
                .NotEmpty()
                .NotNull();
        }
    }
}