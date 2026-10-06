using api_custo_prato.src.Calculations;
using api_custo_prato.src.Interfaces;
using api_custo_prato.src.Models;
using api_custo_prato.src.Models.Base;

namespace api_custo_prato.src.Services
{
    public class DashboardService(
        IRecipeRepository recipeRepository,
        IIngredientRepository ingredientRepository,
        IStoreService storeService
    ) : IDashboardService
    {
        public async Task<ResponseApi<dynamic?>> GetAsync(string userId)
        {
            try
            {
                List<Recipe> recipes = await recipeRepository.GetAllByUserAsync(userId);
                List<Ingredient> ingredients = await ingredientRepository.GetAllByUserAsync(userId);
                Store store = await storeService.EnsureStoreAsync(userId);
                Dictionary<string, Ingredient> map = ingredients.ToDictionary(i => i.Id);

                var computed = recipes
                    .Select(r => new { recipe = r, calc = RecipeInputBuilder.Calculate(r, map, store.TargetMargin) })
                    .ToList();

                var priced = computed.Where(x => x.calc.SalePrice > 0).ToList();

                object Row(Recipe r, RecipeCostResult c) => new
                {
                    id = r.Id,
                    name = r.Name,
                    category = r.Category,
                    salePrice = c.SalePrice,
                    costPerPortion = c.CostPerPortion,
                    grossProfit = c.GrossProfit,
                    margin = c.Margin,
                    costPercent = c.CostPercent,
                    suggestedPrice = c.SuggestedPrice
                };

                return new(new
                {
                    store = new { store.Name, store.Slug, store.TargetMargin, store.MenuEnabled, hasWhatsapp = !string.IsNullOrEmpty(store.Whatsapp) },
                    totals = new
                    {
                        recipes = recipes.Count,
                        ingredients = ingredients.Count,
                        inMenu = recipes.Count(r => r.ShowInMenu && r.Active && r.SalePrice > 0),
                        withoutPrice = computed.Count(x => x.calc.SalePrice <= 0),
                        withLoss = priced.Count(x => x.calc.GrossProfit < 0),
                        belowTarget = priced.Count(x => x.calc.Margin < store.TargetMargin),
                        averageMargin = priced.Count > 0 ? CostCalculator.Pct(priced.Average(x => x.calc.Margin)) : 0,
                        averageCostPercent = priced.Count > 0 ? CostCalculator.Pct(priced.Average(x => x.calc.CostPercent)) : 0
                    },
                    bestMargins = priced.OrderByDescending(x => x.calc.Margin).Take(5).Select(x => Row(x.recipe, x.calc)),
                    worstMargins = priced.OrderBy(x => x.calc.Margin).Take(5).Select(x => Row(x.recipe, x.calc)),
                    attention = priced
                        .Where(x => x.calc.Margin < store.TargetMargin)
                        .OrderBy(x => x.calc.Margin)
                        .Take(10)
                        .Select(x => Row(x.recipe, x.calc)),
                    topIngredients = computed
                        .SelectMany(x => x.calc.Items.Select(i => new { i.IngredientId, i.Name, i.Cost }))
                        .GroupBy(i => i.IngredientId)
                        .Select(g => new { id = g.Key, name = g.First().Name, recipes = g.Count(), totalCost = CostCalculator.Money(g.Sum(i => i.Cost)) })
                        .OrderByDescending(x => x.recipes)
                        .ThenByDescending(x => x.totalCost)
                        .Take(5)
                }, 200, "Dashboard carregado com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }
    }
}
