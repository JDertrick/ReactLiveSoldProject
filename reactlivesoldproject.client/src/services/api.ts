import axios from 'axios';

// Crear instancia de Axios
// En desarrollo, Vite proxy redirigirá /api a http://localhost:5165/api
// En producción, se usará el mismo dominio
export const apiClient = axios.create({
  baseURL: '/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

// Interceptor para agregar el token JWT a todas las peticiones
apiClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Interceptor para manejar errores de respuesta
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      const requestUrl = error.config?.url || '';

      // NO redirigir si es un error de login (credenciales incorrectas)
      const isLoginEndpoint =
        requestUrl.includes('/auth/employee-login') ||
        requestUrl.includes('/auth/portal/login');

      if (!isLoginEndpoint) {
        // Token inválido o expirado en endpoint protegido
        localStorage.removeItem('token');
        localStorage.removeItem('user');

        // Redirigir según el contexto
        const isPortalRoute = window.location.pathname.includes('/portal/');
        if (isPortalRoute) {
          // Extraer orgSlug de la URL actual
          const match = window.location.pathname.match(/\/portal\/([^\/]+)/);
          const orgSlug = match ? match[1] : 'login';
          window.location.href = `/portal/${orgSlug}/login`;
        } else {
          window.location.href = '/login';
        }
      }
    }
    return Promise.reject(error);
  }
);

export default apiClient;
