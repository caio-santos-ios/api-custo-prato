namespace api_custo_prato.src.Shared.DTOs
{
    public class GetAllDTO
    {
        public GetAllDTO(IQueryCollection queries)
        {
            foreach (var query in queries)
            { 
                QueryParams.Add(query.Key, query.Value!);
            }
        }

        public GetAllDTO(Dictionary<string, string> queries)
        {
            QueryParams = queries;
        }

        public Dictionary<string, string> QueryParams { get; set; } = [];

    }
}