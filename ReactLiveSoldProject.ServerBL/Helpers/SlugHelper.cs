using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ReactLiveSoldProject.ServerBL.Base;

namespace ReactLiveSoldProject.ServerBL.Helpers
{
    public static class SlugHelper
    {
        /// <summary>
        /// Genera un slug limpio desde un texto
        /// Ejemplo: "Tienda de Juan" -> "tienda-de-juan"
        /// </summary>
        public static string GenerateSlug(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("El texto no puede estar vacío", nameof(text));

            // Convertir a minúsculas
            var slug = text.ToLowerInvariant();

            // Normalizar y remover acentos
            slug = RemoveDiacritics(slug);

            // Reemplazar espacios y underscores con guiones
            slug = slug.Replace(" ", "-").Replace("_", "-");

            // Remover caracteres no permitidos (solo letras, números y guiones)
            var sb = new StringBuilder();
            foreach (var c in slug)
            {
                if (char.IsLetterOrDigit(c) || c == '-')
                {
                    sb.Append(c);
                }
            }

            slug = sb.ToString();

            // Remover guiones múltiples
            while (slug.Contains("--"))
            {
                slug = slug.Replace("--", "-");
            }

            // Remover guiones al inicio y final
            slug = slug.Trim('-');

            if (string.IsNullOrWhiteSpace(slug))
                throw new ArgumentException("El texto no generó un slug válido", nameof(text));

            return slug;
        }

        /// <summary>
        /// Asegura que el slug sea único en la base de datos
        /// Si ya existe, agrega un sufijo numérico
        /// </summary>
        public static async Task<string> EnsureUniqueSlugAsync(
            LiveSoldDbContext dbContext,
            string baseSlug,
            Guid? excludeOrganizationId = null)
        {
            var slug = baseSlug;
            var counter = 1;

            while (true)
            {
                var exists = excludeOrganizationId.HasValue
                    ? await dbContext.Organizations
                        .AnyAsync(o => o.Slug == slug && o.Id != excludeOrganizationId.Value)
                    : await dbContext.Organizations
                        .AnyAsync(o => o.Slug == slug);

                if (!exists)
                    break;

                slug = $"{baseSlug}-{counter}";
                counter++;
            }

            return slug;
        }

        /// <summary>
        /// Remueve acentos y diacríticos de un texto
        /// </summary>
        private static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
