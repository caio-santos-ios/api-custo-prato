using api_custo_prato.src.Models;
using api_custo_prato.src.Models.Base;
using api_custo_prato.src.Requests;
using api_custo_prato.src.Requests.Ingredients;
using api_custo_prato.src.Requests.Recipes;
using api_custo_prato.src.Requests.Simulator;
using api_custo_prato.src.Requests.Stores;
using api_custo_prato.src.Shared.DTOs;

namespace api_custo_prato.src.Interfaces
{
    public interface IAuthService
    {
        Task<ResponseApi<dynamic?>> RegisterAsync(RegisterRequest request);
        Task<ResponseApi<dynamic?>> LoginAsync(LoginRequest request);
        Task<ResponseApi<dynamic?>> ForgotPasswordAsync(ForgotPasswordRequest request);
        Task<ResponseApi<dynamic?>> ResetPasswordAsync(ResetPasswordRequest request);
        Task<ResponseApi<dynamic?>> NewCodeAsync(NewCodeRequest request);
    }

    public interface IUserService
    {
        Task<ResponseApi<dynamic?>> GetMeAsync(string id);
        Task<ResponseApi<dynamic?>> UpdateAsync(UpdateUserDTO request);
        Task<ResponseApi<dynamic?>> ChangePasswordAsync(ChangePasswordDTO request);
        Task<ResponseApi<dynamic?>> UpdateConfirmAccountAsync(UpdateConfirmAccountDTO request);
    }

    public interface IStoreService
    {
        Task<ResponseApi<Store?>> GetAsync(string userId);
        Task<ResponseApi<Store?>> UpdateAsync(UpdateStoreRequest request, string userId);
        Task<ResponseApi<Store?>> UploadLogoAsync(IFormFile file, string userId);
        Task<Store> EnsureStoreAsync(string userId, string name = "");
    }

    public interface IIngredientService
    {
        Task<ResponseApi<PaginationApi<List<dynamic>>>> GetAllAsync(GetAllDTO request, string userId);
        Task<ResponseApi<List<dynamic>>> GetSelectAsync(string userId);
        Task<ResponseApi<List<string>>> GetCategoriesAsync(string userId);
        Task<ResponseApi<dynamic?>> GetByIdAsync(string id, string userId);
        Task<ResponseApi<dynamic?>> CreateAsync(CreateIngredientRequest request);
        Task<ResponseApi<dynamic?>> UpdateAsync(UpdateIngredientRequest request, string userId);
        Task<ResponseApi<dynamic?>> DeleteAsync(DeleteDTO request, string userId);
    }

    public interface IRecipeService
    {
        Task<ResponseApi<PaginationApi<List<dynamic>>>> GetAllAsync(GetAllDTO request, string userId);
        Task<ResponseApi<List<string>>> GetCategoriesAsync(string userId);
        Task<ResponseApi<dynamic?>> GetByIdAsync(string id, string userId);
        Task<ResponseApi<dynamic?>> CreateAsync(CreateRecipeRequest request);
        Task<ResponseApi<dynamic?>> UpdateAsync(UpdateRecipeRequest request, string userId);
        Task<ResponseApi<dynamic?>> DuplicateAsync(string id, string userId);
        Task<ResponseApi<dynamic?>> ToggleMenuAsync(string id, string userId);
        Task<ResponseApi<dynamic?>> UploadPhotoAsync(string id, IFormFile file, string userId);
        Task<ResponseApi<dynamic?>> DeleteAsync(DeleteDTO request, string userId);
        Task<ResponseApi<List<dynamic>>> GetMenuAsync(string userId);
    }

    public interface ISimulatorService
    {
        Task<ResponseApi<dynamic?>> CalculateAsync(SimulationRequest request, string userId);
    }

    public interface IDashboardService
    {
        Task<ResponseApi<dynamic?>> GetAsync(string userId);
    }

    public interface IPublicMenuService
    {
        Task<ResponseApi<dynamic?>> GetMenuAsync(string slug);
    }

    public interface IStockEntryService
    {
        Task<ResponseApi<PaginationApi<List<dynamic>>>> GetAllAsync(GetAllDTO request, string userId);
        Task<ResponseApi<List<dynamic>>> GetByIngredientIdAsync(string ingredientId, string userId);
        Task<ResponseApi<dynamic?>> CreateAsync(api_custo_prato.src.Requests.Stock.CreateStockEntryRequest request, string userId);
        Task<ResponseApi<dynamic?>> DeleteAsync(string id, string userId);
    }
}
