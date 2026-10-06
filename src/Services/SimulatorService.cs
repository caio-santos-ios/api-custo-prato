using api_custo_prato.src.Calculations;
using api_custo_prato.src.Interfaces;
using api_custo_prato.src.Models;
using api_custo_prato.src.Models.Base;
using api_custo_prato.src.Requests.Simulator;

namespace api_custo_prato.src.Services
{
    /// <summary>
    /// Simulador "E se eu mudar isso?". Trabalha SOMENTE sobre a cópia enviada pelo cliente:
    /// este serviço não possui nenhuma operação de escrita, então a receita original nunca é alterada.
    /// </summary>
    public class SimulatorService(
        IRecipeRepository recipeRepository,
        IIngredientRepository ingredientRepository,
        IStoreService storeService
    ) : ISimulatorService
    {
        private static void Sanitize(RecipeInput input)
        {
            input.Portions = input.Portions > 0 ? input.Portions : 1;
            input.PackagingCost = Math.Max(input.PackagingCost, 0);
            input.SalePrice = Math.Max(input.SalePrice, 0);
            input.TargetMargin = Math.Clamp(input.TargetMargin, 0, 99);

            foreach (RecipeItemInput i in input.Items)
            {
                i.Quantity = Math.Max(i.Quantity, 0);
                i.PurchaseQuantity = Math.Max(i.PurchaseQuantity, 0);
                i.PurchasePrice = Math.Max(i.PurchasePrice, 0);
                i.YieldPercent = CostCalculator.SafeYield(i.YieldPercent);
                i.Unit = UnitConverter.Normalize(i.Unit);
                i.PurchaseUnit = UnitConverter.Normalize(i.PurchaseUnit);
            }

            foreach (OtherCostInput o in input.OtherCosts)
            {
                o.Value = Math.Max(o.Value, 0);
                o.AppliesTo = o.AppliesTo == "portion" ? "portion" : "recipe";
            }

            foreach (FeeInput f in input.Fees)
            {
                f.Value = Math.Max(f.Value, 0);
                f.Type = f.Type == "fixed" ? "fixed" : "percent";
            }
        }

        public async Task<ResponseApi<dynamic?>> CalculateAsync(SimulationRequest request, string userId)
        {
            try
            {
                if (request.Recipe is null) return new(null, 400, "Dados da simulação inválidos.");
                Sanitize(request.Recipe);

                RecipeCostResult simulated = CostCalculator.CalculateRecipe(request.Recipe);
                RecipeCostResult? original = null;

                if (!string.IsNullOrWhiteSpace(request.RecipeId))
                {
                    Recipe? recipe = await recipeRepository.GetByIdAsync(request.RecipeId, userId);
                    if (recipe is null) return new(null, 404, "Receita original não encontrada");

                    List<Ingredient> ingredients = await ingredientRepository.GetByIdsAsync(recipe.Items.Select(i => i.IngredientId), userId);
                    Store store = await storeService.EnsureStoreAsync(userId);
                    original = RecipeInputBuilder.Calculate(recipe, ingredients.ToDictionary(i => i.Id), store.TargetMargin);
                }

                GoalResult? simulatedGoals = null;
                GoalResult? originalGoals = null;
                if (request.Goal is not null)
                {
                    simulatedGoals = GoalCalculator.Calculate(simulated, request.Goal);
                    if (original is not null)
                    {
                        // Para o original considera só a venda média/meta (sem as variações do cenário personalizado).
                        GoalInput baseGoal = new()
                        {
                            DailySales = request.Goal.DailySales,
                            DaysPerMonth = request.Goal.DaysPerMonth,
                            TargetMonthlyProfit = request.Goal.TargetMonthlyProfit
                        };
                        originalGoals = GoalCalculator.Calculate(original, baseGoal);
                    }
                }

                return new(new
                {
                    original,
                    simulated,
                    goals = request.Goal is null ? null : new { original = originalGoals, simulated = simulatedGoals },
                    persisted = false
                }, 200, "Simulação calculada (nenhuma alteração foi salva)");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }
    }
}
