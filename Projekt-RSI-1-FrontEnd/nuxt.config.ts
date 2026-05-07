import fs from 'fs'
import path from 'path'

// Ścieżka wewnątrz kontenera (teraz bezpośrednio w /certs)
const dockerKeyPath = '/certs/localhost.key'
const dockerCertPath = '/certs/localhost.crt'

// Ścieżka lokalna (gdy uruchamiasz "npm run dev" bezpośrednio na Windowsie)
const localKeyPath = path.resolve(__dirname, './certs/localhost.key')
const localCertPath = path.resolve(__dirname, './certs/localhost.crt')

const keyPath = fs.existsSync(dockerKeyPath) ? dockerKeyPath : (fs.existsSync(localKeyPath) ? localKeyPath : null)
const certPath = fs.existsSync(dockerCertPath) ? dockerCertPath : (fs.existsSync(localCertPath) ? localCertPath : null)

const hasCertificates = keyPath && certPath
export default defineNuxtConfig({
  modules: [
    '@nuxt/eslint',
    '@nuxt/ui'
  ],
  
  devServer: {
    https: {
      key: keyPath,
      cert: certPath
    }
  },

  devtools: {
    enabled: true
  },

  css: ['~/assets/css/main.css'],

  routeRules: {
    '/': { prerender: true }
  },

  compatibilityDate: '2025-01-15',

  eslint: {
    config: {
      stylistic: {
        commaDangle: 'never',
        braceStyle: '1tbs'
      }
    }
  }
})
