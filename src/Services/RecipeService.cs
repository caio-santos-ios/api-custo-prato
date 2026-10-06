using api_custo_prato.src.Calculations;
using api_custo_prato.src.Helpers;
using api_custo_prato.src.Interfaces;
using api_custo_prato.src.Models;
using api_custo_prato.src.Models.Base;
using api_custo_prato.src.Requests.Recipes;
using api_custo_prato.src.Shared.DTOs;
using api_custo_prato.src.Shared.Utils;

namespace api_custo_prato.src.Services
{
    public class RecipeService(
        IRecipeRepository repository,
        IIngredientRepository ingredientRepository,
        IStoreService storeService,
        UploadHelper uploadHelper
    ) : IRecipeService
    {
        #region HELPERS
        private async Task<(Dictionary<string, Ingredient> map, decimal targetMargin)> ContextAsync(string userId)
        {
            List<Ingredient> ingredients = await ingredientRepository.GetAllByUserAsync(userId);
            Store store = await storeService.EnsureStoreAsync(userId);
            return (ingredients.ToDictionary(i => i.Id), store.TargetMargin);
        }

        public static object ToSummary(Recipe r, RecipeCostResult c) => new
        {
            id = r.Id,
            name = r.Name,
            category = r.Category,
            description = r.Description,
            photo = r.Photo,
            showInMenu = r.ShowInMenu,
            active = r.Active,
            portions = r.Portions,
            itemsCount = r.Items.Count,
            salePrice = c.SalePrice,
            totalCost = c.TotalCost,
            costPerPortion = c.CostPerPortion,
            feesValue = c.FeesValue,
            grossProfit = c.GrossProfit,
            margin = c.Margin,
            costPercent = c.CostPercent,
            targetMargin = c.TargetMargin,
            suggestedPrice = c.SuggestedPrice,
            belowTarget = c.SalePrice > 0 && c.Margin < c.TargetMargin,
            warnings = c.Warnings,
            createdAt = r.CreatedAt,
            updatedAt = r.UpdatedAt
        };

        public static object ToDetail(Recipe r, RecipeCostResult c) => new
        {
            id = r.Id,
            name = r.Name,
            category = r.Category,
            description = r.Description,
            photo = r.Photo,
            showInMenu = r.ShowInMenu,
            active = r.Active,
            items = r.Items,
            yieldQuantity = r.YieldQuantity,
            yieldUnit = r.YieldUnit,
            portions = r.Portions,
            packagingCost = r.PackagingCost,
            otherCosts = r.OtherCosts,
            salePrice = r.SalePrice,
            fees = r.Fees,
            createdAt = r.CreatedAt,
            updatedAt = r.UpdatedAt,
            calculation = c
        };

        /// <summary>Valida e normaliza os dados da ficha técnica.</summary>
        private async Task<string?> ValidateAndNormalizeAsync(CreateRecipeRequest request, string userId)
        {
            if (request.Portions <= 0) return "O número de porções deve ser maior que zero.";
            if (request.YieldQuantity > 0 && !UnitConverter.IsValid(request.YieldUnit)) return "Unidade do rendimento inválida.";
            request.YieldUnit = UnitConverter.IsValid(request.YieldUnit) ? UnitConverter.Normalize(request.YieldUnit) : "kg";

            request.Items = request.Items.Where(i => !string.IsNullOrWhiteSpace(i.IngredientId)).ToList();

            List<Ingredient> ingredients = await ingredientRepository.GetByIdsAsync(request.Items.Select(i => i.IngredientId), userId);
            Dictionary<string, Ingredient> map = ingredients.ToDictionary(i => i.Id);

            foreach (RecipeItem item in request.Items)
            {
                if (!map.TryGetValue(item.IngredientId, out Ingredient? ing)) return "Um dos ingredientes selecionados não foi encontrado.";
                if (item.Quantity <= 0) return $"Informe a quantidade de \"{ing.Name}\".";
                if (!UnitConverter.IsValid(item.Unit)) return $"Unidade inválida para \"{ing.Name}\".";
                if (!UnitConverter.AreCompatible(item.Unit, ing.PurchaseUnit))
                    return $"\"{ing.Name}\" é comprado em {ing.PurchaseUnit}; use uma unidade compatível ({string.Join(", ", UnitConverter.SupportedUnits.Where(u => UnitConverter.AreCompatible(u, ing.PurchaseUnit)))}).";
                item.Unit = UnitConverter.Normalize(item.Unit);
            }

            request.OtherCosts = request.OtherCosts
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) || o.Value != 0)
                .Select(o => new RecipeOtherCost { Name = o.Name?.Trim() ?? "", Value = Math.Max(o.Value, 0), AppliesTo = o.AppliesTo == "portion" ? "portion" : "recipe" })
                .ToList();

