using api_custo_prato.src.Interfaces;
using api_custo_prato.src.Models;
using api_custo_prato.src.Models.Base;

namespace api_custo_prato.src.Services
{
    /// <summary>Cardápio público (/c/{slug}). Expõe SOMENTE dados de venda — nunca custo, lucro ou margem.</summary>
    public class PublicMenuService(IStoreRepository storeRepository, IRecipeRepository recipeRepository) : IPublicMenuService
    {
        public async Task<ResponseApi<dynamic?>> GetMenuAsync(string slug)
        {
            try
            {
                Store? store = await storeRepository.GetBySlugAsync(slug);
                if (store is null || !store.MenuEnabled) return new(null, 404, "Cardápio não encontrado");

                List<Recipe> recipes = await recipeRepository.GetAllByUserAsync(store.UserId);

                var items = recipes
                    .Where(r => r.ShowInMenu && r.Active && r.SalePrice > 0)
                    .Select(r => new
                    {
                        id = r.Id,
                        name = r.Name,
                        description = r.Description,
                        photo = r.Photo,
                        category = string.IsNullOrWhiteSpace(r.Category) ? "Outros" : r.Category,
                        price = r.SalePrice
                    })
                    .ToList();

                var categories = items
                    .GroupBy(i => i.category)
                    .OrderBy(g => g.Key == "Outros" ? 1 : 0)
                    .ThenBy(g => g.Key)
                    .Select(g => new { name = g.Key, items = g.OrderBy(i => i.name).ToList() })
                    .ToList();

                return new(new
                {
                    store = new
                    {
                        name = store.Name,
                        slug = store.Slug,
                        description = store.Description,
                        logo = store.Logo,
                        whatsapp = store.Whatsapp,
                        deliveryFee = store.DeliveryFee,
                        paymentMethods = store.PaymentMethods
                    },
                    categories
                }, 200, "Cardápio carregado com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }
    }
}
