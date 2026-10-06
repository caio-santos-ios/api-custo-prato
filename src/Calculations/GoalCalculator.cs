namespace api_custo_prato.src.Calculations
{
    /// <summary>Metas e estimativa de vendas (simulação — não é previsão).</summary>
    public static class GoalCalculator
    {
        private record UnitEconomics(decimal Price, decimal Cost, decimal Fees, decimal Profit);

        private static UnitEconomics Unit(decimal price, decimal cost, decimal feesPercent, decimal feesFixed)
        {
            decimal fees = price * feesPercent / 100m + feesFixed;
            return new(price, cost, fees, price - cost - fees);
        }

        private static decimal? UnitsNeeded(decimal target, decimal profitPerUnit)
            => target > 0 && profitPerUnit > 0 ? Math.Ceiling(target / profitPerUnit) : null;

        private static decimal? PerDay(decimal? monthly, decimal days)
            => monthly is null || days <= 0 ? null : Math.Ceiling(monthly.Value / days);

        public static GoalResult Calculate(RecipeCostResult recipe, GoalInput goal)
        {
            decimal days = goal.DaysPerMonth > 0 ? Math.Min(goal.DaysPerMonth, 31) : 26;
            decimal daily = Math.Max(goal.DailySales, 0);
            decimal monthlyUnits = daily * days;
            decimal target = Math.Max(goal.TargetMonthlyProfit, 0);

            UnitEconomics u = Unit(recipe.SalePrice, recipe.CostPerPortion, recipe.FeesPercent, recipe.FeesFixed);
            decimal monthlyProfit = u.Profit * monthlyUnits;
            decimal? needed = UnitsNeeded(target, u.Profit);

            GoalResult result = new()
            {
                DailySales = daily,
                DaysPerMonth = days,
                MonthlyUnits = monthlyUnits,
                ProfitPerUnit = CostCalculator.Money(u.Profit),
                MonthlyRevenue = CostCalculator.Money(u.Price * monthlyUnits),
                MonthlyCost = CostCalculator.Money(u.Cost * monthlyUnits),
                MonthlyFees = CostCalculator.Money(u.Fees * monthlyUnits),
                MonthlyProfit = CostCalculator.Money(monthlyProfit),
                TargetMonthlyProfit = target,
                UnitsNeededMonth = needed,
                UnitsNeededDay = PerDay(needed, days),
                GoalProgress = target > 0 ? CostCalculator.Pct(monthlyProfit / target * 100m) : null,
                GoalReachable = u.Profit > 0
            };

            List<(string key, string label, decimal priceMul, decimal salesMul, decimal costMul, decimal? fixedPrice)> defs =
            [
                ("current", "Atual", 1m, 1m, 1m, null),
                ("price_up", "Preço +10%", 1.10m, 1m, 1m, null),
                ("price_down", "Preço −10%", 0.90m, 1m, 1m, null),
                ("sales_up", "Vendas +20%", 1m, 1.20m, 1m, null),
                ("sales_down", "Vendas −20%", 1m, 0.80m, 1m, null),
                ("cost_up", "Custo +10%", 1m, 1m, 1.10m, null),
                ("cost_down", "Custo −10%", 1m, 1m, 0.90m, null),
            ];

            if (recipe.SuggestedPrice is not null)
                defs.Add(("target_margin", $"Margem de {recipe.TargetMargin:0.##}%", 1m, 1m, 1m, recipe.SuggestedPrice));

            if (goal.PriceChangePercent != 0 || goal.SalesChangePercent != 0 || goal.CostChangePercent != 0)
            {
                defs.Add(("custom", "Personalizado",
                    1m + goal.PriceChangePercent / 100m,
                    1m + goal.SalesChangePercent / 100m,
                    1m + goal.CostChangePercent / 100m,
                    null));
            }

            foreach (var d in defs)
            {
                decimal price = d.fixedPrice ?? recipe.SalePrice * d.priceMul;
                decimal cost = recipe.CostPerPortion * d.costMul;
                decimal sales = daily * d.salesMul;
                UnitEconomics s = Unit(price, cost, recipe.FeesPercent, recipe.FeesFixed);
                decimal units = sales * days;
                decimal? sNeeded = UnitsNeeded(target, s.Profit);

                result.Scenarios.Add(new GoalScenario
                {
                    Key = d.key,
                    Label = d.label,
                    SalePrice = CostCalculator.Money(price),
                    CostPerPortion = CostCalculator.Money(cost),
                    DailySales = Math.Round(sales, 1),
                    ProfitPerUnit = CostCalculator.Money(s.Profit),
                    Margin = price > 0 ? CostCalculator.Pct(s.Profit / price * 100m) : 0,
                    MonthlyRevenue = CostCalculator.Money(price * units),
                    MonthlyProfit = CostCalculator.Money(s.Profit * units),
                    UnitsNeededMonth = sNeeded,
                    UnitsNeededDay = PerDay(sNeeded, days)
                });
            }

            return result;
        }
    }
}
