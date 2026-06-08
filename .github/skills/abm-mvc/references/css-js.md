# Referencia: CSS (BEM) y JS (Vanilla)

## CSS — Estructura general

### variables.css (compartido, una sola vez en el proyecto)

```css
:root {
    --{pref}-primary:       #2563eb;
    --{pref}-primary-hover: #1d4ed8;
    --{pref}-white:         #ffffff;
    --{pref}-slate-300:     #cbd5e1;
    --{pref}-slate-700:     #334155;
    --{pref}-transition:    0.18s ease;
    --{pref}-shadow:        0 1px 3px rgba(0,0,0,.18);
    /* agregar según necesidad */
}
```

### {nombreDominioCamel}.css

```css
/* ==========================================================================
   BEM blocks para {NombreDominio}
   Prefijo: {pref}-
   ========================================================================== */

/* ── Layout ───────────────────────────────────────────────────── */
.{pref}-page   { padding: 1.5rem 2rem; }
.{pref}-header { display: flex; align-items: center; gap: 1rem; margin-bottom: 1.25rem; }
.{pref}-header__title   { font-size: 1.25rem; font-weight: 600; margin: 0; }
.{pref}-header__actions { margin-left: auto; display: flex; gap: 0.5rem; }

/* ── Card ─────────────────────────────────────────────────────── */
.{pref}-card        { background: var(--{pref}-white); border-radius: 0.5rem; box-shadow: var(--{pref}-shadow); }
.{pref}-card__header { display: flex; align-items: center; gap: 0.5rem; padding: 1rem 1.25rem; border-bottom: 1px solid var(--{pref}-slate-300); }
.{pref}-card__title  { font-size: 0.95rem; font-weight: 600; margin: 0; }
.{pref}-card__body   { padding: 1.25rem; }

/* ── Form ─────────────────────────────────────────────────────── */
.{pref}-form-row   { display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; }
.{pref}-form-group { display: flex; flex-direction: column; gap: 0.25rem; }
.{pref}-form-group__label  { font-size: 0.85rem; font-weight: 500; color: var(--{pref}-slate-700); }
.{pref}-form-group__input,
.{pref}-form-group__select { padding: 0.45rem 0.75rem; border: 1px solid var(--{pref}-slate-300); border-radius: 0.375rem; font-size: 0.9rem; }
.{pref}-form-group__error  { font-size: 0.8rem; color: #dc2626; }

/* ── Switch (campo Activo) ───────────────────────────────────── */
.{pref}-switch { display: flex; align-items: center; gap: 0.6rem; margin-bottom: 0.5rem; }

.{pref}-switch__input {
    position: absolute;
    width: 0;
    height: 0;
    opacity: 0;
    /* NO pointer-events: none — interfiere con activación por label en algunos browsers */
}

.{pref}-switch__track {
    position: relative;
    display: inline-block;
    width: 2.5rem;
    height: 1.375rem;
    background: var(--{pref}-slate-300);
    border-radius: 9999px;
    cursor: pointer;
    transition: background var(--{pref}-transition);
    flex-shrink: 0;
}

.{pref}-switch__thumb {
    position: absolute;
    top: 0.1875rem;
    left: 0.1875rem;
    width: 1rem;
    height: 1rem;
    background: var(--{pref}-white);
    border-radius: 50%;
    box-shadow: var(--{pref}-shadow);
    transition: transform var(--{pref}-transition);
}

/* IMPORTANTE: usar ~ (hermano general), NO + (adyacente).
   MVC inserta <input type="hidden"> entre el checkbox y la label,
   rompiendo el selector + */
.{pref}-switch__input:checked ~ .{pref}-switch__track            { background: var(--{pref}-primary); }
.{pref}-switch__input:checked ~ .{pref}-switch__track .{pref}-switch__thumb { transform: translateX(1.125rem); }

.{pref}-switch__label { font-size: 0.88rem; color: var(--{pref}-slate-700); cursor: pointer; margin: 0; }
```

### Reglas BEM

- Bloque: `.{pref}-card`
- Elemento: `.{pref}-card__header`
- Modificador: `.{pref}-card--destacado`
- No anidar selectores; usar clases planas
- No usar `#id` en CSS; solo en JS para `getElementById`
- No usar `!important`

---

## JS — Estructura base

```javascript
(function () {
    'use strict';

    // Datos del servidor (inyectados desde <script> en la vista)
    let someList = (window.{camel}SomeList || []).slice();

    document.addEventListener('DOMContentLoaded', function () {
        renderList();
        wireAddButton();
        wireActivoSwitch();
        wireFormSubmit();
    });

    // ── Switch Activo ─────────────────────────────────────────────────────
    function wireActivoSwitch() {
        const input = document.getElementById('{pref}-activo');
        const track = document.getElementById('{pref}-activo-track');
        track.addEventListener('click', function () {
            input.checked = !input.checked;
        });
    }

    // ── Botón agregar ─────────────────────────────────────────────────────
    function wireAddButton() {
        const btn = document.getElementById('{pref}-add-btn');
        btn.addEventListener('click', function () {
            openModal();
        });
    }

    // ── Submit ────────────────────────────────────────────────────────────
    function wireFormSubmit() {
        const form = document.getElementById('{pref}-form');
        form.addEventListener('submit', function () {
            syncHiddenInputs();
        });
    }

    function syncHiddenInputs() {
        const container = document.getElementById('{pref}-hidden');
        container.innerHTML = '';
        someList.forEach(function (item) {
            const input = document.createElement('input');
            input.type  = 'hidden';
            input.name  = 'itemIds';
            input.value = item.Id;
            container.appendChild(input);
        });
    }

    // ── Render ────────────────────────────────────────────────────────────
    function renderList() {
        const container = document.getElementById('{pref}-list');
        container.innerHTML = someList.map(renderItem).join('');
        syncHiddenInputs();
    }

    function renderItem(item) {
        return '<div class="{pref}-item">' +
               '<span class="{pref}-item__nombre">' + escHtml(item.Nombre) + '</span>' +
               '<button type="button" class="{pref}-item__remove" data-id="' + item.Id + '">' +
               '<span class="fa fa-times"></span></button>' +
               '</div>';
    }

    // ── Modal ─────────────────────────────────────────────────────────────
    function openModal() {
        // ... lógica de modal
    }

    // ── Utilidades ────────────────────────────────────────────────────────
    function escHtml(str) {
        return String(str)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;');
    }

    function escAttr(str) {
        return String(str).replace(/"/g, '&quot;').replace(/'/g, '&#39;');
    }

}());
```

### Reglas JS

| Regla | Motivo |
|-------|--------|
| `const` / `let`, nunca `var` | evitar hoisting no intencional |
| Sin guards `if (element)` en `getElementById` | se asume que los elementos existen cuando el script corre |
| Nombres de propiedades de objetos en PascalCase español (`{Id, Nombre, Ip}`) | coherencia con JSON serializado desde C# (`CamaraItemModel`) |
| Datos del servidor vía `window.{camel}*` en `<script>` de la vista | evitar que JSON con comillas dobles rompa `data-*` atributos |
| `StringEscapeHandling.EscapeHtml` en la serialización Razor | evitar XSS e inyección de scripts |
| `escHtml` / `escAttr` para todo string dinámico en HTML generado por JS | evitar XSS |
| IIFE `(function(){ 'use strict'; })()` | evitar contaminación del scope global |
