using api_custo_prato.src.Infraestructure;
using api_custo_prato.src.Interfaces;
using api_custo_prato.src.Models;
using api_custo_prato.src.Shared.Utils;
using MongoDB.Bson;
using MongoDB.Driver;

namespace api_custo_prato.src.Repository
{
    public class IngredientRepository(AppDbContext context) : IIngredientRepository
    {
        public async Task<List<Ingredient>> GetAllAsync(PaginationUtil<Ingredient> pagination, string userId)
        {
            List<BsonDocument> pipeline =
            [
                MongoUtil.UserMatch(userId, pagination.PipelineFilter),
                new("$sort", pagination.PipelineSort),
                new("$skip", pagination.Skip),
                new("$limit", pagination.Limit)
            ];

            return await context.Ingredients.Aggregate<Ingredient>(pipeline).ToListAsync();
        }

        public async Task<int> GetCountDocumentsAsync(PaginationUtil<Ingredient> pagination, string userId)
        {
            List<BsonDocument> pipeline =
            [
                MongoUtil.UserMatch(userId, pagination.PipelineFilter),
                new("$count", "total")
            ];

            BsonDocument? result = await context.Ingredients.Aggregate<BsonDocument>(pipeline).FirstOrDefaultAsync();
            return result is null ? 0 : result["total"].ToInt32();
        }

        public async Task<List<Ingredient>> GetAllByUserAsync(string userId)
            => await context.Ingredients
                .Find(x => x.UserId == userId && !x.Deleted)
                .SortBy(x => x.Name)
                .ToListAsync();

        public async Task<List<Ingredient>> GetByIdsAsync(IEnumerable<string> ids, string userId)
        {
            List<string> valid = ids.Where(MongoUtil.IsObjectId).Distinct().ToList();
            if (valid.Count == 0) return [];
            return await context.Ingredients.Find(x => valid.Contains(x.Id) && x.UserId == userId && !x.Deleted).ToListAsync();
        }

        public async Task<List<string>> GetCategoriesAsync(string userId)
        {
            FilterDefinition<Ingredient> filter = Builders<Ingredient>.Filter.Where(x => x.UserId == userId && !x.Deleted && x.Category != "");
            List<string> list = await context.Ingredients.Distinct(x => x.Category, filter).ToListAsync();
            return list.OrderBy(x => x).ToList();
        }

        public async Task<Ingredient?> GetByIdAsync(string id, string userId)
        {
            if (!MongoUtil.IsObjectId(id)) return null;
            return await context.Ingredients.Find(x => x.Id == id && x.UserId == userId && !x.Deleted).FirstOrDefaultAsync();
        }

        public async Task<Ingredient> CreateAsync(Ingredient entity)
        {
            await context.Ingredients.InsertOneAsync(entity);
            return entity;
        }

        public async Task<Ingredient> UpdateAsync(Ingredient entity)
        {
            await context.Ingredients.ReplaceOneAsync(x => x.Id == entity.Id && x.UserId == entity.UserId, entity);
            return entity;
        }
    }
}
