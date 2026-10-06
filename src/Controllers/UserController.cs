using api_custo_prato.src.Interfaces;
using api_custo_prato.src.Models.Base;
using api_custo_prato.src.Requests;
using api_custo_prato.src.Shared.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api_custo_prato.src.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController(IUserService service) : ControllerBase
    {
        private string UserId => User.GetUserId();

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            ResponseApi<dynamic?> response = await service.GetMeAsync(UserId);
            return StatusCode(response.StatusCode, response.Result);
        }

        [Authorize]
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateUserDTO request)
        {
            if (request == null) return BadRequest("Dados inválidos.");
            request.Id = UserId;
            ResponseApi<dynamic?> response = await service.UpdateAsync(request);
            return StatusCode(response.StatusCode, response.Result);
        }

        [Authorize]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO request)
        {
            if (request == null) return BadRequest("Dados inválidos.");
            request.Id = UserId;
            ResponseApi<dynamic?> response = await service.ChangePasswordAsync(request);
            return StatusCode(response.StatusCode, response.Result);
        }

        [HttpPut("confirm-account")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmAccount([FromBody] UpdateConfirmAccountDTO request)
        {
            if (request == null) return BadRequest("Dados inválidos.");
            ResponseApi<dynamic?> response = await service.UpdateConfirmAccountAsync(request);
            return StatusCode(response.StatusCode, response.Result);
        }
    }
}
