using System.ComponentModel.DataAnnotations;
using api_custo_prato.src.Models;

namespace api_custo_prato.src.Requests.Recipes
{
    public class CreateRecipeRequest : Request
    {
        [Required(ErrorMessage = "O Nome do prato é obrigatório.")]
        public string Name { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Photo { get; set; } = string.Empty;

        public List<RecipeItem> Items { get; set; } = [];

        [Range(0, 999999999, ErrorMessage = "O rendimento deve ser maior ou igual a zero.")]
        public decimal YieldQuantity { get; set; } = 0;

        public string YieldUnit { get; set; } = "kg";

        [Range(0.01, 999999, ErrorMessage = "O número de porções deve ser maior que zero.")]
        public decimal Portions { get; set; } = 1;

        [Range(0, 999999999, ErrorMessage = "O custo da embalagem deve ser maior ou igual a zero.")]
        public decimal PackagingCost { get; set; } = 0;

        public List<RecipeOtherCost> OtherCosts { get; set; } = [];

        [Range(0, 999999999, ErrorMessage = "O preço de venda deve ser maior ou igual a zero.")]
        public decimal SalePrice { get; set; } = 0;

        public List<RecipeFee> Fees { get; set; } = [];

        public bool ShowInMenu { get; set; } = true;

        public string UserId { get; set; } = string.Empty;
    }

    public class UpdateRecipeRequest : CreateRecipeRequest
    {
        [Required(ErrorMessage = "O Id é obrigatório.")]
        public string Id { get; set; } = string.Empty;
    }
}
