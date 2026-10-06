using MongoDB.Bson.Serialization.Attributes;

namespace api_custo_prato.src.Models
{
    /// <summary>Base para todas as entidades que pertencem a um usuário (isolamento de dados por conta).</summary>
    public class UserModelBase : ModelBase
    {
        [BsonElement("userId")]
        public string UserId { get; set; } = string.Empty;
    }
}
