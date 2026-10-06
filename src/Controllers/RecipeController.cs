using api_custo_prato.src.Interfaces;
using api_custo_prato.src.Models.Base;
using api_custo_prato.src.Requests.Recipes;
using api_custo_prato.src.Shared.DTOs;
using api_custo_prato.src.Shared.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api_custo_prato.src.Controllers
{
    [Route("api/recipes")]
    [ApiController]
    [Authorize]
    public class RecipeController(IRecipeService service) : ControllerBase
    {
        private string UserId => User.GetUserId();

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            ResponseApi<PaginationApi<List<dynamic>>> response = await service.GetAllAsync(new(Request.Query), UserId);
            return StatusCode(response.StatusCode, response.Result);
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            ResponseApi<List<string>> response = await service.GetCategoriesAsync(UserId);
            return StatusCode(response.StatusCode, response.Result);
        }

        [HttpGet("menu")]
        public async Task<IActionResult> GetMenu()
        {
            ResponseApi<List<dynamic>> response = await service.GetMenuAsync(UserId);
            return StatusCode(response.StatusCode, response.Result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            ResponseApi<dynamic?> response = await service.GetByIdAsync(id, UserId);
            return StatusCode(response.StatusCode, response.Result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRecipeRequest request)
        {
            if (request == null) return BadRequest("Dados inválidos.");
            request.UserId = UserId;
            request.CreatedBy = UserId;

            ResponseApi<dynamic?> response = await service.CreateAsync(request);
            return StatusCode(response.StatusCode, response.Result);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateRecipeRequest request)
        {
            if (request == null) return BadRequest("Dados inválidos.");
            request.UserId = UserId;
            request.UpdatedBy = UserId;

            ResponseApi<dynamic?> response = await service.UpdateAsync(request, UserId);
            return StatusCode(response.StatusCode, response.Result);
        }

        [HttpPost("{id}/duplicate")]
        public async Task<IActionResult> Duplicate(string id)
        {
            ResponseApi<dynamic?> response = await service.DuplicateAsync(id, UserId);
            return StatusCode(response.StatusCode, response.Result);
        }

        [HttpPatch("{id}/menu-visibility")]
        public async Task<IActionResult> ToggleMenuVisibility(string id)
        {
            ResponseApi<dynamic?> response = await service.ToggleMenuAsync(id, UserId);
            return StatusCode(response.StatusCode, response.Result);
        }

        [HttpPost("{id}/photo")]
        public async Task<IActionResult> UploadPhoto(string id, [FromForm] IFormFile file)
        {
            if (file == null) return BadRequest("Nenhum arquivo enviado.");
            ResponseApi<dynamic?> response = await service.UploadPhotoAsync(id, file, UserId);
            return StatusCode(response.StatusCode, response.Result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            ResponseApi<dynamic?> response = await service.DeleteAsync(new() { Id = id, DeletedBy = UserId }, UserId);
            return StatusCode(response.StatusCode, response.Result);
        }
    }
}
