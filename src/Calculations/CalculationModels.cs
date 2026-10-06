namespace api_custo_prato.src.Calculations
{
    #region INPUTS
    /// <summary>Dados de um ingrediente necessários para o cálculo (vindos do banco ou da sandbox do simulador).</summary>
    public class IngredientInput
    {
        public string IngredientId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal PurchaseQuantity { get; set; }
        public string PurchaseUnit { get; set; } = "kg";
        public decimal PurchasePrice { get; set; }
        public decimal YieldPercent { get; set; } = 100;
    }

    public class RecipeItemInput : IngredientInput
    {
        /// <summary>Quantidade líquida (aproveitada) usada na receita.</summary>
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = "g";
        /// <summary>Ingrediente não encontrado (excluído).</summary>
        public bool Missing { get; set; }
    }

    public class OtherCostInput
    {
        public string Name { get; set; } = string.Empty;
        public decimal Value { get; set; }
        /// <summary>"recipe" ou "portion"</summary>
        public string AppliesTo { get; set; } = "recipe";
    }

    public class FeeInput
    {
        public string Name { get; set; } = string.Empty;
        /// <summary>"percent" ou "fixed"</summary>
        public string Type { get; set; } = "percent";
        public decimal Value { get; set; }
    }

    public class RecipeInput
    {
        public string Name { get; set; } = string.Empty;
        public List<RecipeItemInput> Items { get; set; } = [];
        public decimal YieldQuantity { get; set; }
        public string YieldUnit { get; set; } = "kg";
        public decimal Portions { get; set; } = 1;
        public decimal PackagingCost { get; set; }
        public List<OtherCostInput> OtherCosts { get; set; } = [];
        public decimal SalePrice { get; set; }
        public List<FeeInput> Fees { get; set; } = [];
        /// <summary>Margem desejada (%) para o preço sugerido.</summary>
        public decimal TargetMargin { get; set; } = 30;
    }

    public class GoalInput
    {
        public decimal DailySales { get; set; }
        public decimal DaysPerMonth { get; set; } = 26;
        public decimal TargetMonthlyProfit { get; set; }
        /// <summary>Cenário personalizado: variação % do preço de venda.</summary>
        public decimal PriceChangePercent { get; set; }
        /// <summary>Cenário personalizado: variação % da quantidade vendida.</summary>
        public decimal SalesChangePercent { get; set; }
        /// <summary>Cenário personalizado: variação % do custo por porção.</summary>
        public decimal CostChangePercent { get; set; }
    }
    #endregion

    #region OUTPUTS
    public class IngredientCostResult
    {
        public string BaseUnit { get; set; } = string.Empty;
        /// <summary>Custo por unidade de compra, sem perdas (ex.: R$ 20/kg).</summary>
        public decimal CostPerPurchaseUnit { get; set; }
        /// <summary>Custo por unidade base, sem perdas (ex.: R$ 0,02/g).</summary>
        public decimal CostPerBaseUnit { get; set; }
        /// <summary>Fator de correção = 100 / aproveitamento.</summary>
        public decimal CorrectionFactor { get; set; }
        /// <summary>Custo real por unidade de compra aproveitada (ex.: R$ 42,86/kg limpo).</summary>
        public decimal RealCostPerPurchaseUnit { get; set; }
        public decimal RealCostPerBaseUnit { get; set; }
    }

    public class RecipeItemResult
    {
        public string IngredientId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
        public decimal PurchaseQuantity { get; set; }
        public string PurchaseUnit { get; set; } = string.Empty;
        public decimal PurchasePrice { get; set; }
        public decimal YieldPercent { get; set; }
        public decimal CorrectionFactor { get; set; }
        /// <summary>Quantidade bruta que precisa ser comprada/separada (líquida × FC).</summary>
        public decimal GrossQuantity { get; set; }
        public decimal CostPerPurchaseUnit { get; set; }
        public decimal Cost { get; set; }
        public decimal CostPerPortion { get; set; }
        /// <summary>% do custo da porção.</summary>
        public decimal Percent { get; set; }
        public string Warning { get; set; } = string.Empty;
    }

    public class FeeResult
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public decimal Amount { get; set; }
    }

    public class XRaySlice
    {
        public string Label { get; set; } = string.Empty;
        /// <summary>ingredient | packaging | other | others</summary>
        public string Type { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public decimal Percent { get; set; }
    }

    public class RecipeCostResult
    {
        public List<RecipeItemResult> Items { get; set; } = [];
        public decimal IngredientsCost { get; set; }
        public decimal PackagingCost { get; set; }
        public decimal PackagingTotal { get; set; }
        public decimal OtherCostsRecipe { get; set; }
        public decimal OtherCostsPortion { get; set; }
        /// <summary>Embalagem + outros custos (total da receita).</summary>
        public decimal AdditionalCosts { get; set; }
        public decimal TotalCost { get; set; }
        public decimal Portions { get; set; }
        public decimal CostPerPortion { get; set; }
        public decimal IngredientsCostPerPortion { get; set; }
        public decimal AdditionalCostsPerPortion { get; set; }
        public decimal YieldQuantity { get; set; }
        public string YieldUnit { get; set; } = string.Empty;
        public decimal PortionSize { get; set; }
        public decimal SalePrice { get; set; }
        public List<FeeResult> Fees { get; set; } = [];
        public decimal FeesValue { get; set; }
        public decimal FeesPercent { get; set; }
        public decimal FeesFixed { get; set; }
        public decimal NetRevenue { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal Margin { get; set; }
        public decimal CostPercent { get; set; }
        public decimal Markup { get; set; }
        public decimal TargetMargin { get; set; }
        public decimal? SuggestedPrice { get; set; }
        public List<XRaySlice> XRay { get; set; } = [];
        public List<string> Warnings { get; set; } = [];
    }

    public class GoalScenario
    {
        public string Key { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public decimal SalePrice { get; set; }
        public decimal CostPerPortion { get; set; }
        public decimal DailySales { get; set; }
        public decimal ProfitPerUnit { get; set; }
        public decimal Margin { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public decimal MonthlyProfit { get; set; }
        public decimal? UnitsNeededMonth { get; set; }
        public decimal? UnitsNeededDay { get; set; }
    }

    public class GoalResult
    {
        public decimal DailySales { get; set; }
        public decimal DaysPerMonth { get; set; }
        public decimal MonthlyUnits { get; set; }
        public decimal ProfitPerUnit { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public decimal MonthlyCost { get; set; }
        public decimal MonthlyFees { get; set; }
        public decimal MonthlyProfit { get; set; }
        public decimal TargetMonthlyProfit { get; set; }
        public decimal? UnitsNeededMonth { get; set; }
        public decimal? UnitsNeededDay { get; set; }
        /// <summary>% da meta atingida com a venda média atual.</summary>
        public decimal? GoalProgress { get; set; }
        public bool GoalReachable { get; set; }
        public List<GoalScenario> Scenarios { get; set; } = [];
        public string Disclaimer { get; set; } = "Simulação/estimativa com base nos dados informados. Não é uma previsão garantida de vendas.";
    }
    #endregion
}
