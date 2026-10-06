using api_custo_prato.src.Helpers;
using api_custo_prato.src.Interfaces;
using api_custo_prato.src.Models;
using api_custo_prato.src.Models.Base;
using api_custo_prato.src.Requests;
using api_custo_prato.src.Shared.Utils;

namespace api_custo_prato.src.Services
{
    public class UserService(IUserRepository repository, MailHelper mailHelper) : IUserService
    {
        private static object ToPublic(User u) => new
        {
            id = u.Id,
            name = u.Name,
            email = u.Email,
            phone = u.Phone,
            photo = u.Photo,
            createdAt = u.CreatedAt
        };

        public async Task<ResponseApi<dynamic?>> GetMeAsync(string id)
        {
            try
            {
                ResponseApi<User?> user = await repository.GetByIdAsync(id);
                if (user.Data is null) return new(null, 404, "Usuário não encontrado");
                return new(ToPublic(user.Data), 200, "Usuário encontrado");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }

        public async Task<ResponseApi<dynamic?>> UpdateAsync(UpdateUserDTO request)
        {
            try
            {
                ResponseApi<User?> user = await repository.GetByIdAsync(request.Id);
                if (user.Data is null) return new(null, 404, "Usuário não encontrado");

                user.Data.Name = request.Name.Trim();
                user.Data.Phone = request.Phone?.Trim() ?? "";
                user.Data.UpdatedAt = DateTime.UtcNow;
                user.Data.UpdatedBy = request.Id;

                await repository.UpdateAsync(user.Data);
                return new(ToPublic(user.Data), 200, "Perfil atualizado com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }

        public async Task<ResponseApi<dynamic?>> ChangePasswordAsync(ChangePasswordDTO request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 8)
                    return new(null, 400, "A nova senha deve conter no mínimo 8 caracteres.");

                ResponseApi<User?> user = await repository.GetByIdAsync(request.Id);
                if (user.Data is null) return new(null, 404, "Usuário não encontrado");

                if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.Data.Password))
                    return new(null, 400, "Senha atual incorreta");

                user.Data.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                user.Data.UpdatedAt = DateTime.UtcNow;
                await repository.UpdateAsync(user.Data);

                return new(new { }, 200, "Senha alterada com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }

        public async Task<ResponseApi<dynamic?>> UpdateConfirmAccountAsync(UpdateConfirmAccountDTO request)
        {
            try
            {
                ResponseApi<User?> user = await repository.GetByCodeAsync(request.Code.Trim());

                bool expired = user.Data?.CodeAccessExpiration is not null
                    && DateTime.UtcNow > user.Data.CodeAccessExpiration.Value.ToUniversalTime();

                if (user.Data is null || expired)
                {
                    User? existed = user.Data;
                    if (existed is null && !string.IsNullOrWhiteSpace(request.Email))
                    {
                        existed = (await repository.GetByEmailAsync(request.Email)).Data;
                    }

                    if (existed is not null && !existed.ValidatedAccess)
                    {
                        dynamic access = Util.GenerateCodeAccess();
                        existed.CodeAccess = access.CodeAccess;
                        existed.CodeAccessExpiration = access.CodeAccessExpiration;
                        await repository.UpdateAsync(existed);

                        await mailHelper.SendAccountConfirmationMail(existed.Email, existed.Name, access.CodeAccess);
                        return new(null, 404, expired
                            ? "Código expirado, foi enviado um novo para o seu e-mail"
                            : "Código inválido, foi enviado um novo para o seu e-mail");
                    }
                    return new(null, 404, "Código inválido");
                }

                user.Data.UpdatedAt = DateTime.UtcNow;
                user.Data.CodeAccessExpiration = null;
                user.Data.CodeAccess = "";
                user.Data.ValidatedAccess = true;

                ResponseApi<User?> response = await repository.UpdateAsync(user.Data);
                if (!response.IsSuccess) return new(null, 400, "Falha ao confirmar conta");

                return new(new { email = user.Data.Email }, 200, "Conta confirmada com sucesso! Faça login para continuar.");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }
    }
}
