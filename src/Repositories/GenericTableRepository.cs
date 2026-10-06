using api_custo_prato.src.Infraestructure;
using api_custo_prato.src.Interfaces;
using api_custo_prato.src.Models;
using api_custo_prato.src.Shared.Utils;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace api_custo_prato.src.Repository
{
    public class GenericTableRepository(AppDbContext context) : IGenericTableRepository
    {
        public async Task<List<dynamic>> GetSelectAsync(List<BsonDocument> pipeline)
        {
            List<BsonDocument> results = await context.GenericTables.Aggregate<BsonDocument>(pipeline).ToListAsync();
            List<dynamic> entities = results.Select(doc => BsonSerializer.Deserialize<dynamic>(doc)).ToList();
            return entities;
        }

        public async Task<GenericTable?> GetByIdAsync(string id)
        {
            return await context.GenericTables.Find(x => x.Id == id && !x.Deleted).FirstOrDefaultAsync();
        }

        public async Task<GenericTable> CreateAsync(GenericTable entity)
        {
            await context.GenericTables.InsertOneAsync(entity);
            return entity;
        }

        public async Task<GenericTable> UpdateAsync(GenericTable entity)
        {
            await context.GenericTables.ReplaceOneAsync(x => x.Id == entity.Id && x.UserId == entity.UserId, entity);
            return entity;
        }
    }
}
