// tailwind.config.ts

import { defineConfig } from 'tailwindcss'
import animatePlugin from 'tailwindcss-animate' // 1. Importa el plugin

export default defineConfig({
  content: [
    './src/**/*.{js,ts,jsx,tsx}',
    // ...
  ],
  theme: {
    extend: {
      // ...
    },
  },
  plugins: [
    animatePlugin, // 2. Añádelo al array de plugins
  ],
})