using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace api_custo_prato.src.Models
{
    /// <summary>Dados do estabelecimento (1 por usuário). Usado no cardápio público e nos valores padrão.</summary>
    public class Store : UserModelBase
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>Identificador do link público: /c/{slug}</summary>
        [BsonElement("slug")]
        public string Slug { get; set; } = string.Empty;

        [BsonElement("whatsapp")]
        public string Whatsapp { get; set; } = string.Empty;

        [BsonElement("logo")]
        public string Logo { get; set; } = string.Empty;

        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;

        [BsonElement("deliveryFee")]
        public decimal DeliveryFee { get; set; } = 0;

        [BsonElement("paymentMethods")]
        public List<string> PaymentMethods { get; set; } = ["Pix", "Cartão de crédito", "Cartão de débito", "Dinheiro"];

        /// <summary>Margem desejada (%) usada para alertas e preço sugerido.</summary>
        [BsonElement("targetMargin")]
        public decimal TargetMargin { get; set; } = 30;

        /// <summary>Taxas aplicadas por padrão em novas receitas (cartão, delivery...).</summary>
        [BsonElement("defaultFees")]
        public List<RecipeFee> DefaultFees { get; set; } = [];

        [BsonElement("menuEnabled")]
        public bool MenuEnabled { get; set; } = true;
    }
}
