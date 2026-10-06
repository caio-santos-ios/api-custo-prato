using api_custo_prato.src.Calculations;
using api_custo_prato.src.Interfaces;
using api_custo_prato.src.Models;
using api_custo_prato.src.Models.Base;
using api_custo_prato.src.Requests.Ingredients;
using api_custo_prato.src.Shared.DTOs;
using api_custo_prato.src.Shared.Utils;

namespace api_custo_prato.src.Services
{
    public class IngredientService(IIngredientRepository repository, IRecipeRepository recipeRepository) : IIngredientService
    {
        public static object ToResponse(Ingredient i, int usedInRecipes = 0)
        {
            IngredientCostResult cost = CostCalculator.CalculateIngredient(i.PurchaseQuantity, i.PurchaseUnit, i.PurchasePrice, i.YieldPercent);
            return new
            {
                id = i.Id,
                name = i.Name,
                category = i.Category,
                purchaseQuantity = i.PurchaseQuantity,
                purchaseUnit = i.PurchaseUnit,
                purchasePrice = i.PurchasePrice,
                yieldPercent = i.YieldPercent,
                notes = i.Notes,
                active = i.Active,
                createdAt = i.CreatedAt,
                updatedAt = i.UpdatedAt,
                usedInRecipes,
                currentStock = i.CurrentStock,
                baseUnit = cost.BaseUnit,
                costPerPurchaseUnit = cost.CostPerPurchaseUnit,
                costPerBaseUnit = cost.CostPerBaseUnit,
                correctionFactor = cost.CorrectionFactor,
                realCostPerPurchaseUnit = cost.RealCostPerPurchaseUnit,
                realCostPerBaseUnit = cost.RealCostPerBaseUnit
            };
        }

