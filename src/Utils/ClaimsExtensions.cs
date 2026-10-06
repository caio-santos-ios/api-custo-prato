using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace api_custo_prato.src.Shared.Utils
{
    public static class ClaimsExtensions
    {
        /// <summary>Id do usuário logado (claim "sub", mapeada ou não para NameIdentifier).</summary>
        public static string GetUserId(this ClaimsPrincipal user)
            => user.FindFirst(ClaimTypes.NameIdentifier)?.Value
               ?? user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
               ?? "";
    }
}
