using B2BAdmin.ApiDocument.Application;
using MediatR;
using Microsoft.AspNetCore.Mvc;

using System.Threading.Tasks;

namespace BBCAdmin.SupplierEmail.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DocumentController(
            IMediator mediator
            )
        {
            _mediator = mediator;
        }

        [HttpPost("DocumentMenuAdmin")]
        [DocumentAuthorize]
        public async Task<IActionResult> GetDocumentMenuAdminAsync([FromBody] GetDocumentMenuAdmin query)
        {
            var rs = await _mediator.Send(query);
            return Ok(rs);
        }

        [HttpGet("GetDocumentMenu")]
        public async Task<IActionResult> GetDocumentMenuAsync([FromQuery] GetDocumentMenu query)
        {
            var rs = await _mediator.Send(query);
            return Ok(rs);
        }

        [HttpPost("AddDocumentMenu")]
        [DocumentAuthorize]
        public async Task<IActionResult> AddDocumentMenuCommand([FromBody] AddDocumentMenuCommand query)
        {
            var rs = await _mediator.Send(query);
            return Ok(rs);
        }

        [HttpPost("UpdateDocumentMenu")]
        [DocumentAuthorize]
        public async Task<IActionResult> UpdateDocumentMenuAsync([FromBody] UpdateDocumentMenuCommand query)
        {
            var rs = await _mediator.Send(query);
            return Ok(rs);
        }

        [HttpPost("RemoveDocumentMenu")]
        [DocumentAuthorize]
        public async Task<IActionResult> RemoveDocumentMenuAsync([FromBody] RemoveDocumentMenuCommand query)
        {
            var rs = await _mediator.Send(query);
            return Ok(rs);
        }

        [HttpGet("GetDocumentContentBySlug")]
        public async Task<IActionResult> GetDocumentMenuAsync([FromQuery] GetDocumentContentBySlug query)
        {
            var rs = await _mediator.Send(query);
            return Ok(rs);
        }

        [HttpGet("configPage")]
        public async Task<IActionResult> configPageAsync([FromQuery] configPageAsync query)
        {
            var rs = await _mediator.Send(query);
            return Ok(rs);
        }

        [HttpGet("countries")]
        public async Task<IActionResult> GetCountriesAsync()
        {
            var rs = await _mediator.Send(new GetCountriesQuery());
            return Ok(rs);
        }
    }
}
