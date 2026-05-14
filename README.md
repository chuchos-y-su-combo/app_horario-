# README — Cambios para Issue #10

A continuación se listan **únicamente las secciones del README que deben modificarse**.
Aplica cada cambio en el archivo `README.md` existente.

---

## Cambio 1 — Tabla "Estado actual del proyecto"

Agregar la fila del Issue #10 después de la fila de Req 9:

```md
| Issue #10 | Implementado | Asignar asignaturas a docentes de forma manual |
```

La tabla queda así:

| Requerimiento | Estado | Descripción |
|---|---|---|
| Req 2 | Implementado | Crear y gestionar cuentas con rol administrador o coordinador |
| Req 3 | Implementado | Registrar docentes con nombre, identificación y tipo de contrato |
| Req 4 | Implementado | Registrar asignaturas con nombre, código, créditos, semestre y plan de estudios |
| Req 5 | Implementado | Limitar carga docente según contrato |
| Req 6 | Implementado | Cargar currículo docente desde Excel |
| Req 7 | Implementado | Marcar materias obligatorias TAPSI como fijas |
| Req 8 | Implementado | Contemplar asignatura adicional requerida para TAPSI jornada diurna |
| Disponibilidad docente | Implementado | Importar disponibilidad docente desde el Excel actual de coordinación |
| Req 9 | Implementado | Generar automáticamente propuestas de asignación para Ingeniería diurna, Ingeniería nocturna, TAPSI diurna y TAPSI nocturna |
| **Issue #10** | **Implementado** | **Asignar asignaturas a docentes de forma manual** |

---

## Cambio 2 — Sección "Objetivo del sistema" (lista de capacidades)

Agregar al final de la lista existente:

```md
- Asignar asignaturas a docentes de forma manual, con validación de currículo y límite de carga.
- Consultar las asignaturas disponibles para asignar a un docente en un periodo determinado.
```

---

## Cambio 3 — Sección de endpoints de "Asignaciones"

Reemplazar el bloque actual:

```http
GET /api/asignaciones
POST /api/asignaciones
GET /api/asignaciones/docente/{idDocente}
GET /api/asignaciones/docente/{idDocente}/resumen?periodo=2026-1
DELETE /api/asignaciones/{idAsignacion}
```

Por este (agrega los dos endpoints nuevos del Issue #10):

```http
GET  /api/asignaciones
POST /api/asignaciones
GET  /api/asignaciones/docente/{idDocente}
GET  /api/asignaciones/docente/{idDocente}/resumen?periodo=2026-1
DELETE /api/asignaciones/{idAsignacion}

# Issue #10 — Asignación manual
POST /api/asignaciones/manual
GET  /api/asignaciones/docente/{idDocente}/asignaturas-disponibles?periodo=2026-1
```

---

## Cambio 4 — Agregar sección nueva "Issue #10: asignación manual de asignaturas"

Insertar esta sección completa **después** del bloque de "Requerimiento 9" y **antes** del bloque "Cambios recientes de base de datos":

---

```md
# Issue #10: asignación manual de asignaturas a docentes

## Descripción

El sistema permite asignar asignaturas a docentes de forma manual para un periodo determinado.

La asignación manual es independiente del bloque horario: permite registrar primero
qué asignatura dicta cada docente y completar el horario detallado en una etapa posterior.

## Reglas de negocio aplicadas

- Se verifica que el docente exista.
- Se verifica que la asignatura exista.
- Se valida que el contrato del docente sea TC (máx. 5 asignaturas) o TP (máx. 3 asignaturas).
- Si el docente tiene currículo cargado, se valida que esté habilitado para dictar la asignatura.
  - El campo `ForzarSinCurriculo = true` permite omitir esta validación de forma excepcional.
- No se permite asignar la misma asignatura al mismo docente dos veces en el mismo periodo.

## Estado de la asignación manual

Las asignaciones creadas por este flujo quedan con estado `AsignadaManual`.

Los estados posibles en el sistema son:

| Estado | Origen |
|--------|--------|
| `Propuesta` | Generación automática (Req 9) |
| `AsignadaManual` | Asignación manual (Issue #10) |
| `Confirmada` | Confirmación posterior |
| `Cancelada` | Cancelación |

## Endpoints

### Asignar asignatura a docente de forma manual

```http
POST /api/asignaciones/manual
```

**Body:**
```json
{
  "idDocente": "uuid-del-docente",
  "idAsignatura": "uuid-de-la-asignatura",
  "periodo": "2026-1",
  "forzarSinCurriculo": false
}
```

**Respuesta exitosa (201 Created):**
```json
{
  "idAsignacion": "...",
  "idDocente": "...",
  "nombreDocente": "Juan Pérez",
  "tipoContrato": "TC",
  "maxAsignaturas": 5,
  "asignaturasActuales": 1,
  "idAsignatura": "...",
  "codigoAsignatura": "103001",
  "nombreAsignatura": "Cálculo I",
  "dia": 0,
  "horaInicio": "",
  "horaFin": "",
  "periodo": "2026-1",
  "estado": "AsignadaManual"
}
```

**Errores posibles (400 Bad Request):**
- Docente no encontrado.
- Asignatura no encontrada.
- Contrato del docente no válido.
- Docente no habilitado por currículo para la asignatura.
- Docente ya alcanzó el límite de asignaturas para el periodo.
- La asignatura ya está asignada al docente en el periodo.

---

### Consultar asignaturas disponibles para asignar a un docente

```http
GET /api/asignaciones/docente/{idDocente}/asignaturas-disponibles?periodo=2026-1
```

**Respuesta exitosa (200 OK):**
```json
[
  {
    "idAsignatura": "...",
    "codigo": "103001",
    "nombre": "Cálculo I",
    "creditos": 4,
    "semestre": 1,
    "habilitadaPorCurriculo": true,
    "yaAsignadaEnPeriodo": false
  },
  {
    "idAsignatura": "...",
    "codigo": "103002",
    "nombre": "Álgebra Lineal",
    "creditos": 3,
    "semestre": 2,
    "habilitadaPorCurriculo": false,
    "yaAsignadaEnPeriodo": true
  }
]
```

## Flujo de uso recomendado

```
1. Coordinador consulta asignaturas disponibles para el docente en el periodo:
   GET /api/asignaciones/docente/{id}/asignaturas-disponibles?periodo=2026-1

2. El sistema devuelve todas las asignaturas con dos indicadores:
   - HabilitadaPorCurriculo: si el docente está en la lista habilitada para esa materia.
   - YaAsignadaEnPeriodo: si ya tiene esa materia asignada en el periodo.

3. Coordinador selecciona una asignatura disponible y la asigna:
   POST /api/asignaciones/manual

4. La asignación queda registrada con estado "AsignadaManual".

5. En una etapa posterior se puede completar el bloque horario usando:
   POST /api/asignaciones  (con Dia, HoraInicio, HoraFin)
```
```

---

## Cambio 5 — Sección "Alcance actual / Implementado"

Agregar al final de la lista "Implementado":

```md
- Asignación manual de asignaturas a docentes (Issue #10).
- Consulta de asignaturas disponibles para un docente por periodo.
```

---

## Cambio 6 — Checklist antes de Pull Request

Agregar al final del checklist:

```txt
[ ] POST /api/asignaciones/manual funciona correctamente.
[ ] GET /api/asignaciones/docente/{id}/asignaturas-disponibles funciona correctamente.
[ ] Las asignaciones manuales quedan con estado "AsignadaManual".
[ ] El límite de carga se respeta en asignaciones manuales.
[ ] El issue #10 queda enlazado al PR.
```
