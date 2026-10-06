using api_custo_prato.src.Requests;

namespace api_custo_prato.src.Shared.DTOs
{
    public class DeleteDTO : Request
    {
        public string Id {get;set;} = string.Empty;
    }
}