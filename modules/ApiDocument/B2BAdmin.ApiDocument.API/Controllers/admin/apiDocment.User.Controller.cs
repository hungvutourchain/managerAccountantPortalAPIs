using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using B2BAdmin.ApiDocument.Infrastructure;
using B2BAdmin.ApiDocument.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;


namespace B2BAdmin.ApiDocument.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthenticateController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IConfiguration _configuration;
        private readonly IUserServiceDocument _userService;
        private readonly ApiDocumentDbContext _db;

        public AuthenticateController(IMediator mediator, IConfiguration configuration,
        IUserServiceDocument userService, ApiDocumentDbContext db)
        {
            _mediator = mediator;
            _configuration = configuration;
            _userService = userService;
            _db = db;
        }
        [HttpPost("authenticate")]
        public async Task<IActionResult> AuthenticateAsync([FromBody] AuthenticateApiDocumentCommand request)
        {
            var rs = await _mediator.Send(request);
            return Ok(rs);
        }
        [DocumentAuthorize]
        [HttpGet("info")]
        public async Task<IActionResult> UserInfoAsync([FromQuery] UserInfoQuery query, [FromHeader] string Authorization)
        {

            var token = Authorization.Substring("Bearer ".Length);
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JwtSecret"]);
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var userId = jwtToken.Claims.First(x => x.Type == "Id").Value.ToString();
            var rs = _userService.GetByIdDocument(userId).Result;
            return Ok(rs);
        }

        [DocumentAuthorize]
        [HttpPut("info")]
        public async Task<IActionResult> UpdateUserInfoAsync([FromBody] UpdateUserInfoRequest request, [FromHeader] string Authorization)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Request body is required" });
            }

            string userId;
            try
            {
                var token = Authorization.Substring("Bearer ".Length);
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["JwtSecret"]);
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                userId = jwtToken.Claims.First(x => x.Type == "Id").Value;
            }
            catch
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var updateDef = Builders<B2BAdmin.ApiDocument.Domains.Models.UserAdmin>.Update
                .Set(u => u.TwoFAGoogle, request.TwoFAGoogle)
                .Set(u => u.TwoFactorEnabled, request.TwoFactorEnabled)
                .Set(u => u.SecretKey, request.SecretKey ?? string.Empty);

            if (!string.IsNullOrWhiteSpace(request.FullName))
                updateDef = updateDef.Set(u => u.FullName, request.FullName.Trim());

            if (!string.IsNullOrWhiteSpace(request.Email))
                updateDef = updateDef.Set(u => u.Email, request.Email.Trim());

            // Only update password if a new hashed value is explicitly provided
            if (!string.IsNullOrWhiteSpace(request.pass))
                updateDef = updateDef.Set(u => u.Password, request.pass.Trim());

            await _db.AdminUsers.UpdateOneAsync(u => u.Id == userId, updateDef);

            return Ok(new { success = true });
        }
    }

    public class UpdateUserInfoRequest
    {
        [JsonPropertyName("twoFAGoogle")]
        public bool TwoFAGoogle { get; set; }

        [JsonPropertyName("twoFactorEnabled")]
        public bool TwoFactorEnabled { get; set; }

        [JsonPropertyName("SecretKey")]
        public string? SecretKey { get; set; }

        [JsonPropertyName("FullName")]
        public string? FullName { get; set; }

        [JsonPropertyName("Email")]
        public string? Email { get; set; }

        [JsonPropertyName("pass")]
        public string? pass { get; set; }
    }
}
