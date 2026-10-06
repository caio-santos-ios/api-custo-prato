using System.Net.Http.Headers;
namespace api_custo_prato.src.Helpers
{
    public class UploadHelper(HttpClient Http)
    {
        private readonly string ApiUri = Environment.GetEnvironmentVariable("API_URI") ?? "";
        private readonly string SupabaseUrl = Environment.GetEnvironmentVariable("SUPABASE_URL")?.TrimEnd('/') ?? "";
        private readonly string SupabaseKey = Environment.GetEnvironmentVariable("SUPABASE_SERVICE_KEY") ?? "";
        private readonly string Bucket = Environment.GetEnvironmentVariable("SUPABASE_BUCKET") ?? "uploads";

        public async Task<string> SaveFileAsync(IFormFile file, string folder)
        {
            try
            {
                if (file == null || file.Length == 0) return "";

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var objectPath = $"{folder.Trim('/')}/{Guid.NewGuid():N}{extension}";

                using var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    $"{SupabaseUrl}/storage/v1/object/{Bucket}/{objectPath}");

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", SupabaseKey);
                request.Headers.Add("x-upsert", "false");

                using var stream = file.OpenReadStream();
                request.Content = new StreamContent(stream);
                request.Content.Headers.ContentType = new MediaTypeHeaderValue(
                    string.IsNullOrWhiteSpace(file.ContentType)
                        ? "application/octet-stream"
                        : file.ContentType);

                using var response = await Http.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Supabase {(int)response.StatusCode}: {body}");
                    return "";
                }

                return $"{SupabaseUrl}/storage/v1/object/public/{Bucket}/{objectPath}";
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
                return "";
            }
        }

        public async Task<string> SaveFileV1Async(IFormFile file)
        {
            try
            {
                string uploadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

                if (!Directory.Exists(uploadDirectory)) Directory.CreateDirectory(uploadDirectory);
                string filePath = Path.Combine(uploadDirectory, file.FileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return $"{ApiUri}/uploads/{file.FileName}";
            }
            catch
            {
                return "";
            }
        }
    }
}
