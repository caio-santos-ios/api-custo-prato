using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace api_custo_prato.src.Models
{
    public class GenericTable : UserModelBase
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("code")]
        public string Code { get; set; } = string.Empty;
        
        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;
        
        [BsonElement("table")]
        public string Table { get; set; } = string.Empty;
    }
}