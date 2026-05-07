using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using B2BAdmin.ApiDocument.Services;
using B2BAdmin.ApiDocument.Domains.Models;

namespace B2BAdmin.ApiDocument.Helpers
{
    public class JwtMiddlewareDocment
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public JwtMiddlewareDocment(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task Invoke(HttpContext context, IUserServiceDocument userService)
        {
            var token = context.Request.Headers["Authorization"]
                .FirstOrDefault()
                ?.Split(" ")
                .Last();

            if (token != null)
                attachUserToContext(context, userService, token);

            await _next(context);
        }

        private void attachUserToContext(
            HttpContext context,
            IUserServiceDocument userService,
            string token
        )
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["JwtSecret"]);
                tokenHandler.ValidateToken(
                    token,
                    new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                        ClockSkew = TimeSpan.Zero
                    },
                    out SecurityToken validatedToken
                );

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userId = jwtToken.Claims.First(x => x.Type == "Id").Value.ToString();
                var IdAgency = jwtToken.Claims.First(x => x.Type == "IdAgency").Value.ToString();
                var User = new UserAdminTourchain();
                if (!string.IsNullOrWhiteSpace(IdAgency) && IdAgency != "false")
                {
                    User = userService.GetAgencyId(userId, IdAgency).Result;
                }
                else
                {
                    User = userService.GetByIdDocument(userId).Result;
                }
                context.Items["UserAdminTourchain"] = User;
                // attach user to context on successful jwt validation
            }
            catch (Exception ex)
            {
                var x = ex;
                // do nothing if jwt validation fails
                // user is not attached to context so request won't have access to secure routes
            }
        }
    }
}
