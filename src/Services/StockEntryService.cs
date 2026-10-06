using api_custo_prato.src.Calculations;
using api_custo_prato.src.Interfaces;
using api_custo_prato.src.Models;
using api_custo_prato.src.Models.Base;
using api_custo_prato.src.Requests.Stock;
using api_custo_prato.src.Shared.DTOs;
using api_custo_prato.src.Shared.Utils;

namespace api_custo_prato.src.Services
{
    public class StockEntryService(
        IStockEntryRepository repository,
        IIngredientRepository ingredientRepository,
        IRecipeRepository recipeRepository
    ) : IStockEntryService
    {
        public static object ToResponse(StockEntry e) => new
        {
            id = e.Id,
            ingredientId = e.IngredientId,
            ingredientName = e.IngredientName,
            quantity = e.Quantity,
            unit = e.Unit,
            totalPrice = e.TotalPrice,
            unitCost = e.UnitCost,
            supplier = e.Supplier,
            purchaseDate = e.PurchaseDate,
            notes = e.Notes,
            createdAt = e.CreatedAt
        };

        public async Task<ResponseApi<PaginationApi<List<dynamic>>>> GetAllAsync(GetAllDTO request, string userId)
        {
            try
            {
                PaginationUtil<StockEntry> pagination = new(request.QueryParams);
                List<StockEntry> list = await repository.GetAllAsync(pagination, userId);
                int count = await repository.GetCountDocumentsAsync(pagination, userId);

                List<dynamic> items = list.Select(ToResponse).ToList();
                return new(new(items, count, pagination.PageNumber, pagination.Limit), 200, "Entradas de estoque carregadas com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Erro ao listar entradas de estoque: {ex.Message}");
            }
        }

        public async Task<ResponseApi<List<dynamic>>> GetByIngredientIdAsync(string ingredientId, string userId)
        {
            try
            {
                List<StockEntry> list = await repository.GetByIngredientIdAsync(ingredientId, userId);
                List<dynamic> items = list.Select(ToResponse).ToList();
                return new(items, 200, "Histórico de compras do ingrediente carregado com sucesso");
            }
            catch (Exception ex)
            {
                return new([], 500, $"Erro ao carregar histórico: {ex.Message}");
            }
        }

        public async Task<ResponseApi<dynamic?>> CreateAsync(CreateStockEntryRequest request, string userId)
        {
            try
            {
                if (request == null) return new(null, 400, "Dados da compra inválidos.");

                Ingredient? ingredient = await ingredientRepository.GetByIdAsync(request.IngredientId, userId);
                if (ingredient == null) return new(null, 404, "Ingrediente não encontrado.");

                if (request.Quantity <= 0) return new(null, 400, "A quantidade comprada deve ser maior que zero.");
                if (request.TotalPrice < 0) return new(null, 400, "O preço total não pode ser negativo.");

                if (!UnitConverter.IsValid(request.Unit)) return new(null, 400, "Unidade inválida.");

                string normalizedUnit = UnitConverter.Normalize(request.Unit);

                // Se a unidade for de família diferente da atual do ingrediente (ex: era 'kg' mas comprou 'un' ou 'L')
                if (!UnitConverter.AreCompatible(normalizedUnit, ingredient.PurchaseUnit))
                {
                    // Se o ingrediente ainda não tem estoque acumulado ou não está vinculado a receitas,
                    // adota a nova unidade informada na compra sem travar o usuário!
                    List<Recipe> usingRecipes = await recipeRepository.GetUsingIngredientAsync(ingredient.Id, userId);
                    if (ingredient.CurrentStock <= 0 && usingRecipes.Count == 0)
                    {
                        ingredient.PurchaseUnit = normalizedUnit;
                    }
                    else
                    {
                        return new(null, 400, $"Unidade {request.Unit} incompatível com a unidade padrão do ingrediente ({ingredient.PurchaseUnit}).");
                    }
                }

                decimal unitCost = request.Quantity > 0 ? request.TotalPrice / request.Quantity : 0;

                // Converte a quantidade comprada para a unidade de compra base do ingrediente
                decimal convertedQuantity = UnitConverter.Convert(request.Quantity, normalizedUnit, ingredient.PurchaseUnit);

                StockEntry entry = new()
                {
                    UserId = userId,
                    CreatedBy = userId,
                    IngredientId = ingredient.Id,
                    IngredientName = ingredient.Name,
                    Quantity = request.Quantity,
                    Unit = normalizedUnit,
                    TotalPrice = request.TotalPrice,
                    UnitCost = unitCost,
                    Supplier = request.Supplier?.Trim() ?? string.Empty,
                    PurchaseDate = request.PurchaseDate ?? DateTime.UtcNow,
                    Notes = request.Notes?.Trim() ?? string.Empty
                };

                await repository.CreateAsync(entry);

                // Incrementa estoque atual
                ingredient.CurrentStock += convertedQuantity;

                // Se marcado para atualizar custo, atualiza o preço de referência do ingrediente
                if (request.UpdateIngredientCost)
                {
                    ingredient.PurchaseQuantity = convertedQuantity > 0 ? convertedQuantity : 1;
                    ingredient.PurchasePrice = request.TotalPrice;
                    ingredient.PurchaseUnit = ingredient.PurchaseUnit;
                }

                ingredient.UpdatedAt = DateTime.UtcNow;
                ingredient.UpdatedBy = userId;
                await ingredientRepository.UpdateAsync(ingredient);

                return new(ToResponse(entry), 201, $"Entrada de {request.Quantity} {normalizedUnit} de \"{ingredient.Name}\" registrada com sucesso!");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Erro ao registrar compra: {ex.Message}");
            }
        }

        public async Task<ResponseApi<dynamic?>> DeleteAsync(string id, string userId)
        {
            try
            {
                StockEntry? entry = await repository.GetByIdAsync(id, userId);
                if (entry == null) return new(null, 404, "Entrada não encontrada.");

                Ingredient? ingredient = await ingredientRepository.GetByIdAsync(entry.IngredientId, userId);
                if (ingredient != null)
                {
                    decimal convertedQuantity = UnitConverter.Convert(entry.Quantity, entry.Unit, ingredient.PurchaseUnit);
                    ingredient.CurrentStock = Math.Max(0, ingredient.CurrentStock - convertedQuantity);
                    ingredient.UpdatedAt = DateTime.UtcNow;
                    ingredient.UpdatedBy = userId;
                    await ingredientRepository.UpdateAsync(ingredient);
                }

                await repository.DeleteAsync(id, userId);
                return new(null, 200, "Entrada de estoque removida com sucesso.");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Erro ao remover entrada: {ex.Message}");
            }
        }
    }
}
