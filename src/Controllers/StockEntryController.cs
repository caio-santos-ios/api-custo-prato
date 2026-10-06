using api_custo_prato.src.Interfaces;
using api_custo_prato.src.Models.Base;
using api_custo_prato.src.Requests.Stock;
using api_custo_prato.src.Shared.DTOs;
using api_custo_prato.src.Shared.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api_custo_prato.src.Controllers
{
    [Route("api/stock-entries")]
    [ApiController]
    [Authorize]
    public class StockEntryController(IStockEntryService service) : ControllerBase
    {
        private string UserId => User.GetUserId();

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            ResponseApi<PaginationApi<List<dynamic>>> response = await service.GetAllAsync(new(Request.Query), UserId);
            return StatusCode(response.StatusCode, response.Result);
        }

        [HttpGet("ingredient/{ingredientId}")]
        public async Task<IActionResult> GetByIngredientId(string ingredientId)
        {
            ResponseApi<List<dynamic>> response = await service.GetByIngredientIdAsync(ingredientId, UserId);
            return StatusCode(response.StatusCode, response.Result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateStockEntryRequest request)
        {
            if (request == null) return BadRequest("Dados inválidos.");
            ResponseApi<dynamic?> response = await service.CreateAsync(request, UserId);
            return StatusCode(response.StatusCode, response.Result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            ResponseApi<dynamic?> response = await service.DeleteAsync(id, UserId);
            return StatusCode(response.StatusCode, response.Result);
        }
    }
}
