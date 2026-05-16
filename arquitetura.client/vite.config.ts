import { fileURLToPath, URL } from 'node:url';
import { defineConfig } from 'vite';
import plugin from '@vitejs/plugin-react';
import tailwindcss from '@tailwindcss/vite';

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [plugin(), tailwindcss()],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },
  server: {
    port: 5173,
    proxy: {
      '/api/produtos': {
        target: 'http://localhost:5001',
        changeOrigin: true,
      },
      '/api/pedidos': {
        target: 'http://localhost:5002',
        changeOrigin: true,
      },
    },
  },
});

