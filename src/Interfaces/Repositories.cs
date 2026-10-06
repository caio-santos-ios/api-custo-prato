using api_custo_prato.src.Models;
using api_custo_prato.src.Models.Base;
using api_custo_prato.src.Shared.Utils;

namespace api_custo_prato.src.Interfaces
{
    public interface IUserRepository
    {
        Task<ResponseApi<User?>> CreateAsync(User user);
        Task<ResponseApi<User?>> GetByIdAsync(string id);
        Task<ResponseApi<User?>> GetByEmailAsync(string email);
        Task<ResponseApi<User?>> GetByCodeAsync(string code);
        Task<ResponseApi<User?>> UpdateAsync(User user);
    }

    public interface IStoreRepository
    {
        Task<Store?> GetByUserIdAsync(string userId);
        Task<Store?> GetBySlugAsync(string slug);
        Task<bool> SlugExistsAsync(string slug, string exceptUserId = "");
        Task<Store> CreateAsync(Store entity);
        Task<Store> UpdateAsync(Store entity);
    }

    public interface IIngredientRepository
    {
        Task<List<Ingredient>> GetAllAsync(PaginationUtil<Ingredient> pagination, string userId);
        Task<int> GetCountDocumentsAsync(PaginationUtil<Ingredient> pagination, string userId);
        Task<List<Ingredient>> GetAllByUserAsync(string userId);
        Task<List<Ingredient>> GetByIdsAsync(IEnumerable<string> ids, string userId);
        Task<List<string>> GetCategoriesAsync(string userId);
        Task<Ingredient?> GetByIdAsync(string id, string userId);
        Task<Ingredient> CreateAsync(Ingredient entity);
        Task<Ingredient> UpdateAsync(Ingredient entity);
    }

    public interface IRecipeRepository
    {
        Task<List<Recipe>> GetAllAsync(PaginationUtil<Recipe> pagination, string userId);
        Task<int> GetCountDocumentsAsync(PaginationUtil<Recipe> pagination, string userId);
        Task<List<Recipe>> GetAllByUserAsync(string userId);
        Task<List<string>> GetCategoriesAsync(string userId);
        Task<List<Recipe>> GetUsingIngredientAsync(string ingredientId, string userId);
        Task<Recipe?> GetByIdAsync(string id, string userId);
        Task<Recipe> CreateAsync(Recipe entity);
        Task<Recipe> UpdateAsync(Recipe entity);
    }

    public interface IStockEntryRepository
    {
        Task<List<StockEntry>> GetAllAsync(PaginationUtil<StockEntry> pagination, string userId);
        Task<int> GetCountDocumentsAsync(PaginationUtil<StockEntry> pagination, string userId);
        Task<List<StockEntry>> GetByIngredientIdAsync(string ingredientId, string userId, int limit = 50);
        Task<StockEntry?> GetByIdAsync(string id, string userId);
        Task<StockEntry> CreateAsync(StockEntry entity);
        Task DeleteAsync(string id, string userId);
    }
}
