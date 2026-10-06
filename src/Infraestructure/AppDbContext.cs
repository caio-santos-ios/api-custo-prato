using MongoDB.Driver;
using api_custo_prato.src.Models;

namespace api_custo_prato.src.Infraestructure
{
    public class AppDbContext
    {
        public static string? ConnectionString { get; set; }
        public static string? DatabaseName { get; set; }
        public static bool IsSSL { get; set; }
        private IMongoDatabase Database { get; }

        public AppDbContext()
        {
            try
            {
                MongoClientSettings mongoClientSettings = MongoClientSettings.FromUrl(new MongoUrl(ConnectionString));
                if (IsSSL)
                {
                    mongoClientSettings.SslSettings = new SslSettings
                    {
                        EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12
                    };
                }

                var mongoClient = new MongoClient(mongoClientSettings);
                Database = mongoClient.GetDatabase(DatabaseName);
                CreateIndexes();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to connect to database. Error: {ex.Message}");
            }
        }

        public IMongoCollection<User> Users => Database.GetCollection<User>("users");
        public IMongoCollection<Store> Stores => Database.GetCollection<Store>("stores");
        public IMongoCollection<Ingredient> Ingredients => Database.GetCollection<Ingredient>("ingredients");
        public IMongoCollection<Recipe> Recipes => Database.GetCollection<Recipe>("recipes");
        public IMongoCollection<StockEntry> StockEntries => Database.GetCollection<StockEntry>("stock_entries");
        public IMongoCollection<GenericTable> GenericTables => Database.GetCollection<GenericTable>("generic_tables");

        private void CreateIndexes()
        {
            try
            {
                Users.Indexes.CreateOne(new CreateIndexModel<User>(Builders<User>.IndexKeys.Ascending(x => x.Email)));
                Stores.Indexes.CreateOne(new CreateIndexModel<Store>(Builders<Store>.IndexKeys.Ascending(x => x.Slug)));
                Stores.Indexes.CreateOne(new CreateIndexModel<Store>(Builders<Store>.IndexKeys.Ascending(x => x.UserId)));
                Ingredients.Indexes.CreateOne(new CreateIndexModel<Ingredient>(Builders<Ingredient>.IndexKeys.Ascending(x => x.UserId).Ascending(x => x.Deleted)));
                Recipes.Indexes.CreateOne(new CreateIndexModel<Recipe>(Builders<Recipe>.IndexKeys.Ascending(x => x.UserId).Ascending(x => x.Deleted)));
                StockEntries.Indexes.CreateOne(new CreateIndexModel<StockEntry>(Builders<StockEntry>.IndexKeys.Ascending(x => x.UserId).Ascending(x => x.IngredientId).Descending(x => x.PurchaseDate)));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Falha ao criar índices: {ex.Message}");
            }
        }
    }
}
