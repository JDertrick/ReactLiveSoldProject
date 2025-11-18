/**
 * Construye la URL para una imagen almacenada en el servidor
 * @param relativeUrl - URL relativa de la imagen (ej: /Uploads/organizations/...)
 * @returns URL relativa que será manejada por el proxy de Vite en desarrollo
 */
export function getImageUrl(relativeUrl: string | null | undefined): string | null {
  if (!relativeUrl) return null;

  // Si ya es una URL completa, retornarla tal cual
  if (relativeUrl.startsWith('http://') || relativeUrl.startsWith('https://')) {
    return relativeUrl;
  }

  // Asegurarse de que la URL relativa empiece con /
  const cleanUrl = relativeUrl.startsWith('/') ? relativeUrl : '/' + relativeUrl;

  // En desarrollo, Vite proxy redirigirá /Uploads a http://localhost:5165/Uploads
  // En producción, se servirá desde el mismo dominio
  console.log('🖼️ Image URL Helper:', {
    relativeUrl,
    cleanUrl,
    note: 'Using relative URL - Vite proxy will handle redirection to backend'
  });

  // Retornar la URL relativa
  return cleanUrl;
}
