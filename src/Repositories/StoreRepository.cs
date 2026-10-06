using api_custo_prato.src.Infraestructure;
using api_custo_prato.src.Interfaces;
using api_custo_prato.src.Models;
using MongoDB.Driver;

namespace api_custo_prato.src.Repository
{
    public class StoreRepository(AppDbContext context) : IStoreRepository
    {
        public async Task<Store?> GetByUserIdAsync(string userId)
            => await context.Stores.Find(x => x.UserId == userId && !x.Deleted).FirstOrDefaultAsync();

        public async Task<Store?> GetBySlugAsync(string slug)
        {
            string normalized = (slug ?? "").Trim().ToLowerInvariant();
            return await context.Stores.Find(x => x.Slug == normalized && !x.Deleted).FirstOrDefaultAsync();
        }

        public async Task<bool> SlugExistsAsync(string slug, string exceptUserId = "")
        {
            string normalized = (slug ?? "").Trim().ToLowerInvariant();
            return await context.Stores.Find(x => x.Slug == normalized && !x.Deleted && x.UserId != exceptUserId).AnyAsync();
        }

        public async Task<Store> CreateAsync(Store entity)
        {
            await context.Stores.InsertOneAsync(entity);
            return entity;
        }

        public async Task<Store> UpdateAsync(Store entity)
        {
            await context.Stores.ReplaceOneAsync(x => x.Id == entity.Id, entity);
            return entity;
        }
    }
}
