import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

// Сборка кладётся прямо в wwwroot — ASP.NET раздаёт SPA как статику (см. Program.cs).
export default defineConfig({
  plugins: [react()],
  build: {
    outDir: '../backend/PeopleHub.Web/wwwroot',
    emptyOutDir: true,
  },
  server: {
    port: 5173,
    proxy: {
      '/api': 'http://localhost:5084',
      '/swagger': 'http://localhost:5084',
    },
  },
});
