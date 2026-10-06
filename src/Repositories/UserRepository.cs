using api_custo_prato.src.Infraestructure;
using api_custo_prato.src.Interfaces;
using api_custo_prato.src.Models;
using api_custo_prato.src.Models.Base;
using MongoDB.Driver;

namespace api_custo_prato.src.Repository
{
    public class UserRepository(AppDbContext context) : IUserRepository
    {
        public async Task<ResponseApi<User?>> CreateAsync(User user)
        {
            try
            {
                await context.Users.InsertOneAsync(user);
                return new(user, 201, "Usuário criado com sucesso");
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }

        public async Task<ResponseApi<User?>> GetByIdAsync(string id)
        {
            try
            {
                User? user = await context.Users.Find(x => x.Id == id && !x.Deleted).FirstOrDefaultAsync();
                return new(user);
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }

        public async Task<ResponseApi<User?>> GetByEmailAsync(string email)
        {
            try
            {
                string normalized = (email ?? "").Trim().ToLowerInvariant();
                User? user = await context.Users.Find(x => x.Email == normalized && !x.Deleted).FirstOrDefaultAsync();
                return new(user);
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }

        public async Task<ResponseApi<User?>> GetByCodeAsync(string code)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code)) return new(null);
                User? user = await context.Users.Find(x => x.CodeAccess == code && !x.Deleted).FirstOrDefaultAsync();
                return new(user);
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }

        public async Task<ResponseApi<User?>> UpdateAsync(User user)
        {
            try
            {
                await context.Users.ReplaceOneAsync(x => x.Id == user.Id, user);
                return new(user, 200, "Usuário atualizado com sucesso");
            }
            catch
            {
                return new(null, 500, "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.");
            }
        }
    }
}
