using System.Threading.Tasks;
using B2BAdmin.ApiDocument.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;


namespace B2BAdmin.ApiDocument.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IConfiguration _configuration;
        private readonly IUserServiceDocument _userService;

        public AuthController(IMediator mediator, IConfiguration configuration,
        IUserServiceDocument userService)
        {
            _mediator = mediator;
            _configuration = configuration;
            _userService = userService;
        }
        [HttpPost("sign-in")]
        public async Task<IActionResult> SignInAsync([FromBody] SignInAsync request)
        {
            var rs = await _mediator.Send(request);
            return Ok(rs);
        }
    }
}
