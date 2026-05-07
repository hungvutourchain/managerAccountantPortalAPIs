using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using B2BAdmin.ApiDocument.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;


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

        public AuthenticateController(IMediator mediator, IConfiguration configuration,
        IUserServiceDocument userService)
        {
            _mediator = mediator;
            _configuration = configuration;
            _userService = userService;
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
    }
}
