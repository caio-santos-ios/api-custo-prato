using api_custo_prato.src.Interfaces;
using api_custo_prato.src.Models;
using api_custo_prato.src.Models.Base;
using api_custo_prato.src.Requests.Stores;
using api_custo_prato.src.Shared.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api_custo_prato.src.Controllers
{
    [Route("api/store")]
    [ApiController]
    [Authorize]
    public class StoreController(IStoreService service) : ControllerBase
    {
        private string UserId => User.GetUserId();

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            ResponseApi<Store?> response = await service.GetAsync(UserId);
            return StatusCode(response.StatusCode, response.Result);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateStoreRequest request)
        {
            if (request == null) return BadRequest("Dados inválidos.");
            ResponseApi<Store?> response = await service.UpdateAsync(request, UserId);
            return StatusCode(response.StatusCode, response.Result);
        }

        [HttpPost("logo")]
        public async Task<IActionResult> UploadLogo([FromForm] IFormFile file)
        {
            if (file == null) return BadRequest("Nenhum arquivo enviado.");
            ResponseApi<Store?> response = await service.UploadLogoAsync(file, UserId);
            return StatusCode(response.StatusCode, response.Result);
        }
    }
}
