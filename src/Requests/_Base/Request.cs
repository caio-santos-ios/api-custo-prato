namespace api_custo_prato.src.Requests
{
    public class Request
    {
        public string UserId { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
        public string DeletedBy { get; set; } = string.Empty;
    }
}