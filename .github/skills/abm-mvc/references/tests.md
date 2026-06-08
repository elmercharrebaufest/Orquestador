# Referencia: Tests

## Unit tests — NUnit (Molinos.Orquest.Test)

### Ubicación
`Molinos.Orquest.Test\Controllers\{NombreDominio}ControllerTest.cs`

### Estructura base

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using Molinos.Orquest.Dominio.Consultas;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Recursos;
using Molinos.Orquest.Repositorio;
using Molinos.Orquest.Servicios;
using Molinos.Orquest.Test.Mocks;
using Molinos.Orquest.Web.Controllers;
using Molinos.Orquest.Web.Conversiones;
using Molinos.Orquest.Web.Models;
using Molinos.Scato.Dominio.Consultas;
using Moq;
using NUnit.Framework;

namespace Molinos.Orquest.Test.Controllers
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable",
        Justification = "Test")]
    [TestFixture]
    public class {NombreDominio}ControllerTest
    {
        private {NombreDominio}Controller target;
        private Mock<IRepositorioFactory> repositorioFactoryMock;
        private Mock<IRepositorio>        repositorioMock;
        private Mock<IConversor>          conversorMock;
        private Mock<IServicioOrquestador> servicioMock;

        private List<{EntidadDominio}> items;

        [SetUp]
        public void SetUp()
        {
            servicioMock          = new Mock<IServicioOrquestador>();
            repositorioMock       = new Mock<IRepositorio>();
            repositorioFactoryMock = new Mock<IRepositorioFactory>();
            conversorMock         = new Mock<IConversor>();

            repositorioFactoryMock.Setup(f => f.Repositorio()).Returns(repositorioMock.Object);

            target = new {NombreDominio}Controller(
                repositorioFactoryMock.Object,
                conversorMock.Object,
                servicioMock.Object,
                new NullLogger());

            items = new List<{EntidadDominio}>
            {
                new {EntidadDominio} { Id = 1, Nombre = "Item 1", Codigo = "COD-001", Activo = true },
                new {EntidadDominio} { Id = 2, Nombre = "Item 2", Codigo = "COD-002", Activo = false }
            };

            // Setup dropdowns si aplica
            repositorioMock.Setup(r => r.Listar<OtroTipo>())
                           .Returns(new List<OtroTipo>());
        }

        [Test]
        public void TestIndex()
        {
            repositorioMock.Setup(r => r.Listar(
                It.IsAny<Expression<Func<{EntidadDominio}, bool>>>(),
                It.IsAny<Paginacion>()))
                .Returns(new ListaPaginada<{EntidadDominio}>(items, 1, 8, 2));

            var result = target.Index("") as ViewResult;
            IEnumerable<{EntidadDominio}> results = target.ViewBag.Items;

            Assert.That(result.ViewName, Is.Null.Or.Empty);
            Assert.That(results.Select(x => x.Id), Is.EquivalentTo(new[] { 1, 2 }));
        }

        [Test]
        public void TestListar()
        {
            repositorioMock.Setup(r => r.Listar(
                It.IsAny<Expression<Func<{EntidadDominio}, bool>>>(),
                It.IsAny<Paginacion>()))
                .Returns(new ListaPaginada<{EntidadDominio}>(items, 1, 8, 2));

            var result = target.Listar("") as ViewResult;

            Assert.That(result.ViewName, Is.EqualTo("Listar"));
        }

        [Test]
        public void TestCrearGet()
        {
            var result = target.Crear() as ViewResult;
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestCrearPost_ModeloValido_Redirige()
        {
            repositorioMock.Setup(r => r.Existe(
                It.IsAny<Expression<Func<{EntidadDominio}, bool>>>()))
                .Returns(false);

            var model = new {NombreDominio}Model { Nombre = "Nuevo", Codigo = "COD-NEW", Activo = true };
            var result = target.Crear(model) as RedirectToRouteResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.RouteValues["action"], Is.EqualTo("Index"));
            repositorioMock.Verify(r => r.Agregar(It.IsAny<{EntidadDominio}>()), Times.Once);
            repositorioMock.Verify(r => r.GuardarCambios(), Times.Once);
        }

        [Test]
        public void TestCrearPost_CodigoRepetido_RetornaVista()
        {
            repositorioMock.Setup(r => r.Existe(
                It.IsAny<Expression<Func<{EntidadDominio}, bool>>>()))
                .Returns(true);

            var model = new {NombreDominio}Model { Nombre = "Nuevo", Codigo = "COD-001" };
            var result = target.Crear(model) as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(target.ModelState.IsValid, Is.False);
        }

        [Test]
        public void TestModificarGet()
        {
            var entidad = items[0];
            repositorioMock.Setup(r => r.Obtener<{EntidadDominio}>(1)).Returns(entidad);
            conversorMock.Setup(c => c.Convertir<{EntidadDominio}, {NombreDominio}Model>(entidad))
                         .Returns(new {NombreDominio}Model { Id = 1, Nombre = entidad.Nombre });

            var result = target.Modificar(1) as ViewResult;
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestModificarPost_ModeloValido_Redirige()
        {
            var entidad = items[0];
            repositorioMock.Setup(r => r.Obtener<{EntidadDominio}>(1)).Returns(entidad);
            repositorioMock.Setup(r => r.Existe(
                It.IsAny<Expression<Func<{EntidadDominio}, bool>>>()))
                .Returns(false);

            var model = new {NombreDominio}Model { Id = 1, Nombre = "Modificado", Codigo = "COD-001" };
            var result = target.Modificar(model) as RedirectToRouteResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.RouteValues["action"], Is.EqualTo("Index"));
        }

        [Test]
        public void TestEliminar()
        {
            var entidad = items[0];
            repositorioMock.Setup(r => r.Obtener<{EntidadDominio}>(1)).Returns(entidad);

            var result = target.Eliminar(1);

            repositorioMock.Verify(r => r.Remover(entidad), Times.Once);
            repositorioMock.Verify(r => r.GuardarCambios(), Times.Once);
        }
    }
}
```

### Notas

- `NullLogger` está en `Molinos.Orquest.Test.Mocks`
- Si el controller recibe parámetros extra (ej: `int[] camaraIds`), agregar al mock correspondiente
- Verificar que `ViewBag.Lectores`, `ViewBag.Sensores`, etc. se popule en los tests GET y en los tests POST con error de validación

---

## E2E tests — Playwright (Molinos.Orquest.E2ETests)

### Ubicación
`Molinos.Orquest.E2ETests\tests\{NombreDominio}.spec.js`

### Estructura base

```javascript
require('dotenv').config();
const { test, expect } = require('@playwright/test');

