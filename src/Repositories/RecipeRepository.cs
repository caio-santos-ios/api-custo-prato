using api_custo_prato.src.Infraestructure;
using api_custo_prato.src.Interfaces;
using api_custo_prato.src.Models;
using api_custo_prato.src.Shared.Utils;
using MongoDB.Bson;
using MongoDB.Driver;

namespace api_custo_prato.src.Repository
{
    public class RecipeRepository(AppDbContext context) : IRecipeRepository
    {
        public async Task<List<Recipe>> GetAllAsync(PaginationUtil<Recipe> pagination, string userId)
        {
            List<BsonDocument> pipeline =
            [
                MongoUtil.UserMatch(userId, pagination.PipelineFilter),
                new("$sort", pagination.PipelineSort),
                new("$skip", pagination.Skip),
                new("$limit", pagination.Limit)
            ];

            return await context.Recipes.Aggregate<Recipe>(pipeline).ToListAsync();
        }

        public async Task<int> GetCountDocumentsAsync(PaginationUtil<Recipe> pagination, string userId)
        {
            List<BsonDocument> pipeline =
            [
                MongoUtil.UserMatch(userId, pagination.PipelineFilter),
                new("$count", "total")
            ];

            BsonDocument? result = await context.Recipes.Aggregate<BsonDocument>(pipeline).FirstOrDefaultAsync();
            return result is null ? 0 : result["total"].ToInt32();
        }

        public async Task<List<Recipe>> GetAllByUserAsync(string userId)
            => await context.Recipes
                .Find(x => x.UserId == userId && !x.Deleted)
                .SortBy(x => x.Name)
                .ToListAsync();

        public async Task<List<string>> GetCategoriesAsync(string userId)
        {
            FilterDefinition<Recipe> filter = Builders<Recipe>.Filter.Where(x => x.UserId == userId && !x.Deleted && x.Category != "");
            List<string> list = await context.Recipes.Distinct(x => x.Category, filter).ToListAsync();
            return list.OrderBy(x => x).ToList();
        }

        public async Task<List<Recipe>> GetUsingIngredientAsync(string ingredientId, string userId)
            => await context.Recipes
                .Find(x => x.UserId == userId && !x.Deleted && x.Items.Any(i => i.IngredientId == ingredientId))
                .ToListAsync();

        public async Task<Recipe?> GetByIdAsync(string id, string userId)
        {
            if (!MongoUtil.IsObjectId(id)) return null;
            return await context.Recipes.Find(x => x.Id == id && x.UserId == userId && !x.Deleted).FirstOrDefaultAsync();
        }

        public async Task<Recipe> CreateAsync(Recipe entity)
        {
            await context.Recipes.InsertOneAsync(entity);
            return entity;
        }

        public async Task<Recipe> UpdateAsync(Recipe entity)
        {
            await context.Recipes.ReplaceOneAsync(x => x.Id == entity.Id && x.UserId == entity.UserId, entity);
            return entity;
        }
    }
}
