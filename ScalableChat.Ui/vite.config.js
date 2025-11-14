/* eslint-disable no-undef */
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  server: {
    allowedHosts: [
      'host.docker.internal'
    ],
    port: parseInt(process.env.PORT) || 3000,
    host: true
  },
})
