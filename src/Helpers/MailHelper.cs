using System.Net.Http.Json;

namespace api_custo_prato.src.Helpers
{
    public class MailHelper(HttpClient http)
    {
        private readonly string _apiKey = Environment.GetEnvironmentVariable("RESEND_API_KEY") ?? "";
        private readonly string _fromEmail = Environment.GetEnvironmentVariable("RESEND_EMAIL") ?? "";

        public async Task<string> SendMail(string recipient, string subject, string body)
        {
            try
            {
                var payload = new
                {
                    from = _fromEmail,
                    to = new[] { recipient },
                    subject,
                    html = body
                };

                var req = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails")
                {
                    Content = JsonContent.Create(payload)
                };
                req.Headers.Authorization = new("Bearer", _apiKey);

                var res = await http.SendAsync(req);
                if (!res.IsSuccessStatusCode)
                {
                    string error = await res.Content.ReadAsStringAsync();
                    Console.WriteLine($"[MailHelper] Falha ao enviar e-mail para {recipient}: {error}");
                    return error;
                }

                return "";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MailHelper] {ex.Message}");
                return ex.Message;
            }
        }

        public async Task<string> SendAccountConfirmationMail(string recipient, string name, string code)
        {
            // Facilita o desenvolvimento local quando o Resend não está configurado.
            Console.WriteLine($"[MailHelper] Código de confirmação para {recipient}: {code}");
            string body = EmailTemplates.GetAccountConfirmationTemplate(name, code);
            return await SendMail(recipient, "Confirmação de Cadastro — CustoPrato", body);
        }

        public async Task<string> SendPasswordResetMail(string recipient, string name, string code)
        {
            Console.WriteLine($"[MailHelper] Código de redefinição para {recipient}: {code}");
            string body = EmailTemplates.GetPasswordResetTemplate(name, code);
            return await SendMail(recipient, "Redefinição de Senha — CustoPrato", body);
        }
    }
}
