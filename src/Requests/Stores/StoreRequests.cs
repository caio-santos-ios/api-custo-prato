using System.ComponentModel.DataAnnotations;
using api_custo_prato.src.Models;

namespace api_custo_prato.src.Requests.Stores
{
    public class UpdateStoreRequest : Request
    {
        [Required(ErrorMessage = "O nome do estabelecimento é obrigatório.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "O link do cardápio é obrigatório.")]
        public string Slug { get; set; } = string.Empty;

        public string Whatsapp { get; set; } = string.Empty;

        public string Logo { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Range(0, 999999, ErrorMessage = "A taxa de entrega deve ser maior ou igual a zero.")]
        public decimal DeliveryFee { get; set; } = 0;

        public List<string> PaymentMethods { get; set; } = [];

        [Range(0, 99, ErrorMessage = "A margem desejada deve ser entre 0% e 99%.")]
        public decimal TargetMargin { get; set; } = 30;

        public List<RecipeFee> DefaultFees { get; set; } = [];

        public bool MenuEnabled { get; set; } = true;
    }
}
