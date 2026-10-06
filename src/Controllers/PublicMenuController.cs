using api_custo_prato.src.Interfaces;
using api_custo_prato.src.Models.Base;
using Microsoft.AspNetCore.Mvc;

namespace api_custo_prato.src.Controllers
{
    [Route("api/public/menu")]
    [ApiController]
    public class PublicMenuController(IPublicMenuService service) : ControllerBase
    {
        [HttpGet("{slug}")]
        public async Task<IActionResult> Get(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug)) return BadRequest("Slug inválido.");
            ResponseApi<dynamic?> response = await service.GetMenuAsync(slug);
            return StatusCode(response.StatusCode, response.Result);
        }
    }
}
