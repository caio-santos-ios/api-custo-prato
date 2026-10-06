using System.ComponentModel.DataAnnotations;

namespace api_custo_prato.src.Requests.Ingredients
{
    public class CreateIngredientRequest : Request
    {
        [Required(ErrorMessage = "O Nome do ingrediente é obrigatório.")]
        public string Name { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        [Range(0, 999999999, ErrorMessage = "A quantidade comprada deve ser maior ou igual a zero.")]
        public decimal PurchaseQuantity { get; set; } = 0;

        [Required(ErrorMessage = "A unidade de compra é obrigatória.")]
        public string PurchaseUnit { get; set; } = "kg";

        [Range(0, 999999999, ErrorMessage = "O preço pago deve ser maior ou igual a zero.")]
        public decimal PurchasePrice { get; set; } = 0;

        [Range(0.01, 100, ErrorMessage = "O aproveitamento deve ser entre 0,01% e 100%.")]
        public decimal YieldPercent { get; set; } = 100;

        public string Notes { get; set; } = string.Empty;

        public string UserId { get; set; } = string.Empty;
    }

    public class UpdateIngredientRequest : CreateIngredientRequest
    {
        [Required(ErrorMessage = "O Id é obrigatório.")]
        public string Id { get; set; } = string.Empty;
    }
}
