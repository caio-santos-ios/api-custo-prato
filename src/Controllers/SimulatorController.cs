using api_custo_prato.src.Interfaces;
using api_custo_prato.src.Models.Base;
using api_custo_prato.src.Requests.Simulator;
using api_custo_prato.src.Shared.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api_custo_prato.src.Controllers
{
    [Route("api/simulator")]
    [ApiController]
    [Authorize]
    public class SimulatorController(ISimulatorService service) : ControllerBase
    {
        private string UserId => User.GetUserId();

        [HttpPost("calculate")]
        public async Task<IActionResult> Calculate([FromBody] SimulationRequest request)
        {
            if (request == null) return BadRequest("Dados inválidos.");
            ResponseApi<dynamic?> response = await service.CalculateAsync(request, UserId);
            return StatusCode(response.StatusCode, response.Result);
        }
    }
}
