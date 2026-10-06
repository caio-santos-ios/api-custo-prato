namespace api_custo_prato.src.Calculations
{
    /// <summary>
    /// Motor de cálculo de custos. Puro (sem banco), usado pelas receitas, pelo simulador e pelos testes.
    /// </summary>
    public static class CostCalculator
    {
        private const int TopXRaySlices = 5;

        public static decimal Money(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
        public static decimal Precise(decimal value) => Math.Round(value, 6, MidpointRounding.AwayFromZero);
        public static decimal Pct(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

        /// <summary>Aproveitamento válido entre 0 (exclusivo) e 100. Fora disso considera 100%.</summary>
        public static decimal SafeYield(decimal yieldPercent) => yieldPercent > 0 && yieldPercent <= 100 ? yieldPercent : 100m;

        public static decimal CorrectionFactor(decimal yieldPercent) => 100m / SafeYield(yieldPercent);

        #region INGREDIENT
        /// <summary>
        /// Ex.: 5 kg por R$ 100 → R$ 20/kg e R$ 0,02/g.
        /// Com 70% de aproveitamento → FC 1,4286 → R$ 28,57/kg aproveitado.
        /// </summary>
        public static IngredientCostResult CalculateIngredient(decimal purchaseQuantity, string purchaseUnit, decimal purchasePrice, decimal yieldPercent)
        {
            decimal baseQty = UnitConverter.ToBase(purchaseQuantity, purchaseUnit);
            decimal fc = CorrectionFactor(yieldPercent);

            if (baseQty <= 0 || purchaseQuantity <= 0)
            {
                return new() { BaseUnit = UnitConverter.BaseUnitOf(purchaseUnit), CorrectionFactor = Precise(fc) };
            }

            decimal costPerBase = purchasePrice / baseQty;
            decimal costPerPurchaseUnit = purchasePrice / purchaseQuantity;

            return new()
            {
                BaseUnit = UnitConverter.BaseUnitOf(purchaseUnit),
                CostPerPurchaseUnit = Precise(costPerPurchaseUnit),
                CostPerBaseUnit = Precise(costPerBase),
                CorrectionFactor = Precise(fc),
                RealCostPerPurchaseUnit = Precise(costPerPurchaseUnit * fc),
                RealCostPerBaseUnit = Precise(costPerBase * fc)
            };
        }

        /// <summary>Custo (sem arredondar) da quantidade líquida usada, já com fator de correção.</summary>
        public static decimal ItemCostRaw(RecipeItemInput item)
        {
            if (!UnitConverter.AreCompatible(item.Unit, item.PurchaseUnit)) return 0;
            decimal purchaseBase = UnitConverter.ToBase(item.PurchaseQuantity, item.PurchaseUnit);
            if (purchaseBase <= 0) return 0;

            decimal costPerBase = item.PurchasePrice / purchaseBase;
            decimal usedBase = UnitConverter.ToBase(item.Quantity, item.Unit);
            return usedBase * costPerBase * CorrectionFactor(item.YieldPercent);
        }
        #endregion

        #region RECIPE
        public static RecipeCostResult CalculateRecipe(RecipeInput input)
        {
            RecipeCostResult result = new();

            decimal portions = input.Portions;
            if (portions <= 0)
            {
                portions = 1;
                result.Warnings.Add("Número de porções inválido. Considerado 1 porção.");
            }

            // ---- Ingredientes
            List<(RecipeItemResult item, decimal raw)> items = [];
            foreach (RecipeItemInput i in input.Items)
            {
                decimal fc = CorrectionFactor(i.YieldPercent);
                string warning = "";

                if (i.Missing)
                    warning = "Ingrediente não encontrado (foi excluído). Remova ou substitua na receita.";
                else if (!UnitConverter.AreCompatible(i.Unit, i.PurchaseUnit))
                    warning = $"Unidade {i.Unit} incompatível com a unidade de compra ({i.PurchaseUnit}).";
                else if (i.PurchaseQuantity <= 0)
                    warning = "Quantidade comprada do ingrediente não informada.";

                decimal raw = warning == "" ? ItemCostRaw(i) : 0;
                if (warning != "") result.Warnings.Add($"{i.Name}: {warning}");

                items.Add((new RecipeItemResult
                {
                    IngredientId = i.IngredientId,
                    Name = i.Name,
                    Category = i.Category,
                    Quantity = i.Quantity,
                    Unit = UnitConverter.Normalize(i.Unit),
                    PurchaseQuantity = i.PurchaseQuantity,
                    PurchaseUnit = UnitConverter.Normalize(i.PurchaseUnit),
                    PurchasePrice = i.PurchasePrice,
                    YieldPercent = SafeYield(i.YieldPercent),
                    CorrectionFactor = Math.Round(fc, 4),
                    GrossQuantity = Math.Round(i.Quantity * fc, 3),
                    CostPerPurchaseUnit = i.PurchaseQuantity > 0 ? Precise(i.PurchasePrice / i.PurchaseQuantity) : 0,
                    Cost = Money(raw),
                    Warning = warning
                }, raw));
            }

            decimal ingredientsCost = items.Sum(x => x.raw);

            // ---- Custos adicionais
            decimal packagingTotal = Math.Max(input.PackagingCost, 0) * portions;
            decimal otherRecipe = input.OtherCosts.Where(o => o.AppliesTo != "portion").Sum(o => o.Value);
            decimal otherPortion = input.OtherCosts.Where(o => o.AppliesTo == "portion").Sum(o => o.Value);
            decimal otherPortionTotal = otherPortion * portions;
            decimal additional = packagingTotal + otherRecipe + otherPortionTotal;

            decimal total = ingredientsCost + additional;
            decimal cpp = total / portions;

            // ---- Preço, taxas e margem
            decimal price = Math.Max(input.SalePrice, 0);
            decimal feesPercent = input.Fees.Where(f => f.Type != "fixed").Sum(f => f.Value);
            decimal feesFixed = input.Fees.Where(f => f.Type == "fixed").Sum(f => f.Value);
            decimal feesValue = price * feesPercent / 100m + feesFixed;
            decimal profit = price - cpp - feesValue;

            result.Fees = input.Fees.Select(f => new FeeResult
            {
                Name = f.Name,
                Type = f.Type == "fixed" ? "fixed" : "percent",
                Value = f.Value,
                Amount = Money(f.Type == "fixed" ? f.Value : price * f.Value / 100m)
            }).ToList();

            // ---- Itens: participação
            foreach ((RecipeItemResult item, decimal raw) in items)
            {
                item.CostPerPortion = Money(raw / portions);
                item.Percent = cpp > 0 ? Pct(raw / portions / cpp * 100m) : 0;
            }

            result.Items = items.Select(x => x.item).ToList();
            result.IngredientsCost = Money(ingredientsCost);
            result.PackagingCost = Money(input.PackagingCost);
            result.PackagingTotal = Money(packagingTotal);
            result.OtherCostsRecipe = Money(otherRecipe);
            result.OtherCostsPortion = Money(otherPortion);
            result.AdditionalCosts = Money(additional);
            result.TotalCost = Money(total);
            result.Portions = portions;
            result.CostPerPortion = Money(cpp);
            result.IngredientsCostPerPortion = Money(ingredientsCost / portions);
            result.AdditionalCostsPerPortion = Money(additional / portions);
            result.YieldQuantity = input.YieldQuantity;
            result.YieldUnit = UnitConverter.Normalize(input.YieldUnit);
            result.PortionSize = input.YieldQuantity > 0 ? Math.Round(input.YieldQuantity / portions, 3) : 0;
            result.SalePrice = Money(price);
            result.FeesPercent = Pct(feesPercent);
            result.FeesFixed = Money(feesFixed);
            result.FeesValue = Money(feesValue);
            result.NetRevenue = Money(price - feesValue);
            result.GrossProfit = Money(profit);
            result.Margin = price > 0 ? Pct(profit / price * 100m) : 0;
            result.CostPercent = price > 0 ? Pct(cpp / price * 100m) : 0;
            result.Markup = cpp > 0 ? Math.Round(price / cpp, 2) : 0;
            result.TargetMargin = input.TargetMargin;
            result.SuggestedPrice = SuggestedPrice(cpp, feesPercent, feesFixed, input.TargetMargin);
            result.XRay = BuildXRay(items, input, portions, cpp);

            if (price <= 0) result.Warnings.Add("Preço de venda não informado.");
            else if (profit < 0) result.Warnings.Add("Este prato está dando prejuízo com o preço atual.");

            return result;
        }

        /// <summary>Preço necessário para atingir a margem desejada: (custo + taxas fixas) / (1 − taxas% − margem%).</summary>
        public static decimal? SuggestedPrice(decimal costPerPortion, decimal feesPercent, decimal feesFixed, decimal targetMargin)
        {
            decimal denominator = 1m - (feesPercent + targetMargin) / 100m;
            if (denominator <= 0 || costPerPortion <= 0) return null;
            return Money((costPerPortion + feesFixed) / denominator);
        }

        /// <summary>Raio-X do prato: o que mais pesa no custo da porção (top 5 + "Outros").</summary>
        private static List<XRaySlice> BuildXRay(List<(RecipeItemResult item, decimal raw)> items, RecipeInput input, decimal portions, decimal cpp)
        {
            if (cpp <= 0) return [];

            List<XRaySlice> slices = items
                .GroupBy(x => string.IsNullOrEmpty(x.item.IngredientId) ? x.item.Name : x.item.IngredientId)
                .Select(g => new XRaySlice
                {
                    Label = g.First().item.Name,
                    Type = "ingredient",
                    Value = g.Sum(x => x.raw) / portions
                })
                .ToList();

            if (input.PackagingCost > 0)
                slices.Add(new() { Label = "Embalagem", Type = "packaging", Value = input.PackagingCost });

            foreach (OtherCostInput o in input.OtherCosts.Where(o => o.Value > 0))
            {
                slices.Add(new()
                {
                    Label = string.IsNullOrWhiteSpace(o.Name) ? "Outros custos" : o.Name,
                    Type = "other",
                    Value = o.AppliesTo == "portion" ? o.Value : o.Value / portions
                });
            }

            slices = slices.Where(s => s.Value > 0).OrderByDescending(s => s.Value).ToList();

            List<XRaySlice> top = slices.Take(TopXRaySlices).ToList();
            decimal rest = slices.Skip(TopXRaySlices).Sum(s => s.Value);
            if (rest > 0) top.Add(new() { Label = "Outros", Type = "others", Value = rest });

            foreach (XRaySlice s in top)
            {
                s.Percent = Pct(s.Value / cpp * 100m);
                s.Value = Money(s.Value);
            }

            return top;
        }
        #endregion
    }
}
