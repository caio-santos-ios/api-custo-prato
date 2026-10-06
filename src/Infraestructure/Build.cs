using api_custo_prato.src.Interfaces;
using api_custo_prato.src.Repository;
using api_custo_prato.src.Services;
using api_custo_prato.src.Infraestructure;
using api_custo_prato.src.Helpers;

namespace api_custo_prato.src.Configuration
{
    public static class Build
    {
        public static void AddBuilderConfiguration(this WebApplicationBuilder builder)
        {
            AppDbContext.ConnectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") ?? "mongodb://localhost:27017";
            AppDbContext.DatabaseName = Environment.GetEnvironmentVariable("DATABASE_NAME") ?? "custo_prato";
            AppDbContext.IsSSL = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("IS_SSL"))
                && Convert.ToBoolean(Environment.GetEnvironmentVariable("IS_SSL"));
        }

        public static void AddContext(this WebApplicationBuilder builder)
        {
            builder.Services.AddSingleton<AppDbContext>();
        }

        public static void AddBuilderHelpers(this WebApplicationBuilder builder)
        {
            builder.Services.AddHttpClient<MailHelper>();
            builder.Services.AddHttpClient<UploadHelper>();
        }

        public static void AddBuilderServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddTransient<IAuthService, AuthService>();

            builder.Services.AddTransient<IUserService, UserService>();
            builder.Services.AddTransient<IUserRepository, UserRepository>();

            builder.Services.AddTransient<IStoreService, StoreService>();
            builder.Services.AddTransient<IStoreRepository, StoreRepository>();

            builder.Services.AddTransient<IIngredientService, IngredientService>();
            builder.Services.AddTransient<IIngredientRepository, IngredientRepository>();

            builder.Services.AddTransient<IRecipeService, RecipeService>();
            builder.Services.AddTransient<IRecipeRepository, RecipeRepository>();

            builder.Services.AddTransient<ISimulatorService, SimulatorService>();
            builder.Services.AddTransient<IDashboardService, DashboardService>();
            builder.Services.AddTransient<IPublicMenuService, PublicMenuService>();

            builder.Services.AddTransient<IStockEntryService, StockEntryService>();
            builder.Services.AddTransient<IStockEntryRepository, StockEntryRepository>();

            builder.Services.AddTransient<IGenericTableService, GenericTableService>();
            builder.Services.AddTransient<IGenericTableRepository, GenericTableRepository>();
        }
    }
}
