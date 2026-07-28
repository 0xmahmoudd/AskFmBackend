import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'http://localhost:5180',
        changeOrigin: true,
        secure: false,
      },
      '/notificationHub': {
        target: 'http://localhost:5180',
        ws: true,
        changeOrigin: true,
        secure: false,
      },
      '/avatars': {
        target: 'http://localhost:5180',
        changeOrigin: true,
        secure: false,
      }
    }
  }
})