const BASE_URL = process.env.BASE_URL;

test.describe('{NombreDominio} - ABM', () => {

    test.beforeEach(async ({ page }) => {
        await page.goto(`${BASE_URL}/{NombreDominio}/Index`);
        await page.waitForLoadState('domcontentloaded');
    });

    // ── Listado ──────────────────────────────────────────────────────────
    test.describe('Listado', () => {

        test('Debe mostrar el título', async ({ page }) => {
            const contenido = await page.textContent('body');
            expect(contenido).toMatch(/{TituloEsperado}/i);
        });

        test('Debe mostrar el botón Crear', async ({ page }) => {
            const crear = page.locator('a[href*="Crear"]');
            await expect(crear.first()).toBeVisible();
        });

        test('Debe mostrar el campo de filtro', async ({ page }) => {
            await expect(page.locator('input[name="filtro"]')).toBeVisible();
        });

        test('Debe mostrar columnas de la grilla', async ({ page }) => {
            const contenido = await page.textContent('body');
            expect(contenido).toMatch(/Nombre/i);
            expect(contenido).toMatch(/C[oó]digo/i);
            expect(contenido).toMatch(/Activo/i);
        });

    });

    // ── Crear ────────────────────────────────────────────────────────────
    test.describe('Formulario Crear', () => {

        test.beforeEach(async ({ page }) => {
            await page.goto(`${BASE_URL}/{NombreDominio}/Crear`);
            await page.waitForLoadState('domcontentloaded');
        });

        test('Debe mostrar el campo Nombre', async ({ page }) => {
            await expect(page.locator('input[name="Nombre"]')).toBeVisible();
        });

        test('Debe mostrar el campo Codigo', async ({ page }) => {
            await expect(page.locator('input[name="Codigo"]')).toBeVisible();
        });

        test('Debe mostrar el switch Activo', async ({ page }) => {
            await expect(page.locator('.{pref}-switch')).toBeVisible();
        });

        test('Debe mostrar error si se guarda sin datos requeridos', async ({ page }) => {
            await page.click('button[type="submit"]');
            await page.waitForLoadState('domcontentloaded');
            const contenido = await page.textContent('body');
            expect(contenido).toMatch(/requerido|obligatorio|required/i);
        });

        test('Debe guardar y redirigir al listado con datos válidos', async ({ page }) => {
            await page.fill('input[name="Nombre"]', 'Test E2E Nombre');
            await page.fill('input[name="Codigo"]', 'E2E-001');
            await page.click('button[type="submit"]');
            await page.waitForLoadState('domcontentloaded');
            expect(page.url()).toMatch(/{NombreDominio}\/Index/i);
        });

    });

    // ── Modificar ────────────────────────────────────────────────────────
    test.describe('Formulario Modificar', () => {

        test('Debe abrir el formulario de modificar', async ({ page }) => {
            const editBtn = page.locator('a[href*="Modificar"]').first();
            await editBtn.click();
            await page.waitForLoadState('domcontentloaded');
            expect(page.url()).toMatch(/Modificar/i);
        });

    });

    // ── Eliminar ─────────────────────────────────────────────────────────
    test.describe('Eliminar', () => {

        test('El botón eliminar existe en la grilla', async ({ page }) => {
            const deleteBtn = page.locator('.ajax-borrar-link').first();
            await expect(deleteBtn).toBeVisible();
        });

    });

});
```

### Ejecutar

```bash
# Desde Molinos.Orquest.E2ETests/
npx playwright test tests/{NombreDominio}.spec.js --reporter=line
```

### Variables de entorno

Copiar `.env.example` como `.env` y configurar `BASE_URL=http://localhost:{puerto}`.
