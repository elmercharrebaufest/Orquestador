---
name: user-story
description: >
  Estructura y plantillas para escribir y refinar historias de usuario, criterios de aceptación,
  reglas de negocio y análisis de gaps. Usar cuando el PO necesita formalizar un requerimiento,
  definir una historia, especificar acceptance criteria o priorizar el backlog.
---

# User Story — Estructura y Plantillas

## Historia de Usuario

**Formato estándar:**
```
Como [rol del usuario],
quiero [objetivo o acción],
para [beneficio o valor de negocio].
```

**Reglas:**
- El **rol** es un usuario real del sistema (operador de planta, supervisor, administrador, sistema SAP, etc.) — no un actor técnico
- El **objetivo** describe qué quiere hacer, no cómo se implementa
- El **beneficio** justifica por qué es valioso — si no hay beneficio claro, la historia no debería existir

**Ejemplo:**
```
Como operador de planta,
quiero visualizar el estado actual de todas las barreras de un sector,
para poder reaccionar rápidamente ante una barrera bloqueada sin tener que ir físicamente.
```

---

## Criterios de Aceptación

**Formato Given/When/Then:**
```
Dado que [contexto o precondición],
cuando [acción del usuario o evento del sistema],
entonces [resultado observable y verificable].
```

**Reglas:**
- Cada criterio debe ser **verificable** — si no se puede probar, no es un criterio
- Un criterio por escenario; no mezclar múltiples condiciones en uno
- Cubrir: camino feliz, errores esperados, casos borde

**Ejemplo:**
```
Dado que la barrera BARRERA-01 está en estado "Cerrada",
cuando el operador ejecuta la acción "Abrir",
entonces la barrera cambia a estado "Abierta" en menos de 5 segundos
  y el evento queda registrado en el historial con usuario, fecha y hora.

Dado que la barrera BARRERA-01 no responde (sin conexión),
cuando el operador ejecuta la acción "Abrir",
entonces se muestra el mensaje "El dispositivo no responde. Verificar conexión."
  y el estado de la barrera permanece sin cambios.
```

---

## Reglas de Negocio

Para invariantes y restricciones del sistema que aplican siempre (no solo en un escenario):

```
RN-01: Un dispositivo solo puede pertenecer a un sector a la vez.
RN-02: Solo usuarios con permiso "ConfigBarrera" pueden crear o modificar configuraciones de barrera.
RN-03: Un código de dispositivo es único en todo el sistema y se almacena en mayúsculas.
RN-04: No se pueden eliminar dispositivos que tengan suscripciones activas.
```

**Formato:** `RN-[número]: [restricción en lenguaje de negocio, sin tecnicismos]`

---

## Análisis de Gaps

Cuando una historia o especificación existente tiene inconsistencias o vacíos:

**Formato:**
```
## Gaps identificados

### Ambigüedades
- [Elemento]: [Qué no está claro y por qué importa aclararlo]

### Escenarios faltantes
- [Escenario que no está cubierto y debería estarlo]

### Conflictos
- [Regla A] contradice [Regla B] en [situación específica]

### Preguntas abiertas
- ¿[Pregunta] → Decisión necesaria para poder implementar?
```

---

## Plantilla completa de historia

```markdown
## [Título corto de la historia]

**Como** [rol],
**quiero** [objetivo],
**para** [beneficio].

### Contexto
[Descripción breve del problema o necesidad de negocio que motiva esta historia]

### Criterios de aceptación
1. Dado [contexto], cuando [acción], entonces [resultado].
2. Dado [contexto de error], cuando [acción], entonces [mensaje/comportamiento esperado].
3. [...]

### Reglas de negocio aplicables
- RN-XX: [...]

### Notas
- [Decisiones tomadas, referencias, dependencias con otras historias]
```
