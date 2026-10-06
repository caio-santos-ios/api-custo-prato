using api_custo_prato.src.Models;

namespace api_custo_prato.src.Calculations
{
    /// <summary>Monta a entrada do motor de cálculo a partir da receita gravada + ingredientes atuais.</summary>
    public static class RecipeInputBuilder
    {
        public static RecipeInput Build(Recipe recipe, IReadOnlyDictionary<string, Ingredient> ingredients, decimal targetMargin)
        {
            return new RecipeInput
            {
                Name = recipe.Name,
                YieldQuantity = recipe.YieldQuantity,
                YieldUnit = recipe.YieldUnit,
                Portions = recipe.Portions,
                PackagingCost = recipe.PackagingCost,
                SalePrice = recipe.SalePrice,
                TargetMargin = targetMargin,
                OtherCosts = recipe.OtherCosts.Select(o => new OtherCostInput { Name = o.Name, Value = o.Value, AppliesTo = o.AppliesTo }).ToList(),
                Fees = recipe.Fees.Select(f => new FeeInput { Name = f.Name, Type = f.Type, Value = f.Value }).ToList(),
                Items = recipe.Items.Select(item =>
                {
                    ingredients.TryGetValue(item.IngredientId, out Ingredient? ing);
                    return new RecipeItemInput
                    {
                        IngredientId = item.IngredientId,
                        Quantity = item.Quantity,
                        Unit = item.Unit,
                        Missing = ing is null,
                        Name = ing?.Name ?? "Ingrediente removido",
                        Category = ing?.Category ?? "",
                        PurchaseQuantity = ing?.PurchaseQuantity ?? 0,
                        PurchaseUnit = ing?.PurchaseUnit ?? item.Unit,
                        PurchasePrice = ing?.PurchasePrice ?? 0,
                        YieldPercent = ing?.YieldPercent ?? 100
                    };
                }).ToList()
            };
        }

        public static RecipeCostResult Calculate(Recipe recipe, IReadOnlyDictionary<string, Ingredient> ingredients, decimal targetMargin)
            => CostCalculator.CalculateRecipe(Build(recipe, ingredients, targetMargin));
    }
}
