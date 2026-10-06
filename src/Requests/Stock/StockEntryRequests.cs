using System.ComponentModel.DataAnnotations;

namespace api_custo_prato.src.Requests.Stock
{
    public class CreateStockEntryRequest : Request
    {
        [Required(ErrorMessage = "O ingrediente é obrigatório.")]
        public string IngredientId { get; set; } = string.Empty;

        [Range(0.0001, 999999999, ErrorMessage = "A quantidade comprada deve ser maior que zero.")]
        public decimal Quantity { get; set; } = 1;

        [Required(ErrorMessage = "A unidade é obrigatória.")]
        public string Unit { get; set; } = "kg";

        [Range(0, 999999999, ErrorMessage = "O valor total pago deve ser maior ou igual a zero.")]
        public decimal TotalPrice { get; set; } = 0;

        public string Supplier { get; set; } = string.Empty;

        public DateTime? PurchaseDate { get; set; }

        public string Notes { get; set; } = string.Empty;

        /// <summary>Se true, atualiza o preço de custo padrão do ingrediente no cadastro para recalcular as fichas técnicas.</summary>
        public bool UpdateIngredientCost { get; set; } = true;
    }
}
