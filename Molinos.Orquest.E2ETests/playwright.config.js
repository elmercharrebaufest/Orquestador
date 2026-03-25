// @ts-check
const { defineConfig, devices } = require('@playwright/test');

/**
 * Configuración de Playwright para tests E2E de Molinos.Orquest.Web
 * @see https://playwright.dev/docs/test-configuration
 */
module.exports = defineConfig({
  testDir: './tests',
  
  /* Tiempo máximo de ejecución por test - aumentado para Windows Auth */
  timeout: 60 * 1000,
  
  /* Configuración de expects */
  expect: {
    timeout: 5000
  },
  
  /* Ejecutar tests en paralelo */
  fullyParallel: false,
  
  /* Fallar el build en CI si dejaste test.only */
  forbidOnly: !!process.env.CI,
  
  /* Reintentar en CI */
  retries: process.env.CI ? 2 : 0,
  
  /* Número de workers */
  workers: process.env.CI ? 1 : undefined,
  
  /* Reporter de tests */
  reporter: [
    ['html'],
    ['list']
  ],
  
  /* Configuración compartida para todos los proyectos */
  use: {
    /* URL base */
    baseURL: process.env.BASE_URL || 'http://localhost',
    
    /* Autenticación de Windows (HTTP Basic Auth / NTLM) */
    httpCredentials: process.env.WINDOWS_USER && process.env.WINDOWS_PASSWORD ? {
      username: process.env.WINDOWS_USER,
      password: process.env.WINDOWS_PASSWORD,
    } : undefined,
    
    /* Captura de trazas en caso de fallo */
    trace: 'on-first-retry',
    
    /* Captura de screenshot en fallo */
    screenshot: 'only-on-failure',
    
    /* Captura de video en fallo */
    video: 'retain-on-failure',
    
    /* Timeouts para acciones */
    actionTimeout: 10000,
    navigationTimeout: 60000,
  },

  /* Configuración de proyectos para diferentes browsers */
  /* NOTA: Solo Chromium soporta Windows Authentication con httpCredentials */
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },

    /* Firefox y Webkit no soportan Windows Authentication correctamente
    {
      name: 'firefox',
      use: { ...devices['Desktop Firefox'] },
    },

    {
      name: 'webkit',
      use: { ...devices['Desktop Safari'] },
    },
    */

    /* Tests móviles */
    // {
    //   name: 'Mobile Chrome',
    //   use: { ...devices['Pixel 5'] },
    // },
    // {
    //   name: 'Mobile Safari',
    //   use: { ...devices['iPhone 12'] },
    // },
  ],

  /* Configuración del servidor de desarrollo (opcional) */
  /* Descomenta esto si quieres que Playwright levante automáticamente el servidor
  webServer: {
    command: 'dotnet run --project ../Molinos.Orquest.Web/Molinos.Orquest.Web.csproj',
    url: 'http://localhost:5000',
    reuseExistingServer: !process.env.CI,
    timeout: 120 * 1000,
  },
  */
});
