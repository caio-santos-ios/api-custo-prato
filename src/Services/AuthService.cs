using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using api_custo_prato.src.Helpers;
using api_custo_prato.src.Interfaces;
using api_custo_prato.src.Models;
using api_custo_prato.src.Models.Base;
using api_custo_prato.src.Requests;
using api_custo_prato.src.Shared.Utils;

namespace api_custo_prato.src.Services
{
    public class AuthService(
        IUserRepository userRepository,
        IStoreService storeService,
        MailHelper mailHelper
    ) : IAuthService
    {
        private const int MaxIncorrectPasswords = 5;

        #region REGISTER
        public async Task<ResponseApi<dynamic?>> RegisterAsync(RegisterRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
                    return new(null, 400, "A senha deve conter no mínimo 8 caracteres.");

                string email = request.Email.Trim().ToLowerInvariant();

                ResponseApi<User?> existed = await userRepository.GetByEmailAsync(email);
                if (existed.Data is not null) return new(null, 400, "E-mail inválido, tente usar outro");

                dynamic access = Util.GenerateCodeAccess();

                User user = new()
                {
                    Email = email,
                    Name = request.Name.Trim(),
                    Phone = request.Phone?.Trim() ?? "",
                    Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    CodeAccess = access.CodeAccess,
                    CodeAccessExpiration = access.CodeAccessExpiration,
                    ValidatedAccess = false
                };

                ResponseApi<User?> response = await userRepository.CreateAsync(user);
                if (response.Data is null) return new(null, 400, "Falha ao criar conta.");

                await storeService.EnsureStoreAsync(user.Id, request.StoreName);

                await mailHelper.SendAccountConfirmationMail(email, user.Name, access.CodeAccess);

                return new(new { name = user.Name, email }, 201, "Conta criada com sucesso, verifique o seu e-mail de confirmação da conta.");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde - {ex.Message}");
            }
        }
        #endregion

        #region LOGIN
        public async Task<ResponseApi<dynamic?>> LoginAsync(LoginRequest request)
        {
            try
            {
                ResponseApi<User?> response = await userRepository.GetByEmailAsync(request.Email);
                if (response.Data is null) return new(null, 400, "E-mail ou senha são incorretos");

                User user = response.Data;

                if (user.Blocked) return new(null, 400, "Conta bloqueada por tentativas de acesso. Use \"Esqueci minha senha\" para desbloquear.");

                bool isValid = BCrypt.Net.BCrypt.Verify(request.Password, user.Password);
                if (!isValid)
                {
                    user.IncorrectsPassword.Add(new UserIncorrectPassword()
                    {
                        Ip = request.Device.Ip,
                        Date = DateTime.UtcNow,
                        Platform = request.Device.Platform
                    });

                    if (user.IncorrectsPassword.Count >= MaxIncorrectPasswords)
                    {
                        user.Blocked = true;
                        await userRepository.UpdateAsync(user);
                        return new(null, 400, "Usuário bloqueado por tentativas de acesso indevido. Use \"Esqueci minha senha\" para desbloquear.");
                    }

                    await userRepository.UpdateAsync(user);
                    return new(null, 400, "E-mail ou senha são incorretos");
                }

                if (!user.ValidatedAccess)
                {
                    dynamic generateCode = Util.GenerateCodeAccess();
                    user.CodeAccess = generateCode.CodeAccess;
                    user.CodeAccessExpiration = generateCode.CodeAccessExpiration;
                    await userRepository.UpdateAsync(user);

                    await mailHelper.SendAccountConfirmationMail(user.Email, user.Name, generateCode.CodeAccess);

                    return new(new { pendingConfirmation = true, email = user.Email }, 403, "Conta não foi confirmada, enviamos um e-mail de confirmação novamente");
                }

                user.IncorrectsPassword = [];
                request.Device.Date = DateTime.UtcNow;
                user.Devices.Add(request.Device);
                if (user.Devices.Count > 20) user.Devices = user.Devices.TakeLast(20).ToList();

                await userRepository.UpdateAsync(user);

                Store store = await storeService.EnsureStoreAsync(user.Id);

                return new(new
                {
                    Token = GenerateJwtToken(user),
                    RefreshToken = GenerateJwtToken(user, true),
                    user.Id,
                    user.Name,
                    user.Email,
                    user.Photo,
                    storeName = store.Name,
                    storeSlug = store.Slug
                }, 200, "Login feito com sucesso.");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde - {ex.Message}");
            }
        }
        #endregion

