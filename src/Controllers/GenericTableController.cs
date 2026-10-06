using api_custo_prato.src.Interfaces;
using api_custo_prato.src.Models;
using api_custo_prato.src.Models.Base;
using api_custo_prato.src.Requests.GenericTable;
using api_custo_prato.src.Shared.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api_custo_prato.src.Controllers
{
    [Route("api/generic-tables")]
    [ApiController]
    [Authorize]
    public class GenericTableController(IGenericTableService service) : ControllerBase
    {
        private string UserId => User.GetUserId();

        [HttpGet("select")]
        public async Task<IActionResult> GetAll()
        {
            var dict = Request.Query.ToDictionary(k => k.Key, v => v.Value.ToString());
            if (!dict.ContainsKey("userId")) dict["userId"] = UserId;

            ResponseApi<List<dynamic>> response = await service.GetSelectAsync(new(dict));
            return StatusCode(response.StatusCode, response.Result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateGenericTableRequest request)
        {
            if (request == null) return BadRequest("Dados inválidos.");
            request.UserId = UserId;
            request.CreatedBy = UserId;

            ResponseApi<GenericTable?> response = await service.CreateAsync(request);
            return StatusCode(response.StatusCode, response.Result);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateGenericTableRequest request)
        {
            if (request == null) return BadRequest("Dados inválidos.");
            request.UserId = UserId;
            request.UpdatedBy = UserId;

            ResponseApi<GenericTable?> response = await service.UpdateAsync(request);
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
