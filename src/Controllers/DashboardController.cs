using api_custo_prato.src.Interfaces;
using api_custo_prato.src.Models.Base;
using api_custo_prato.src.Shared.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api_custo_prato.src.Controllers
{
    [Route("api/dashboard")]
    [ApiController]
    [Authorize]
    public class DashboardController(IDashboardService service) : ControllerBase
    {
        private string UserId => User.GetUserId();

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            ResponseApi<dynamic?> response = await service.GetAsync(UserId);
            return StatusCode(response.StatusCode, response.Result);
        }
    }
}
