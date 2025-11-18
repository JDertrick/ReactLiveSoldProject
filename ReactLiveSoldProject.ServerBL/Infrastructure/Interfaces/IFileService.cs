using Microsoft.AspNetCore.Http;

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces
{
    public interface IFileService
    {
        /// <summary>
        /// Guarda una imagen de producto y retorna la URL relativa
        /// </summary>
        /// <param name="file">Archivo a guardar</param>
        /// <param name="organizationId">ID de la organización</param>
        /// <param name="productId">ID del producto (opcional, se genera uno nuevo si no se proporciona)</param>
        /// <returns>URL relativa del archivo guardado</returns>
        Task<string> SaveProductImageAsync(IFormFile file, Guid organizationId, Guid? productId = null);

        /// <summary>
        /// Guarda el logo de una organización y retorna la URL relativa
        /// </summary>
        /// <param name="file">Archivo a guardar</param>
        /// <param name="organizationId">ID de la organización</param>
        /// <returns>URL relativa del archivo guardado</returns>
        Task<string> SaveOrganizationLogoAsync(IFormFile file, Guid organizationId);

        /// <summary>
        /// Elimina un archivo físicamente del servidor
        /// </summary>
        /// <param name="relativeUrl">URL relativa del archivo a eliminar</param>
        /// <returns>True si se eliminó correctamente</returns>
        Task<bool> DeleteFileAsync(string relativeUrl);

        /// <summary>
        /// Valida que el archivo sea una imagen válida
        /// </summary>
        /// <param name="file">Archivo a validar</param>
        /// <returns>True si es una imagen válida</returns>
        bool IsValidImage(IFormFile file);
    }
}
