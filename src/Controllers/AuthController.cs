using api_custo_prato.src.Interfaces;
using api_custo_prato.src.Models.Base;
using api_custo_prato.src.Requests;
using Microsoft.AspNetCore.Mvc;

namespace api_custo_prato.src.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController(IAuthService service) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (request == null) return BadRequest("Dados inválidos.");
            ResponseApi<dynamic?> response = await service.RegisterAsync(request);
            return StatusCode(response.StatusCode, response.Result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request == null) return BadRequest("Dados inválidos.");

            request.Device.Ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
            request.Device.UserAgent = Request.Headers.UserAgent.ToString();

            ResponseApi<dynamic?> response = await service.LoginAsync(request);
            return StatusCode(response.StatusCode, response.Result);
        }

        [HttpPost("new-code")]
        public async Task<IActionResult> NewCode([FromBody] NewCodeRequest request)
        {
            if (request == null) return BadRequest("Dados inválidos.");
            ResponseApi<dynamic?> response = await service.NewCodeAsync(request);
            return StatusCode(response.StatusCode, response.Result);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (request == null) return BadRequest("Dados inválidos.");
            ResponseApi<dynamic?> response = await service.ForgotPasswordAsync(request);
            return StatusCode(response.StatusCode, response.Result);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (request == null) return BadRequest("Dados inválidos.");
            ResponseApi<dynamic?> response = await service.ResetPasswordAsync(request);
            return StatusCode(response.StatusCode, response.Result);
        }
    }
}
