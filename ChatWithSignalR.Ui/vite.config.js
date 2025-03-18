/* eslint-disable no-undef */
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  server: {
    port: parseInt(process.env.PORT) || 3000,
    host: true
  },
  define: {
    'import.meta.env.VITE_CHAT_API': JSON.stringify(process.env['services__chat-api__https__0'])
  }
})
