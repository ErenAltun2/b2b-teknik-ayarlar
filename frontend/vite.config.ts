import vue from '@vitejs/plugin-vue'
import { defineConfig } from 'vite'

// In development the API runs on :5080 and requests to /api are proxied to it.
// `npm run build` writes the app into the API's wwwroot so both ship as one service.
export default defineConfig({
  plugins: [vue()],
  server: {
    port: 5173,
    proxy: {
      '/api': 'http://localhost:5080',
    },
  },
  build: {
    outDir: '../backend/B2BStore.Settings.Api/wwwroot',
    emptyOutDir: true,
  },
})
