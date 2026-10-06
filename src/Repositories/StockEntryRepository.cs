using api_custo_prato.src.Infraestructure;
using api_custo_prato.src.Interfaces;
using api_custo_prato.src.Models;
using api_custo_prato.src.Shared.Utils;
using MongoDB.Bson;
using MongoDB.Driver;

namespace api_custo_prato.src.Repository
{
    public class StockEntryRepository(AppDbContext context) : IStockEntryRepository
    {
        public async Task<List<StockEntry>> GetAllAsync(PaginationUtil<StockEntry> pagination, string userId)
        {
            List<BsonDocument> pipeline =
            [
                MongoUtil.UserMatch(userId, pagination.PipelineFilter),
                new("$sort", pagination.PipelineSort.ElementCount > 0 ? pagination.PipelineSort : new BsonDocument("purchaseDate", -1)),
                new("$skip", pagination.Skip),
                new("$limit", pagination.Limit)
            ];

            return await context.StockEntries.Aggregate<StockEntry>(pipeline).ToListAsync();
        }

        public async Task<int> GetCountDocumentsAsync(PaginationUtil<StockEntry> pagination, string userId)
        {
            List<BsonDocument> pipeline =
            [
                MongoUtil.UserMatch(userId, pagination.PipelineFilter),
                new("$count", "total")
            ];

            BsonDocument? result = await context.StockEntries.Aggregate<BsonDocument>(pipeline).FirstOrDefaultAsync();
            return result is null ? 0 : result["total"].ToInt32();
        }

        public async Task<List<StockEntry>> GetByIngredientIdAsync(string ingredientId, string userId, int limit = 50)
        {
            return await context.StockEntries
                .Find(x => x.IngredientId == ingredientId && x.UserId == userId && !x.Deleted)
                .SortByDescending(x => x.PurchaseDate)
                .Limit(limit)
                .ToListAsync();
        }

        public async Task<StockEntry?> GetByIdAsync(string id, string userId)
        {
            if (!MongoUtil.IsObjectId(id)) return null;
            return await context.StockEntries.Find(x => x.Id == id && x.UserId == userId && !x.Deleted).FirstOrDefaultAsync();
        }

        public async Task<StockEntry> CreateAsync(StockEntry entity)
        {
            await context.StockEntries.InsertOneAsync(entity);
            return entity;
        }

        public async Task DeleteAsync(string id, string userId)
        {
            if (!MongoUtil.IsObjectId(id)) return;
            var update = Builders<StockEntry>.Update
                .Set(x => x.Deleted, true)
                .Set(x => x.DeletedAt, DateTime.UtcNow)
                .Set(x => x.DeletedBy, userId);

            await context.StockEntries.UpdateOneAsync(x => x.Id == id && x.UserId == userId, update);
        }
    }
}
