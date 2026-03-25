require('dotenv').config();
const { test, expect } = require('@playwright/test');

const BASE_URL = process.env.BASE_URL;
const TEST_ITC_ID = process.env.TEST_ITC_ID;

test.describe('PruebaItc - Visualización de Contenido', () => {
  
  test.beforeEach(async ({ page }) => {
    await page.goto(`${BASE_URL}/PruebaItc/Index/${TEST_ITC_ID}`);
    await page.waitForLoadState('domcontentloaded');
    await page.waitForTimeout(1500);
  });

  test.describe('Header', () => {
    
    test('Debe mostrar título "Prueba Concentrador: PLC"', async ({ page }) => {
      const contenido = await page.textContent('body');
      expect(contenido).toContain('Prueba Concentrador');
      expect(contenido).toContain('PLC');
    });

    test('Debe mostrar estado del concentrador "Desconectado" o "Conectado"', async ({ page }) => {
      const contenido = await page.textContent('body');
      const tieneEstado = contenido.includes('Desconectado') || 
                          contenido.includes('Conectado') || 
                          contenido.includes('Consultando');
      expect(tieneEstado).toBeTruthy();
    });
  });

  test.describe('Sección: Lecturas de Tarjetas', () => {
    
    test('Debe mostrar sección "Lecturas de Tarjetas"', async ({ page }) => {
      const contenido = await page.textContent('body');
      expect(contenido).toMatch(/Lecturas? de Tarjetas/i);
    });
  });

  test.describe('Sección: Lectores Qr', () => {
    
    test('Debe mostrar sección "Lectores Qr"', async ({ page }) => {
      const contenido = await page.textContent('body');
      expect(contenido).toMatch(/Lectores? Qr/i);
    });
  });

  test.describe('Sección: Estado de Entradas', () => {
    
    test('Debe mostrar sección "Estado de Entradas"', async ({ page }) => {
      const contenido = await page.textContent('body');
      expect(contenido).toMatch(/Estado de Entradas/i);
    });

    test('Debe mostrar dispositivo "DESNINGUNO"', async ({ page }) => {
      const contenido = await page.textContent('body');
      expect(contenido).toContain('DESNINGUNO');
    });
  });

  test.describe('Sección: Sensores Vehiculares', () => {
    
    test('Debe mostrar sección "Sensores Vehiculares"', async ({ page }) => {
      const contenido = await page.textContent('body');
      expect(contenido).toMatch(/Sensores Vehiculares/i);
    });

    test('Debe mostrar dispositivo "SLODESINGRESOPTA"', async ({ page }) => {
      const contenido = await page.textContent('body');
      expect(contenido).toContain('SLODESINGRESOPTA');
    });
  });

  test.describe('Sección: Activación de Salidas', () => {
    
    test('Debe mostrar sección "Activación de Salidas"', async ({ page }) => {
      const contenido = await page.textContent('body');
      expect(contenido).toMatch(/Activación de Salidas/i);
    });
  });

  test.describe('Sección: Activación de Displays', () => {
    
    test('Debe mostrar sección "Activación de Displays"', async ({ page }) => {
      const contenido = await page.textContent('body');
      expect(contenido).toMatch(/Activación de Displays/i);
    });
  });

  test.describe('Sección: Prueba de Ping', () => {
    
    test('Debe mostrar sección "Prueba de Ping"', async ({ page }) => {
      const contenido = await page.textContent('body');
      expect(contenido).toMatch(/Prueba de Ping/i);
    });

    test('Debe mostrar botón "Iniciar" en Prueba de Ping', async ({ page }) => {
      const contenido = await page.textContent('body');
      expect(contenido).toMatch(/Iniciar/i);
    });
  });

  test.describe('Sección: Prueba de Loop', () => {
    
    test('Debe mostrar sección "Prueba de Loop"', async ({ page }) => {
      const contenido = await page.textContent('body');
      expect(contenido).toMatch(/Prueba de Loop/i);
    });

    test('Debe mostrar botón "Iniciar" en Prueba de Loop', async ({ page }) => {
      const contenido = await page.textContent('body');
      expect(contenido).toMatch(/Iniciar/i);
    });

    test('Debe mostrar campo "Repeticiones"', async ({ page }) => {
      const contenido = await page.textContent('body');
      expect(contenido).toMatch(/Repeticiones/i);
    });
  });

  test.describe('Botones de Navegación', () => {
    
    test('Debe mostrar botón "Volver"', async ({ page }) => {
      const contenido = await page.textContent('body');
      expect(contenido).toMatch(/Volver/i);
    });

    test('Debe mostrar botón "Resuscribir"', async ({ page }) => {
      const contenido = await page.textContent('body');
      expect(contenido).toMatch(/Resuscribir/i);
    });
  });
});
