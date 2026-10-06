using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using api_custo_prato.src.Helpers;
using api_custo_prato.src.Interfaces;
using api_custo_prato.src.Models;
using api_custo_prato.src.Models.Base;
using api_custo_prato.src.Requests.Stores;

namespace api_custo_prato.src.Services
{
    public partial class StoreService(IStoreRepository repository, UploadHelper uploadHelper) : IStoreService
    {
        private static readonly string[] AllowedImageTypes = ["image/jpeg", "image/png", "image/webp"];
        private const long MaxImageBytes = 5 * 1024 * 1024;

        [GeneratedRegex("[^a-z0-9-]")]
        private static partial Regex InvalidSlugChars();

        [GeneratedRegex("-{2,}")]
        private static partial Regex MultipleDashes();

        /// <summary>"Hamburgueria do João!" → "hamburgueria-do-joao"</summary>
        public static string Slugify(string text)
        {
            string normalized = (text ?? "").Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
            StringBuilder sb = new();
            foreach (char c in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) sb.Append(c);
            }
            string slug = sb.ToString().Normalize(NormalizationForm.FormC).Replace(' ', '-');
            slug = InvalidSlugChars().Replace(slug, "");
            slug = MultipleDashes().Replace(slug, "-").Trim('-');
            return slug.Length > 60 ? slug[..60].Trim('-') : slug;
        }

        private async Task<string> UniqueSlugAsync(string baseText, string userId)
        {
            string baseSlug = Slugify(baseText);
            if (string.IsNullOrEmpty(baseSlug)) baseSlug = "cardapio";
            string slug = baseSlug;
            int i = 2;
            while (await repository.SlugExistsAsync(slug, userId))
            {
                slug = $"{baseSlug}-{i++}";
            }
            return slug;
        }

        public async Task<Store> EnsureStoreAsync(string userId, string name = "")
        {
            Store? store = await repository.GetByUserIdAsync(userId);
            if (store is not null) return store;

            string storeName = string.IsNullOrWhiteSpace(name) ? "Meu Restaurante" : name.Trim();
            store = new Store
            {
                UserId = userId,
                Name = storeName,
                Slug = await UniqueSlugAsync(storeName, userId),
                CreatedBy = userId,
                DefaultFees =
                [
                    new RecipeFee { Name = "Cartão", Type = "percent", Value = 0 }
                ]
            };

            return await repository.CreateAsync(store);
        }

        public async Task<ResponseApi<Store?>> GetAsync(string userId)
        {
            try
            {
                Store store = await EnsureStoreAsync(userId);
                return new(store, 200, "Dados do estabelecimento");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }

        public async Task<ResponseApi<Store?>> UpdateAsync(UpdateStoreRequest request, string userId)
        {
            try
            {
                Store store = await EnsureStoreAsync(userId);

                string slug = Slugify(request.Slug);
                if (slug.Length < 3) return new(null, 400, "O link do cardápio deve ter pelo menos 3 caracteres (letras, números ou hífen).");
                if (await repository.SlugExistsAsync(slug, userId)) return new(null, 400, "Este link de cardápio já está em uso. Escolha outro.");

                string whatsapp = new string((request.Whatsapp ?? "").Where(char.IsDigit).ToArray());
                if (whatsapp.Length is > 0 and < 10) return new(null, 400, "WhatsApp inválido. Informe DDD + número.");
                if (whatsapp.Length is 10 or 11) whatsapp = "55" + whatsapp;

                store.Name = request.Name.Trim();
                store.Slug = slug;
                store.Whatsapp = whatsapp;
                store.Logo = request.Logo?.Trim() ?? store.Logo;
                store.Description = request.Description?.Trim() ?? "";
                store.DeliveryFee = request.DeliveryFee;
                store.PaymentMethods = request.PaymentMethods.Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => p.Trim()).Distinct().ToList();
                store.TargetMargin = request.TargetMargin;
                store.DefaultFees = request.DefaultFees.Where(f => !string.IsNullOrWhiteSpace(f.Name)).ToList();
                store.MenuEnabled = request.MenuEnabled;
                store.UpdatedAt = DateTime.UtcNow;
                store.UpdatedBy = userId;

                await repository.UpdateAsync(store);
                return new(store, 200, "Dados do estabelecimento atualizados com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }

        public async Task<ResponseApi<Store?>> UploadLogoAsync(IFormFile file, string userId)
        {
            try
            {
                string? error = ValidateImage(file);
                if (error is not null) return new(null, 400, error);

                Store store = await EnsureStoreAsync(userId);
                string url = await uploadHelper.SaveFileAsync(file, $"stores/{userId}");
                if (string.IsNullOrEmpty(url)) return new(null, 400, "Falha ao enviar imagem. Verifique a configuração do armazenamento.");

                store.Logo = url;
                store.UpdatedAt = DateTime.UtcNow;
                await repository.UpdateAsync(store);
                return new(store, 200, "Logo atualizada com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado. Por favor, tente novamente mais tarde. {ex.Message}");
            }
        }

        public static string? ValidateImage(IFormFile? file)
        {
            if (file is null || file.Length == 0) return "Selecione uma imagem.";
            if (file.Length > MaxImageBytes) return "A imagem deve ter no máximo 5 MB.";
            if (!AllowedImageTypes.Contains(file.ContentType?.ToLowerInvariant())) return "Formato inválido. Use JPG, PNG ou WEBP.";
            return null;
        }
    }
}
