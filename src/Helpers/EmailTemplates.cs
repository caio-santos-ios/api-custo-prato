using System.Net;

namespace api_custo_prato.src.Helpers
{
    public static class EmailTemplates
    {
        private const string PrimaryColor = "#ea580c";
        private const string DarkColor = "#1c1917";

        private static string GetUiUrl()
        {
            string url = Environment.GetEnvironmentVariable("UI_URL") ?? "";
            if (string.IsNullOrWhiteSpace(url)) url = "http://localhost:4310";
            return url.TrimEnd('/');
        }

        public static string GetAccountConfirmationTemplate(string? name, string code)
        {
            string link = $"{GetUiUrl()}/confirmation/{code}";
            return BuildTemplate(
                title: "Confirma&#231;&#227;o de Cadastro",
                greeting: Greeting(name),
                intro: "Seja bem-vindo(a) ao CustoPrato! Para ativar sua conta, clique no bot&#227;o abaixo ou utilize o c&#243;digo de verifica&#231;&#227;o:",
                code: code,
                link: link,
                buttonText: "Confirmar Minha Conta",
                notice: "Se voc&#234; n&#227;o realizou este cadastro, desconsidere este e-mail."
            );
        }

        public static string GetPasswordResetTemplate(string? name, string code)
        {
            string link = $"{GetUiUrl()}/reset-password/{code}";
            return BuildTemplate(
                title: "Redefini&#231;&#227;o de Senha",
                greeting: Greeting(name),
                intro: "Recebemos uma solicita&#231;&#227;o para redefinir a senha da sua conta. Clique no bot&#227;o abaixo ou utilize o c&#243;digo:",
                code: code,
                link: link,
                buttonText: "Redefinir Minha Senha",
                notice: "Se voc&#234; n&#227;o solicitou a redefini&#231;&#227;o, desconsidere esta mensagem. Sua senha atual continua v&#225;lida."
            );
        }

        private static string Greeting(string? name)
            => !string.IsNullOrWhiteSpace(name) ? $"Ol&#225;, <strong>{WebUtility.HtmlEncode(name)}</strong>!" : "Ol&#225;!";

        private static string BuildTemplate(string title, string greeting, string intro, string code, string link, string buttonText, string notice)
        {
            int year = DateTime.UtcNow.Year;
            return $@"<!doctype html>
<html lang=""pt-br"">
<head><meta charset=""utf-8""><meta name=""viewport"" content=""width=device-width, initial-scale=1""><title>{title}</title></head>
<body style=""margin:0;padding:0;background:#f5f5f4;font-family:Arial,Helvetica,sans-serif;color:{DarkColor};"">
  <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""background:#f5f5f4;padding:32px 12px;"">
    <tr><td align=""center"">
      <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""max-width:560px;background:#ffffff;border-radius:16px;overflow:hidden;"">
        <tr><td style=""background:{DarkColor};padding:24px;text-align:center;"">
          <span style=""font-size:22px;font-weight:800;color:#ffffff;"">Custo<span style=""color:{PrimaryColor};"">Prato</span></span>
        </td></tr>
        <tr><td style=""padding:32px 28px;"">
          <h1 style=""margin:0 0 16px;font-size:20px;"">{title}</h1>
          <p style=""margin:0 0 12px;font-size:15px;"">{greeting}</p>
          <p style=""margin:0 0 24px;font-size:14px;line-height:1.6;color:#57534e;"">{intro}</p>
          <div style=""text-align:center;margin:0 0 24px;"">
            <span style=""display:inline-block;font-size:30px;letter-spacing:8px;font-weight:800;background:#fff7ed;border:1px dashed {PrimaryColor};border-radius:12px;padding:14px 22px;color:{PrimaryColor};"">{code}</span>
          </div>
          <div style=""text-align:center;margin:0 0 24px;"">
            <a href=""{link}"" style=""display:inline-block;background:{PrimaryColor};color:#ffffff;text-decoration:none;font-weight:700;padding:14px 28px;border-radius:10px;"">{buttonText}</a>
          </div>
          <p style=""margin:0;font-size:12px;color:#a8a29e;line-height:1.6;"">O c&#243;digo expira em 5 minutos. {notice}</p>
        </td></tr>
        <tr><td style=""padding:16px;text-align:center;font-size:11px;color:#a8a29e;background:#fafaf9;"">&#169; {year} CustoPrato</td></tr>
      </table>
    </td></tr>
  </table>
</body>
</html>";
        }
    }
}
