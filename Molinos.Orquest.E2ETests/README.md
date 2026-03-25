# Molinos.Orquest.E2ETests

Tests End-to-End (E2E) para la aplicación web del Orquestador de Molinos, utilizando Playwright.

## 📋 Descripción

Este proyecto contiene los tests E2E que verifican el contenido y estructura de la pantalla PruebaItc antes del rediseño, para asegurar que después del rediseño se mantenga exactamente el mismo contenido.

### Tests Implementados

#### PruebaItc Baseline (tests/PruebaItc-Baseline.spec.js)
**17 tests de visualización del contenido actual**

Estos tests capturan el contenido de la pantalla actual (ITC 5121 - PLC) para verificar que después del rediseño se mantenga el mismo contenido:

- ✅ **Header**: Título "Prueba Concentrador: PLC" y estado del concentrador
- ✅ **Sección: Lecturas de Tarjetas**
- ✅ **Sección: Lectores Qr**
- ✅ **Sección: Estado de Entradas** (incluye dispositivo DESNINGUNO)
- ✅ **Sección: Sensores Vehiculares** (incluye sensor SLODESINGRESOPTA)
- ✅ **Sección: Activación de Salidas**
- ✅ **Sección: Activación de Displays**
- ✅ **Sección: Prueba de Ping** (con botón Iniciar)
- ✅ **Sección: Prueba de Loop** (con botón Iniciar y campo Repeticiones)
- ✅ **Botones de Navegación**: Volver y Resuscribir

**Nota**: Los tests solo verifican que el contenido existe (visualización), NO ejecutan botones ni interacciones.

## 🚀 Instalación

### Prerrequisitos
- Node.js (v16 o superior)
- npm
- **Molinos.Orquest.Web ejecutándose en IIS**
- **Credenciales de Windows válidas** (la aplicación usa Windows Authentication)

### Instalar dependencias y navegadores

```bash
cd c:\MOA\ReposGit\Orquestador\Molinos.Orquest.E2ETests
npm install
npx playwright install
```

O ejecutar ambos comandos:

```bash
npm run setup:all
```

## ⚙️ Configuración

### Variables de Entorno

Crea un archivo `.env` basado en `.env.example`:

```bash
cp .env.example .env
```

Edita el archivo `.env` y configura:

```env
BASE_URL=http://localhost/Molinos.Orquest.Web
TEST_ITC_ID=5121
WINDOWS_USER=BAUNET\\tu-usuario
WINDOWS_PASSWORD=tu-password
```

**Importante**: 
- Usa **doble barra invertida** (`\\`) en `WINDOWS_USER`
- El usuario debe tener permisos en la aplicación
- No uses comillas en los valores

## 🧪 Ejecutar Tests

### Ejecutar tests baseline

```bash
npm run test:baseline
```

### Ejecutar tests con navegador visible

```bash
npm run test:baseline:headed
```

### Ejecutar todos los tests

```bash
npm test
```

### Modo debug (paso a paso)

```bash
npm run test:debug
```

### Modo UI interactivo

```bash
npm run test:ui
```

### Ver reporte HTML del último test

```bash
npm run test:report
```

## 📊 Reportes

Los reportes se generan automáticamente después de ejecutar los tests:
- **HTML Report**: `playwright-report/` - Ver con `npm run test:report`
- **Test Results**: `test-results/` - Resultados detallados
- **Screenshots**: Se capturan solo en caso de fallos
- **Videos**: Se graban solo en caso de fallos

## 🏗️ Estructura del Proyecto

```
Molinos.Orquest.E2ETests/
├── tests/
│   └── PruebaItc-Baseline.spec.js    # 17 tests de visualización de contenido
├── playwright.config.js               # Configuración (solo Chromium habilitado)
├── package.json                       # Dependencias y scripts
├── .env.example                       # Template de variables de entorno
├── .env                               # Variables de entorno (no versionado)
├── .gitignore                         # Archivos a ignorar
└── README.md                          # Esta documentación
```

## 🔧 Configuración de Playwright

### Navegadores Soportados

Solo **Chromium** está habilitado porque es el único navegador que soporta Windows Authentication correctamente con `httpCredentials`.

Firefox y Webkit están deshabilitados en `playwright.config.js`.

### Timeouts

- **Test timeout**: 60 segundos (aumentado para Windows Auth)
- **Navigation timeout**: 60 segundos
- **Action timeout**: 10 segundos