        private static string? Validate(CreateIngredientRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.PurchaseUnit) && !UnitConverter.IsValid(request.PurchaseUnit)) return "Unidade de compra inválida. Use kg, g, L, ml ou un.";
            if (request.PurchaseQuantity < 0) return "A quantidade comprada não pode ser negativa.";
            if (request.YieldPercent <= 0 || request.YieldPercent > 100) return "O aproveitamento deve ser maior que 0% e no máximo 100%.";
            return null;
        }

        private async Task<Dictionary<string, int>> UsageMapAsync(string userId)
        {
            List<Recipe> recipes = await recipeRepository.GetAllByUserAsync(userId);
            return recipes
                .SelectMany(r => r.Items.Select(i => i.IngredientId).Distinct())
                .GroupBy(id => id)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        public async Task<ResponseApi<PaginationApi<List<dynamic>>>> GetAllAsync(GetAllDTO request, string userId)
        {
            try
            {
                PaginationUtil<Ingredient> pagination = new(request.QueryParams);
                List<Ingredient> list = await repository.GetAllAsync(pagination, userId);
                int count = await repository.GetCountDocumentsAsync(pagination, userId);
                Dictionary<string, int> usage = await UsageMapAsync(userId);

                List<dynamic> data = list.Select(i => (dynamic)ToResponse(i, usage.GetValueOrDefault(i.Id))).ToList();
                return new(new PaginationApi<List<dynamic>>(data, count, pagination.PageNumber, pagination.PageSize), 200, "Ingredientes listados com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }

        public async Task<ResponseApi<List<dynamic>>> GetSelectAsync(string userId)
        {
            try
            {
                List<Ingredient> list = await repository.GetAllByUserAsync(userId);
                List<dynamic> data = list.Select(i => (dynamic)ToResponse(i)).ToList();
                return new(data, 200, "Ingredientes listados com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }

        public async Task<ResponseApi<List<string>>> GetCategoriesAsync(string userId)
        {
            try
            {
                return new(await repository.GetCategoriesAsync(userId), 200, "Categorias listadas com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }

        public async Task<ResponseApi<dynamic?>> GetByIdAsync(string id, string userId)
        {
            try
            {
                Ingredient? entity = await repository.GetByIdAsync(id, userId);
                if (entity is null) return new(null, 404, "Ingrediente não encontrado");

                List<Recipe> recipes = await recipeRepository.GetUsingIngredientAsync(id, userId);
                return new(ToResponse(entity, recipes.Count), 200, "Ingrediente encontrado");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }

        public async Task<ResponseApi<dynamic?>> CreateAsync(CreateIngredientRequest request)
        {
            try
            {
                string? error = Validate(request);
                if (error is not null) return new(null, 400, error);

                Ingredient entity = new()
                {
                    UserId = request.UserId,
                    Name = request.Name.Trim(),
                    Category = request.Category?.Trim() ?? "",
                    PurchaseQuantity = request.PurchaseQuantity,
                    PurchaseUnit = UnitConverter.Normalize(request.PurchaseUnit),
                    PurchasePrice = request.PurchasePrice,
                    YieldPercent = request.YieldPercent,
                    Notes = request.Notes?.Trim() ?? "",
                    CreatedBy = request.CreatedBy,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await repository.CreateAsync(entity);
                return new(ToResponse(entity), 201, "Ingrediente cadastrado com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }

        public async Task<ResponseApi<dynamic?>> UpdateAsync(UpdateIngredientRequest request, string userId)
        {
            try
            {
                string? error = Validate(request);
                if (error is not null) return new(null, 400, error);

                Ingredient? existed = await repository.GetByIdAsync(request.Id, userId);
                if (existed is null) return new(null, 404, "Ingrediente não encontrado");

                string newUnit = UnitConverter.Normalize(request.PurchaseUnit);
                List<Recipe> using_ = await recipeRepository.GetUsingIngredientAsync(existed.Id, userId);

                if (using_.Count > 0 && !UnitConverter.AreCompatible(existed.PurchaseUnit, newUnit))
                {
                    return new(null, 400, $"Este ingrediente é usado em {using_.Count} receita(s) com unidade de {UnitConverter.FamilyOf(existed.PurchaseUnit) switch { "mass" => "peso", "volume" => "volume", _ => "unidade" }}. Não é possível trocar para {newUnit}.");
                }

                existed.Name = request.Name.Trim();
                existed.Category = request.Category?.Trim() ?? "";
                existed.PurchaseQuantity = request.PurchaseQuantity;
                existed.PurchaseUnit = newUnit;
                existed.PurchasePrice = request.PurchasePrice;
                existed.YieldPercent = request.YieldPercent;
                existed.Notes = request.Notes?.Trim() ?? "";
                existed.UpdatedAt = DateTime.UtcNow;
                existed.UpdatedBy = request.UpdatedBy;

                await repository.UpdateAsync(existed);
                return new(ToResponse(existed, using_.Count), 200,
                    using_.Count > 0
                        ? $"Ingrediente atualizado. O custo de {using_.Count} receita(s) foi recalculado."
                        : "Ingrediente atualizado com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }

        public async Task<ResponseApi<dynamic?>> DeleteAsync(DeleteDTO request, string userId)
        {
            try
            {
                Ingredient? existed = await repository.GetByIdAsync(request.Id, userId);
                if (existed is null) return new(null, 404, "Ingrediente não encontrado");

                List<Recipe> using_ = await recipeRepository.GetUsingIngredientAsync(existed.Id, userId);
                if (using_.Count > 0)
                {
                    string names = string.Join(", ", using_.Take(3).Select(r => r.Name)) + (using_.Count > 3 ? "..." : "");
                    return new(null, 400, $"Não é possível excluir: o ingrediente é usado em {using_.Count} receita(s) ({names}).");
                }

                existed.Deleted = true;
                existed.DeletedBy = request.DeletedBy;
                existed.DeletedAt = DateTime.UtcNow;

                await repository.UpdateAsync(existed);
                return new(new { id = existed.Id }, 200, "Ingrediente excluído com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }
    }
}
