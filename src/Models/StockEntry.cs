using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace api_custo_prato.src.Models
{
    /// <summary>
    /// Registro de compra / entrada de mercadoria no estoque.
    /// Mantém o histórico de reposições e atualiza o preço de custo mais recente do ingrediente.
    /// </summary>
    public class StockEntry : UserModelBase
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("ingredientId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string IngredientId { get; set; } = string.Empty;

        [BsonElement("ingredientName")]
        public string IngredientName { get; set; } = string.Empty;

        /// <summary>Quantidade comprada/reposta nesta entrada.</summary>
        [BsonElement("quantity")]
        public decimal Quantity { get; set; } = 0;

        /// <summary>Unidade comprada: kg, g, L, ml, un</summary>
        [BsonElement("unit")]
        public string Unit { get; set; } = "kg";

        /// <summary>Valor total pago nesta compra (R$).</summary>
        [BsonElement("totalPrice")]
        public decimal TotalPrice { get; set; } = 0;

        /// <summary>Custo unitário calculado nesta compra (R$ por unidade).</summary>
        [BsonElement("unitCost")]
        public decimal UnitCost { get; set; } = 0;

        /// <summary>Fornecedor ou local da compra (opcional).</summary>
        [BsonElement("supplier")]
        public string Supplier { get; set; } = string.Empty;

        /// <summary>Data da compra.</summary>
        [BsonElement("purchaseDate")]
        public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;

        [BsonElement("notes")]
        public string Notes { get; set; } = string.Empty;
    }
}