            request.Fees = request.Fees
                .Where(f => !string.IsNullOrWhiteSpace(f.Name) || f.Value != 0)
                .Select(f => new RecipeFee { Name = f.Name?.Trim() ?? "", Value = Math.Max(f.Value, 0), Type = f.Type == "fixed" ? "fixed" : "percent" })
                .ToList();

            if (request.Fees.Where(f => f.Type == "percent").Sum(f => f.Value) >= 100) return "A soma das taxas percentuais deve ser menor que 100%.";

            return null;
        }

        private static void Apply(Recipe entity, CreateRecipeRequest request)
        {
            entity.Name = request.Name.Trim();
            entity.Category = request.Category?.Trim() ?? "";
            entity.Description = request.Description?.Trim() ?? "";
            if (!string.IsNullOrWhiteSpace(request.Photo)) entity.Photo = request.Photo.Trim();
            entity.Items = request.Items;
            entity.YieldQuantity = request.YieldQuantity;
            entity.YieldUnit = request.YieldUnit;
            entity.Portions = request.Portions;
            entity.PackagingCost = request.PackagingCost;
            entity.OtherCosts = request.OtherCosts;
            entity.SalePrice = request.SalePrice;
            entity.Fees = request.Fees;
            entity.ShowInMenu = request.ShowInMenu;
        }
        #endregion

        #region READ
        public async Task<ResponseApi<PaginationApi<List<dynamic>>>> GetAllAsync(GetAllDTO request, string userId)
        {
            try
            {
                PaginationUtil<Recipe> pagination = new(request.QueryParams);
                List<Recipe> list = await repository.GetAllAsync(pagination, userId);
                int count = await repository.GetCountDocumentsAsync(pagination, userId);
                var (map, targetMargin) = await ContextAsync(userId);

                List<dynamic> data = list
                    .Select(r => (dynamic)ToSummary(r, RecipeInputBuilder.Calculate(r, map, targetMargin)))
                    .ToList();

                return new(new PaginationApi<List<dynamic>>(data, count, pagination.PageNumber, pagination.PageSize), 200, "Receitas listadas com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }

        public async Task<ResponseApi<List<dynamic>>> GetMenuAsync(string userId)
        {
            try
            {
                List<Recipe> list = await repository.GetAllByUserAsync(userId);
                var (map, targetMargin) = await ContextAsync(userId);

                List<dynamic> data = list
                    .OrderBy(r => string.IsNullOrEmpty(r.Category) ? "zzz" : r.Category)
                    .ThenBy(r => r.Name)
                    .Select(r => (dynamic)ToSummary(r, RecipeInputBuilder.Calculate(r, map, targetMargin)))
                    .ToList();

                return new(data, 200, "Cardápio carregado com sucesso");
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
                Recipe? entity = await repository.GetByIdAsync(id, userId);
                if (entity is null) return new(null, 404, "Receita não encontrada");

                var (map, targetMargin) = await ContextAsync(userId);
                return new(ToDetail(entity, RecipeInputBuilder.Calculate(entity, map, targetMargin)), 200, "Receita encontrada");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }
        #endregion

        #region WRITE
        public async Task<ResponseApi<dynamic?>> CreateAsync(CreateRecipeRequest request)
        {
            try
            {
                string? error = await ValidateAndNormalizeAsync(request, request.UserId);
                if (error is not null) return new(null, 400, error);

                Recipe entity = new()
                {
                    UserId = request.UserId,
                    CreatedBy = request.CreatedBy,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                Apply(entity, request);

                await repository.CreateAsync(entity);

                var (map, targetMargin) = await ContextAsync(request.UserId);
                return new(ToDetail(entity, RecipeInputBuilder.Calculate(entity, map, targetMargin)), 201, "Receita cadastrada com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }

        public async Task<ResponseApi<dynamic?>> UpdateAsync(UpdateRecipeRequest request, string userId)
        {
            try
            {
                Recipe? existed = await repository.GetByIdAsync(request.Id, userId);
                if (existed is null) return new(null, 404, "Receita não encontrada");

                string? error = await ValidateAndNormalizeAsync(request, userId);
                if (error is not null) return new(null, 400, error);

                Apply(existed, request);
                existed.UpdatedAt = DateTime.UtcNow;
                existed.UpdatedBy = request.UpdatedBy;

                await repository.UpdateAsync(existed);

                var (map, targetMargin) = await ContextAsync(userId);
                return new(ToDetail(existed, RecipeInputBuilder.Calculate(existed, map, targetMargin)), 200, "Receita atualizada com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }

        public async Task<ResponseApi<dynamic?>> DuplicateAsync(string id, string userId)
        {
            try
            {
                Recipe? existed = await repository.GetByIdAsync(id, userId);
                if (existed is null) return new(null, 404, "Receita não encontrada");

                Recipe copy = new()
                {
                    UserId = userId,
                    Name = $"{existed.Name} (cópia)",
                    Category = existed.Category,
                    Description = existed.Description,
                    Photo = existed.Photo,
                    Items = existed.Items.Select(i => new RecipeItem { IngredientId = i.IngredientId, Quantity = i.Quantity, Unit = i.Unit }).ToList(),
                    YieldQuantity = existed.YieldQuantity,
                    YieldUnit = existed.YieldUnit,
                    Portions = existed.Portions,
                    PackagingCost = existed.PackagingCost,
                    OtherCosts = existed.OtherCosts.Select(o => new RecipeOtherCost { Name = o.Name, Value = o.Value, AppliesTo = o.AppliesTo }).ToList(),
                    SalePrice = existed.SalePrice,
                    Fees = existed.Fees.Select(f => new RecipeFee { Name = f.Name, Type = f.Type, Value = f.Value }).ToList(),
                    ShowInMenu = false,
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await repository.CreateAsync(copy);
                return new(new { id = copy.Id, name = copy.Name }, 201, "Receita duplicada com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }

        public async Task<ResponseApi<dynamic?>> ToggleMenuAsync(string id, string userId)
        {
            try
            {
                Recipe? existed = await repository.GetByIdAsync(id, userId);
                if (existed is null) return new(null, 404, "Receita não encontrada");

                existed.ShowInMenu = !existed.ShowInMenu;
                existed.UpdatedAt = DateTime.UtcNow;
                existed.UpdatedBy = userId;
                await repository.UpdateAsync(existed);

                return new(new { id = existed.Id, showInMenu = existed.ShowInMenu }, 200,
                    existed.ShowInMenu ? "Prato visível no cardápio" : "Prato oculto do cardápio");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }

        public async Task<ResponseApi<dynamic?>> UploadPhotoAsync(string id, IFormFile file, string userId)
        {
            try
            {
                string? error = StoreService.ValidateImage(file);
                if (error is not null) return new(null, 400, error);

                Recipe? existed = await repository.GetByIdAsync(id, userId);
                if (existed is null) return new(null, 404, "Receita não encontrada");

                string url = await uploadHelper.SaveFileAsync(file, $"recipes/{userId}");
                if (string.IsNullOrEmpty(url)) return new(null, 400, "Falha ao enviar imagem. Verifique a configuração do armazenamento.");

                existed.Photo = url;
                existed.UpdatedAt = DateTime.UtcNow;
                existed.UpdatedBy = userId;
                await repository.UpdateAsync(existed);

                return new(new { id = existed.Id, photo = url }, 200, "Foto atualizada com sucesso");
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
                Recipe? existed = await repository.GetByIdAsync(request.Id, userId);
                if (existed is null) return new(null, 404, "Receita não encontrada");

                existed.Deleted = true;
                existed.DeletedBy = request.DeletedBy;
                existed.DeletedAt = DateTime.UtcNow;
                await repository.UpdateAsync(existed);

                return new(new { id = existed.Id }, 200, "Receita excluída com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }
        #endregion
    }
}