        #region RESET PASSWORD
        public async Task<ResponseApi<dynamic?>> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            try
            {
                ResponseApi<User?> response = await userRepository.GetByEmailAsync(request.Email);

                // Não revela se o e-mail existe ou não.
                if (response.Data is null) return new(new { }, 200, "Se o e-mail estiver cadastrado, enviaremos um código de verificação.");

                dynamic generateCode = Util.GenerateCodeAccess(15);
                response.Data.CodeAccess = generateCode.CodeAccess;
                response.Data.CodeAccessExpiration = generateCode.CodeAccessExpiration;

                await userRepository.UpdateAsync(response.Data);
                await mailHelper.SendPasswordResetMail(response.Data.Email, response.Data.Name, generateCode.CodeAccess);

                return new(new { }, 200, "Se o e-mail estiver cadastrado, enviaremos um código de verificação.");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde - {ex.Message}");
            }
        }

        public async Task<ResponseApi<dynamic?>> ResetPasswordAsync(ResetPasswordRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
                    return new(null, 400, "A senha deve conter no mínimo 8 caracteres.");

                ResponseApi<User?> response = await userRepository.GetByCodeAsync(request.Code.Trim());
                if (response.Data is null) return new(null, 400, "Código inválido");

                if (response.Data.CodeAccessExpiration is null || DateTime.UtcNow > response.Data.CodeAccessExpiration.Value.ToUniversalTime())
                    return new(null, 400, "Código expirou, deve solicitar outro");

                response.Data.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);
                response.Data.ValidatedAccess = true;
                response.Data.Blocked = false;
                response.Data.IncorrectsPassword = [];
                response.Data.CodeAccessExpiration = null;
                response.Data.CodeAccess = "";
                response.Data.UpdatedAt = DateTime.UtcNow;

                await userRepository.UpdateAsync(response.Data);

                return new(new { }, 200, "Senha redefinida com sucesso.");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde - {ex.Message}");
            }
        }
        #endregion

        #region CODE CONFIRMATION
        public async Task<ResponseApi<dynamic?>> NewCodeAsync(NewCodeRequest request)
        {
            try
            {
                ResponseApi<User?> response = await userRepository.GetByEmailAsync(request.Email);
                if (response.Data is null) return new(null, 400, "E-mail inválido");
                if (response.Data.ValidatedAccess) return new(null, 400, "Esta conta já está confirmada. Faça login.");

                dynamic generateCode = Util.GenerateCodeAccess();
                response.Data.CodeAccess = generateCode.CodeAccess;
                response.Data.CodeAccessExpiration = generateCode.CodeAccessExpiration;

                await userRepository.UpdateAsync(response.Data);
                await mailHelper.SendAccountConfirmationMail(response.Data.Email, response.Data.Name, generateCode.CodeAccess);

                return new(new { }, 200, "Foi enviado um código de verificação para o e-mail.");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde - {ex.Message}");
            }
        }
        #endregion

        public static string GenerateJwtToken(User user, bool refresh = false)
        {
            string secretKey = Environment.GetEnvironmentVariable("SECRET_KEY") ?? "";
            string issuer = Environment.GetEnvironmentVariable("ISSUER") ?? "";
            string audience = Environment.GetEnvironmentVariable("AUDIENCE") ?? "";

            SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(secretKey));

            Claim[] claims =
            [
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("type", refresh ? "refresh" : "access"),
                new Claim("name", user.Name),
                new Claim("photo", user.Photo)
            ];

            SigningCredentials creds = new(key, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken token = new(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: refresh ? DateTime.UtcNow.AddDays(7) : DateTime.UtcNow.AddHours(12),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
