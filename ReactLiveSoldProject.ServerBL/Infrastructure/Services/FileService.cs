using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces;

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private readonly long _maxFileSize = 5 * 1024 * 1024; // 5 MB

        public FileService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> SaveProductImageAsync(IFormFile file, Guid organizationId, Guid? productId = null)
        {
            if (!IsValidImage(file))
            {
                throw new InvalidOperationException("El archivo no es una imagen válida o excede el tamaño máximo permitido (5 MB).");
            }

            var id = productId ?? Guid.NewGuid();
            var directory = Path.Combine("Uploads", "organizations", organizationId.ToString(), "products", id.ToString());
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName).ToLowerInvariant()}";

            return await SaveFileAsync(file, directory, fileName);
        }

        public async Task<string> SaveOrganizationLogoAsync(IFormFile file, Guid organizationId)
        {
            if (!IsValidImage(file))
            {
                throw new InvalidOperationException("El archivo no es una imagen válida o excede el tamaño máximo permitido (5 MB).");
            }

            var directory = Path.Combine("Uploads", "organizations", organizationId.ToString(), "logo");
            var fileName = $"logo_{Guid.NewGuid()}{Path.GetExtension(file.FileName).ToLowerInvariant()}";

            return await SaveFileAsync(file, directory, fileName);
        }

        public async Task<bool> DeleteFileAsync(string relativeUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(relativeUrl))
                    return false;

                // Remover el "/" inicial si existe
                var cleanUrl = relativeUrl.TrimStart('/');

                var fullPath = Path.Combine(_environment.WebRootPath, cleanUrl);

                if (File.Exists(fullPath))
                {
                    await Task.Run(() => File.Delete(fullPath));
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool IsValidImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            if (file.Length > _maxFileSize)
                return false;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
                return false;

            // Validar tipo MIME
            var allowedMimeTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
            if (!allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
                return false;

            return true;
        }

        private async Task<string> SaveFileAsync(IFormFile file, string directory, string fileName)
        {
            try
            {
                // Crear la ruta completa en el servidor (dentro de wwwroot)
                var fullDirectory = Path.Combine(_environment.WebRootPath, directory);

                // Crear el directorio si no existe
                if (!Directory.Exists(fullDirectory))
                {
                    Directory.CreateDirectory(fullDirectory);
                }

                // Ruta completa del archivo
                var fullPath = Path.Combine(fullDirectory, fileName);

                // Guardar el archivo
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Retornar la URL relativa con formato web (con /)
                var relativeUrl = "/" + Path.Combine(directory, fileName).Replace("\\", "/");
                return relativeUrl;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error al guardar el archivo: {ex.Message}", ex);
            }
        }
    }
}
