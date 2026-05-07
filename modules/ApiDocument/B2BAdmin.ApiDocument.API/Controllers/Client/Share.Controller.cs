using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using B2BAdmin.ApiDocument.Domains.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;


namespace B2BAdmin.ApiDocument.API.Controllers
{
    public class ShareController : ControllerBase
    {
        public class ItemsTotal<T>
        {
            public long Total { get; set; }
            public double? sumPrice { get; set; }
            public IList<T> Items { get; set; }
        }
        public UserAccess CheckUserAccess()
        {
            var UserAccess = new UserAccess();
            var authHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();
            if (authHeader != null && authHeader.StartsWith("Bearer "))
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

                if (jwtToken != null)
                {
                    var userIdAgency = jwtToken.Claims.First(claim => claim.Type == "IdAgency")?.Value?.ToString();
                    var userID = jwtToken.Claims.First(claim => claim.Type == "Id")?.Value?.ToString();
                    var CurrencyRounding = jwtToken.Claims.First(claim => claim.Type == "CurrencyRounding")?.Value?.ToString();
                    var nation = jwtToken.Claims.First(claim => claim.Type == "Nation")?.Value?.ToString();
                    var passAccess =
                    new UserAccess
                    {
                        CurrencyRounding = CurrencyRounding,
                        IsPass = true,
                        userID = userID,
                        IdAgency = userIdAgency,
                        nation = nation ?? ""
                    };
                    return passAccess;
                } else
                {
                    return new UserAccess
                    {
                        IsPass = false
                    };
                }
            }
            return new UserAccess
            {
                IsPass = false
            };
        }
    }
}
