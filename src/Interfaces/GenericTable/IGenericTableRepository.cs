using api_custo_prato.src.Models;
using MongoDB.Bson;

namespace api_custo_prato.src.Interfaces
{
    public interface IGenericTableRepository
    {
        Task<List<dynamic>> GetSelectAsync(List<BsonDocument> pipeline);
        Task<GenericTable?> GetByIdAsync(string id);
        Task<long> NextCodeAsync(string table, string userId);
        Task<GenericTable> CreateAsync(GenericTable entity);
        Task<GenericTable> UpdateAsync(GenericTable entity);
    }
}