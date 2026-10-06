using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace api_custo_prato.src.Models
{
    /// <summary>Ficha técnica. Os custos NÃO são gravados: são calculados na leitura a partir dos ingredientes atuais.</summary>
    public class Recipe : UserModelBase
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("category")]
        public string Category { get; set; } = string.Empty;

        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;

        [BsonElement("photo")]
        public string Photo { get; set; } = string.Empty;

        [BsonElement("items")]
        public List<RecipeItem> Items { get; set; } = [];

        /// <summary>Rendimento total da receita (ex.: 3 kg)</summary>
        [BsonElement("yieldQuantity")]
        public decimal YieldQuantity { get; set; } = 0;

        [BsonElement("yieldUnit")]
        public string YieldUnit { get; set; } = "kg";

        /// <summary>Número de porções que a receita rende.</summary>
        [BsonElement("portions")]
        public decimal Portions { get; set; } = 1;

        /// <summary>Custo da embalagem por porção.</summary>
        [BsonElement("packagingCost")]
        public decimal PackagingCost { get; set; } = 0;

        [BsonElement("otherCosts")]
        public List<RecipeOtherCost> OtherCosts { get; set; } = [];

        /// <summary>Preço de venda por porção.</summary>
        [BsonElement("salePrice")]
        public decimal SalePrice { get; set; } = 0;

        [BsonElement("fees")]
        public List<RecipeFee> Fees { get; set; } = [];

        /// <summary>Exibir no cardápio público.</summary>
        [BsonElement("showInMenu")]
        public bool ShowInMenu { get; set; } = true;
    }

    public class RecipeItem
    {
        [BsonElement("ingredientId")]
        public string IngredientId { get; set; } = string.Empty;

        /// <summary>Quantidade líquida (aproveitada) usada na receita.</summary>
        [BsonElement("quantity")]
        public decimal Quantity { get; set; } = 0;

        [BsonElement("unit")]
        public string Unit { get; set; } = "g";
    }

    public class RecipeOtherCost
    {
        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("value")]
        public decimal Value { get; set; } = 0;

        /// <summary>"recipe" = valor para a receita inteira; "portion" = valor por porção.</summary>
        [BsonElement("appliesTo")]
        public string AppliesTo { get; set; } = "recipe";
    }

    public class RecipeFee
    {
        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>"percent" = % sobre o preço de venda; "fixed" = valor fixo (R$) por venda.</summary>
        [BsonElement("type")]
        public string Type { get; set; } = "percent";

        [BsonElement("value")]
        public decimal Value { get; set; } = 0;
    }
}
