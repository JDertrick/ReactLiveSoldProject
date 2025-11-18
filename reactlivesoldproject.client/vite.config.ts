import path from "path"
import { defineConfig } from 'vite';
import plugin from '@vitejs/plugin-react';

// https://vitejs.dev/config/
export default defineConfig({
    plugins: [plugin()],
    resolve: {
        alias: {
            "@": path.resolve(__dirname, "./src"),
        }
    },
    server: {
        port: 5173,
        host: true,
        proxy: {
            // Proxy para imágenes - redirige /Uploads al backend
            '/Uploads': {
                target: 'http://localhost:5165',
                changeOrigin: true,
                secure: false,
            },
            // Proxy para API - redirige /api al backend
            '/api': {
                target: 'http://localhost:5165',
                changeOrigin: true,
                secure: false,
            }
        }
    }
})
