using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace api_custo_prato.src.Models
{
    public class Ingredient : UserModelBase
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("category")]
        public string Category { get; set; } = string.Empty;

        /// <summary>Quantidade comprada (ex.: 5)</summary>
        [BsonElement("purchaseQuantity")]
        public decimal PurchaseQuantity { get; set; } = 1;

        /// <summary>Unidade de compra: kg, g, L, ml, un</summary>
        [BsonElement("purchaseUnit")]
        public string PurchaseUnit { get; set; } = "kg";

        /// <summary>Preço pago pela quantidade comprada (ex.: R$ 100)</summary>
        [BsonElement("purchasePrice")]
        public decimal PurchasePrice { get; set; } = 0;

        /// <summary>Aproveitamento em % (ex.: 70 = de 1 kg sobram 700 g). Fator de correção = 100 / aproveitamento.</summary>
        [BsonElement("yieldPercent")]
        public decimal YieldPercent { get; set; } = 100;

        /// <summary>Saldo atual em estoque na unidade de compra.</summary>
        [BsonElement("currentStock")]
        public decimal CurrentStock { get; set; } = 0;

        [BsonElement("notes")]
        public string Notes { get; set; } = string.Empty;
    }
}
