# ApplicationSchedule API

Backend para la gestión de horarios académicos.

El sistema permite administrar usuarios, roles, docentes, asignaturas, asignaciones académicas, currículo docente, disponibilidad docente y reglas especiales para estudiantes provenientes de TAPSI.

Está desarrollado en C# con ASP.NET Core, Entity Framework Core y SQLite.

---

## Tecnologías utilizadas

- C#
- .NET 10.0
- ASP.NET Core Web API
- Entity Framework Core
- SQLite
- ClosedXML
- Swagger / OpenAPI
- xUnit
- FluentAssertions
- BCrypt para hash de contraseñas
- Git Flow
- GitHub / GitHub Desktop

---

## Estado actual del proyecto

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
| Req 9 | Implementado | Generar automáticamente propuestas de asignación para los 4 escenarios |
| Issue #10 | Implementado | Asignar asignaturas a docentes de forma manual |
| Issue #11 | Implementado | Reducir disponibilidad de un docente que dicta la misma materia en jornada diurna y nocturna |
| Issue #12 | Implementado | Revisar y ajustar manualmente la propuesta generada antes de confirmarla |
| Issue #16 | Implementado | Generar reporte de horas asignadas vs carga contractual por docente |
| Issue #18 | Implementado | Alertar cuando una asignación genera conflicto |
| Issue #39 | Implementado | Mostrar el horario en vista de calendario semanal filtrable por plan y jornada |
| Issue #40 | Implementado | Mostrar el horario individual de cada docente y colocar asignaturas en un día de la semana |

---

## Objetivo del sistema

El objetivo del sistema es apoyar la gestión académica necesaria para la construcción de horarios universitarios.

El sistema permite:

- Gestionar usuarios administradores y coordinadores.
- Registrar docentes y asignaturas.
- Asociar asignaturas a planes de estudio.
- Controlar la carga docente según tipo de contrato.
- Importar currículo docente y disponibilidad desde Excel.
- Marcar materias TAPSI obligatorias como fijas.
- Generar automáticamente propuestas de asignación para 4 escenarios.
- Asignar asignaturas a docentes de forma manual.
- Colocar asignaciones en un día y bloque horario específico.
- Revisar, ajustar y confirmar propuestas antes de publicar el horario.
- Generar reporte de horas asignadas vs carga contractual por docente.
- Detectar y alertar conflictos en las asignaciones del semestre.
- Mostrar el horario en vista de calendario semanal filtrable por plan y jornada.
- Mostrar el horario individual de cada docente en vista de calendario semanal.

---

## Base de datos

El proyecto utiliza SQLite adaptado desde el modelo original en MySQL Workbench.

### Adaptaciones para SQLite

| MySQL original | SQLite |
|---|---|
| `CREATE DATABASE` | No se usa |
| `CHAR(36)` | `TEXT` |
| `ENUM` | `TEXT CHECK (...)` |
| `TINYINT(1)` | `INTEGER CHECK (0, 1)` |
| `TIME` | `TEXT` con formato `HH:mm` |

### Tablas principales

```txt
roles, usuarios, planes_estudio, docentes, asignaturas,
docentes_habilitados, disponibilidad, asignaciones, bloqueos
```

---

## Estructura del proyecto

```txt
src/
├── ApplicationSchedule.Api           → Controladores, Program.cs, Swagger
├── ApplicationSchedule.Application   → DTOs, Interfaces
├── ApplicationSchedule.Domain        → Entidades
├── ApplicationSchedule.Infrastructure → DbContext, Servicios
└── ApplicationSchedule.Tests         → Pruebas de integración
database/
docs/
README.md
```

---

## Configuración

Cadena de conexión en `src/ApplicationSchedule.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=horarios.db"
  }
}
```

La base se crea automáticamente con `context.Database.EnsureCreated()`.

`.gitignore` recomendado:

```gitignore
*.db
*.db-shm
*.db-wal
bin/
obj/
.vs/
.idea/
*.user
*.dll
*.pdb
```

---

## Datos base creados automáticamente

| Roles | ID |
|---|---|
| Administrador | 1 |
| Coordinador | 2 |

| Plan de estudios | ID | Jornada |
|---|---|---|
| Plan de Estudios 1020 Jornada Diurna | `11111111-1111-1111-1111-111111111111` | Diurna |
| Plan de Estudios Jornada Noche | `22222222-2222-2222-2222-222222222222` | Nocturna |

---

# Requerimientos base (Req 2 al Req 9)

## Req 2 — Usuarios

```http
GET    /api/usuarios
POST   /api/usuarios
GET    /api/usuarios/{idUsuario}
PUT    /api/usuarios/{idUsuario}
PATCH  /api/usuarios/{idUsuario}/password
DELETE /api/usuarios/{idUsuario}
```

## Req 3 — Docentes

```http
GET    /api/profesores
POST   /api/profesores
GET    /api/profesores/{idProfesor}
PUT    /api/profesores/{idProfesor}
DELETE /api/profesores/{idProfesor}
```

Tipos de contrato: `TC` (máx. 5 asignaturas) y `TP` (máx. 3 asignaturas).

## Req 4 — Asignaturas

```http
GET    /api/asignaturas
POST   /api/asignaturas
GET    /api/asignaturas/{idAsignatura}
GET    /api/asignaturas/plan/{idPlan}
PUT    /api/asignaturas/{idAsignatura}
DELETE /api/asignaturas/{idAsignatura}
```

## Req 5 — Límite de carga docente

El sistema cuenta asignaturas **distintas** por docente y periodo, no bloques horarios.

```http
GET    /api/asignaciones
POST   /api/asignaciones
GET    /api/asignaciones/docente/{idDocente}
GET    /api/asignaciones/docente/{idDocente}/resumen?periodo=2026-1
DELETE /api/asignaciones/{idAsignacion}
```

## Req 6 y Disponibilidad — Excel

```http
POST /api/profesores/importar-excel
GET  /api/profesores/{idProfesor}/asignaturas-habilitadas
GET  /api/profesores/{idProfesor}/disponibilidad
```

## Req 7 — TAPSI fijas

```http
GET  /api/asignaturas/tapsi/fijas
POST /api/asignaturas/tapsi/marcar-fijas
```

## Req 8 — TAPSI jornada diurna

```http
GET  /api/asignaturas/tapsi/diurna/opciones-adicionales
GET  /api/asignaturas/tapsi/diurna/plan
POST /api/asignaturas/tapsi/diurna/marcar-opciones-adicionales
```

## Req 9 — Generación automática de propuestas

Escenarios: `ING_DIURNA`, `ING_NOCTURNA`, `TAPSI_DIURNA`, `TAPSI_NOCTURNA`.

```http
POST /api/horarios/generar-propuestas
```

---

# Issue #10: asignación manual de asignaturas

## Descripción

El coordinador asigna manualmente una asignatura a un docente para un semestre, sin bloque horario. El bloque se puede completar después con el Issue #40.

## Reglas de negocio

- Valida existencia de docente y asignatura.
- Respeta límite de carga por contrato.
- Si hay currículo cargado, valida que el docente esté habilitado (`ForzarSinCurriculo = true` para omitir).
- No permite duplicar la misma asignatura en el mismo semestre.
- La asignación queda con estado `AsignadaManual`, `Dia = 0`, horas vacías.

## Endpoints

```http
POST /api/asignaciones/manual
GET  /api/asignaciones/docente/{idDocente}/asignaturas-disponibles?periodo=2026-1
```

### POST /api/asignaciones/manual

```json
{
  "idDocente": "uuid",
  "idAsignatura": "uuid",
  "periodo": "2026-1",
  "forzarSinCurriculo": false
}
```

Respuesta (201): asignación con `"estado": "AsignadaManual"`, `"dia": 0`, `"horaInicio": ""`.

---

# Issue #11: reducción de disponibilidad por doble jornada

## Descripción

Cuando un docente dicta la misma asignatura en jornada diurna y nocturna, el sistema elimina los bloques de disponibilidad que se solapan con la jornada diurna.

## Endpoint

```http
POST /api/curriculos-docentes/{idDocente}/reducir-disponibilidad?idAsignatura={id}&periodo=2026-1
```

Respuesta (200):

```json
{
  "bloquesEliminados": 1,
  "bloquesAfectados": ["Día 1 08:00-10:00"],
  "mensaje": "Se eliminaron 1 bloque(s) de disponibilidad por doble jornada."
}
```

---

# Issue #12: revisión y ajuste de propuestas

## Descripción

El coordinador revisa las propuestas generadas, ajusta campos, cancela las que no aplican y confirma en bloque.

## Estados de una asignación

| Estado | Puede ajustarse | Puede confirmarse | Puede cancelarse |
|--------|:-:|:-:|:-:|
| `Propuesta` | ✅ | ✅ | ✅ |
| `AsignadaManual` | ✅ | ✅ | ✅ |
| `Confirmada` | ❌ | ❌ | ❌ |
| `Cancelada` | ❌ | ❌ | ❌ |

## Endpoints

```http
GET   /api/asignaciones/propuestas?periodo=2026-1
PATCH /api/asignaciones/{idAsignacion}/ajustar
POST  /api/asignaciones/confirmar
PATCH /api/asignaciones/{idAsignacion}/cancelar
```

### PATCH /api/asignaciones/{id}/ajustar

Todos los campos son opcionales. Solo se actualiza lo que se envíe.

```json
{
  "idDocente": "nuevo-uuid",
  "idAsignatura": "nuevo-uuid",
  "dia": 3,
  "horaInicio": "10:00",
  "horaFin": "12:00",
  "periodo": "2026-1"
}
```

### POST /api/asignaciones/confirmar

```json
{
  "idsAsignacion": ["uuid-1", "uuid-2", "uuid-3"]
}
```

Respuesta:

```json
{
  "confirmadas": 2,
  "fallidas": 1,
  "idsConfirmadas": ["uuid-1", "uuid-2"],
  "errores": [{ "idAsignacion": "uuid-3", "motivo": "No se puede confirmar una asignación cancelada." }]
}
```

## Flujo de uso

```
1. POST /api/horarios/generar-propuestas
2. GET  /api/asignaciones/propuestas?periodo=2026-1
3. PATCH /api/asignaciones/{id}/ajustar         (corregir lo necesario)
4. PATCH /api/asignaciones/{id}/cancelar         (descartar las que no aplican)
5. POST  /api/asignaciones/confirmar             (aprobar en bloque)
```

---

# Issue #16: reporte de horas vs carga contractual

## Descripción

El sistema compara las horas de clase asignadas a cada docente contra su carga contractual de referencia.

## Horas contractuales de referencia

| Contrato | Horas semanales |
|---|---|
| TC | 40 |
| TP | 20 |

## Clasificación del estado de carga

| Condición | EstadoCarga |
|---|---|
| Sin asignaturas | `Sin asignaciones` |
| Horas > contractuales | `Excedida` |
| Horas ≥ 90% contractuales | `Completa` |
| Horas < 90% contractuales | `Parcial` |

## Endpoints

```http
GET /api/reportes/carga-docente?semestre=2026-1
GET /api/reportes/carga-docente/{idDocente}?semestre=2026-1
```

Respuesta (200):

```json
{
  "semestre": "2026-1",
  "totalDocentes": 10,
  "docentesConCargaCompleta": 4,
  "docentesConCargaParcial": 3,
  "docentesConCargaExcedida": 1,
  "docentesSinAsignaciones": 2,
  "docentes": [
    {
      "nombreDocente": "Carlos Pérez",
      "tipoContrato": "TC",
      "asignaturasAsignadas": 3,
      "totalHorasSemanales": 6.0,
      "horasContractuales": 40.0,
      "diferenciaHoras": -34.0,
      "porcentajeCarga": 15.0,
      "estadoCarga": "Parcial",
      "asignaturas": [...]
    }
  ]
}
```

---

# Issue #18: alertas de conflictos

## Descripción

El sistema analiza todas las asignaciones activas de un semestre y detecta cuatro tipos de conflictos.

## Tipos de conflicto

| Tipo | Severidad | Descripción |
|---|---|---|
| `CruceHorario` | Error | Dos asignaciones del mismo docente se solapan en día y hora |
| `ExcesoCarga` | Error | El docente supera el máximo de asignaturas de su contrato |
| `AsignaturaSinDocente` | Advertencia | Una asignatura no tiene docente asignado en el semestre |
| `DocenteSinHorario` | Advertencia | Una asignación no tiene bloque horario definido |

## Endpoint

```http
GET /api/reportes/conflictos?semestre=2026-1
```

Respuesta (200):

```json
{
  "semestre": "2026-1",
  "totalConflictos": 2,
  "tieneConflictos": true,
  "conflictos": [
    {
      "tipoConflicto": "CruceHorario",
      "severidad": "Error",
      "descripcion": "El docente 'Carlos Pérez' tiene cruce horario el día 1 entre 'Cálculo I' (08:00-10:00) y 'Álgebra' (09:00-11:00).",
      "idDocente": "...",
      "nombreDocente": "Carlos Pérez",
      "idAsignacion1": "...",
      "idAsignacion2": "...",
      "detalleHorario": "Día 1: 08:00-10:00 vs 09:00-11:00"
    }
  ]
}
```

## Flujo de uso recomendado

```
1. GET /api/reportes/conflictos?semestre=2026-1
2. Corregir errores:
   - CruceHorario  → PATCH /api/asignaciones/{id}/asignar-dia
   - ExcesoCarga   → DELETE /api/asignaciones/{id}
3. Resolver advertencias:
   - AsignaturaSinDocente → POST /api/asignaciones/manual
   - DocenteSinHorario   → PATCH /api/asignaciones/{id}/asignar-dia
4. Repetir hasta que totalConflictos sea 0.
5. POST /api/asignaciones/confirmar
```

---

# Issue #39: calendario semanal filtrable

## Descripción

El sistema muestra el horario en vista de calendario semanal (Lunes a Sábado), filtrable por plan de estudios y jornada. Solo aparecen asignaciones con bloque horario definido.

## Endpoint

```http
GET /api/horarios/calendario?semestre=2026-1
GET /api/horarios/calendario?semestre=2026-1&jornada=Diurna
GET /api/horarios/calendario?semestre=2026-1&jornada=Nocturna
GET /api/horarios/calendario?semestre=2026-1&idPlan=11111111-1111-1111-1111-111111111111
GET /api/horarios/calendario?semestre=2026-1&idPlan=11111111-1111-1111-1111-111111111111&jornada=Diurna
```

Parámetros:

| Parámetro | Obligatorio | Valores válidos |
|---|---|---|
| `semestre` | ✅ | Ej: `2026-1` |
| `jornada` | ❌ | `Diurna` o `Nocturna` |
| `idPlan` | ❌ | UUID del plan de estudios |

Respuesta (200):

```json
{
  "semestre": "2026-1",
  "idPlanFiltro": null,
  "jornadaFiltro": "Diurna",
  "dias": [
    {
      "numeroDia": 1,
      "nombreDia": "Lunes",
      "bloques": [
        {
          "idAsignacion": "...",
          "horaInicio": "08:00",
          "horaFin": "10:00",
          "nombreAsignatura": "Cálculo I",
          "codigoAsignatura": "MAT001",
          "nombreDocente": "Carlos Pérez",
          "escenario": "ING_DIURNA",
          "jornada": "Diurna",
          "nombrePlan": "Plan de Estudios 1020 Jornada Diurna",
          "idPlan": "11111111-1111-1111-1111-111111111111",
          "estado": "Confirmada"
        }
      ]
    }
  ]
}
```

---

# Issue #40: horario individual del docente y asignación de día

## Descripción

El sistema permite dos cosas relacionadas:

1. **Ver el horario individual de un docente** en vista de calendario semanal con datos de carga.
2. **Colocar una asignación en un día de la semana**, con o sin bloque horario. Una vez con día y hora definidos, la asignación aparece en el calendario.

---

## Parte A — Ver horario individual del docente

### Endpoint

```http
GET /api/horarios/calendario/docente/{idDocente}?semestre=2026-1
```

Devuelve el calendario semanal del docente con su información de carga.

- Las asignaciones **con** bloque horario aparecen en su día correspondiente.
- Las asignaciones **sin** bloque horario cuentan en `TotalAsignaturas` pero no en `TotalHorasSemanales` ni en el calendario.

Respuesta (200):

```json
{
  "idDocente": "...",
  "nombreDocente": "Carlos Pérez",
  "identificacion": "1001",
  "tipoContrato": "TC",
  "maxAsignaturas": 5,
  "semestre": "2026-1",
  "totalAsignaturas": 3,
  "totalHorasSemanales": 6.0,
  "dias": [
    {
      "numeroDia": 1,
      "nombreDia": "Lunes",
      "bloques": [
        {
          "horaInicio": "08:00",
          "horaFin": "10:00",
          "nombreAsignatura": "Cálculo I",
          "codigoAsignatura": "MAT001",
          "escenario": "ING_DIURNA",
          "estado": "Confirmada"
        }
      ]
    }
  ]
}
```

Errores posibles:

- `400`: falta el parámetro `semestre`.
- `404`: docente no encontrado.

---

## Parte B — Colocar una asignación en un día

### Endpoint

```http
PATCH /api/asignaciones/{idAsignacion}/asignar-dia
```

Coloca una asignación existente en un día de la semana. La hora es opcional: si no se envía, la asignación queda registrada en el día pero sin hora definida y no aparece en el calendario hasta completarla.

Solo aplica a asignaciones en estado `Propuesta` o `AsignadaManual`.

Body:

```json
{
  "dia": 3,
  "horaInicio": "10:00",
  "horaFin": "12:00"
}
```

O solo el día sin hora:

```json
{
  "dia": 3
}
```

Respuesta exitosa (200): devuelve la asignación actualizada.

Reglas de validación:

- `dia` es obligatorio (1 = Lunes … 6 = Sábado).
- Si se envía `horaInicio`, se debe enviar también `horaFin` y viceversa.
- `horaInicio` debe ser menor que `horaFin`.
- No se puede modificar una asignación `Confirmada` o `Cancelada`.

Errores posibles (400):

- Asignación no encontrada.
- Asignación ya confirmada o cancelada.
- Solo se envió una de las dos horas.
- Hora de inicio mayor o igual que hora de fin.

---

## Flujo completo Issue #40

```
1. Crear asignación manual (sin día ni hora):
   POST /api/asignaciones/manual

2. Colocarla en un día con horario:
   PATCH /api/asignaciones/{id}/asignar-dia
   { "dia": 3, "horaInicio": "10:00", "horaFin": "12:00" }

3. Verificar que aparece en el calendario semanal general:
   GET /api/horarios/calendario?semestre=2026-1

4. Verificar que aparece en el calendario individual del docente:
   GET /api/horarios/calendario/docente/{idDocente}?semestre=2026-1

5. Detectar conflictos antes de confirmar:
   GET /api/reportes/conflictos?semestre=2026-1

6. Confirmar:
   POST /api/asignaciones/confirmar
```

---

# Cambios de base de datos acumulados

## Estado del campo `estado` en `asignaciones`

```sql
estado TEXT NOT NULL CHECK(estado IN ('Propuesta', 'Confirmada', 'Cancelada', 'AsignadaManual'))
```

## Columna en `asignaturas` (Req 8)

```sql
ALTER TABLE asignaturas
ADD COLUMN es_opcional_tapsi_diurna INTEGER NOT NULL DEFAULT 0
CHECK (es_opcional_tapsi_diurna IN (0, 1));
```

## Tabla `disponibilidad`

```sql
CREATE TABLE IF NOT EXISTS disponibilidad (
    id_disponibilidad TEXT PRIMARY KEY,
    id_docente TEXT NOT NULL,
    dia_semana INTEGER NOT NULL CHECK (dia_semana BETWEEN 1 AND 6),
    hora_inicio TEXT NOT NULL,
    hora_fin TEXT NOT NULL,
    FOREIGN KEY (id_docente) REFERENCES docentes(id_docente) ON DELETE CASCADE
);
```

---

# Todos los endpoints

## Usuarios

```http
GET    /api/usuarios
POST   /api/usuarios
GET    /api/usuarios/{idUsuario}
PUT    /api/usuarios/{idUsuario}
PATCH  /api/usuarios/{idUsuario}/password
DELETE /api/usuarios/{idUsuario}
```

## Docentes

```http
GET    /api/profesores
POST   /api/profesores
GET    /api/profesores/{idProfesor}
PUT    /api/profesores/{idProfesor}
DELETE /api/profesores/{idProfesor}
POST   /api/profesores/importar-excel
GET    /api/profesores/{idProfesor}/asignaturas-habilitadas
GET    /api/profesores/{idProfesor}/disponibilidad
```

## Asignaturas

```http
GET    /api/asignaturas
POST   /api/asignaturas
GET    /api/asignaturas/{idAsignatura}
GET    /api/asignaturas/plan/{idPlan}
PUT    /api/asignaturas/{idAsignatura}
DELETE /api/asignaturas/{idAsignatura}
GET    /api/asignaturas/tapsi/fijas
POST   /api/asignaturas/tapsi/marcar-fijas
GET    /api/asignaturas/tapsi/diurna/opciones-adicionales
GET    /api/asignaturas/tapsi/diurna/plan
POST   /api/asignaturas/tapsi/diurna/marcar-opciones-adicionales
```

## Asignaciones

```http
GET    /api/asignaciones
POST   /api/asignaciones
GET    /api/asignaciones/docente/{idDocente}
GET    /api/asignaciones/docente/{idDocente}/resumen?periodo=2026-1
DELETE /api/asignaciones/{idAsignacion}

# Issue #10 — Manual
POST   /api/asignaciones/manual
GET    /api/asignaciones/docente/{idDocente}/asignaturas-disponibles?periodo=2026-1

# Issue #12 — Revisión
GET    /api/asignaciones/propuestas?periodo=2026-1
PATCH  /api/asignaciones/{idAsignacion}/ajustar
POST   /api/asignaciones/confirmar
PATCH  /api/asignaciones/{idAsignacion}/cancelar

# Issue #40 — Colocar en día
PATCH  /api/asignaciones/{idAsignacion}/asignar-dia
```

## Currículo y disponibilidad

```http
POST /api/curriculos-docentes/{idDocente}/reducir-disponibilidad?idAsignatura={id}&periodo=2026-1
```

## Horarios

```http
POST /api/horarios/generar-propuestas
GET  /api/horarios/exportar?periodo=2026-1
GET  /api/horarios/calendario?semestre=2026-1
GET  /api/horarios/calendario/docente/{idDocente}?semestre=2026-1
```

## Reportes

```http
GET /api/reportes/carga-docente?semestre=2026-1
GET /api/reportes/carga-docente/{idDocente}?semestre=2026-1
GET /api/reportes/conflictos?semestre=2026-1
```

---

# Cómo ejecutar el proyecto

```powershell
dotnet restore .\src\ApplicationSchedule.Api\ApplicationSchedule.Api.csproj
dotnet build   .\src\ApplicationSchedule.Api\ApplicationSchedule.Api.csproj
dotnet run --project .\src\ApplicationSchedule.Api\ApplicationSchedule.Api.csproj
```

API: `http://localhost:5213`
Swagger: `http://localhost:5213/swagger`

---

# Cómo ejecutar las pruebas

```powershell
dotnet build .\src\ApplicationSchedule.Tests\ApplicationSchedule.Tests.csproj
dotnet test  .\src\ApplicationSchedule.Tests\ApplicationSchedule.Tests.csproj
```

Con detalle:

```powershell
dotnet test .\src\ApplicationSchedule.Tests\ApplicationSchedule.Tests.csproj --logger "console;verbosity=detailed"
```

---

# Guía de pruebas manuales en Swagger

## Paso 1 — Crear docente y asignatura

```http
POST /api/profesores
{ "nombre": "Carlos Pérez", "identificacion": "1001", "tipoContrato": "TC" }

POST /api/asignaturas
{ "idPlan": "11111111-1111-1111-1111-111111111111", "codigo": "MAT001", "nombre": "Cálculo I", "creditos": 4, "semestre": 1 }
```

## Paso 2 — Asignar manualmente (Issue #10)

```http
POST /api/asignaciones/manual
{ "idDocente": "...", "idAsignatura": "...", "periodo": "2026-1", "forzarSinCurriculo": false }
```

## Paso 3 — Colocar en un día con horario (Issue #40)

```http
PATCH /api/asignaciones/{id}/asignar-dia
{ "dia": 3, "horaInicio": "10:00", "horaFin": "12:00" }
```

## Paso 4 — Ver en el calendario semanal (Issue #39)

```http
GET /api/horarios/calendario?semestre=2026-1
GET /api/horarios/calendario?semestre=2026-1&jornada=Diurna
```

## Paso 5 — Ver horario individual del docente (Issue #40)

```http
GET /api/horarios/calendario/docente/{idDocente}?semestre=2026-1
```

## Paso 6 — Detectar conflictos (Issue #18)

```http
GET /api/reportes/conflictos?semestre=2026-1
```

Corregir todos los `Error` antes de confirmar.

## Paso 7 — Confirmar propuestas (Issue #12)

```http
POST /api/asignaciones/confirmar
{ "idsAsignacion": ["uuid-1", "uuid-2"] }
```

## Paso 8 — Reporte de carga docente (Issue #16)

```http
GET /api/reportes/carga-docente?semestre=2026-1
```

---

# Git Flow

| Issue | Rama sugerida |
|---|---|
| Issue #10 | `feature/asignacion-manual-docente` |
| Issue #11 | `feature/reduccion-disponibilidad-doble-jornada` |
| Issue #12 | `feature/revision-ajuste-confirmacion-propuestas` |
| Issue #16 | `feature/reporte-carga-docente` |
| Issue #18 | `feature/alertas-conflictos-asignaciones` |
| Issue #39 | `feature/calendario-semanal` |
| Issue #40 | `feature/horario-individual-docente` |

```bash
git checkout develop
git pull origin develop
git checkout -b feature/horario-individual-docente
git add .
git commit -m "feat(horarios): horario individual docente y asignación de día a bloque"
git push origin feature/horario-individual-docente
```

---

# Alcance actual

## Implementado

- Gestión de usuarios y roles.
- Registro de docentes con límite de carga por contrato.
- Registro de asignaturas con planes de estudio.
- Materias TAPSI fijas y opciones diurnas.
- Importación de currículo y disponibilidad desde Excel.
- Generación automática de propuestas para 4 escenarios.
- **Issue #10:** Asignación manual sin bloque horario.
- **Issue #11:** Reducción de disponibilidad por doble jornada.
- **Issue #12:** Ajuste, cancelación y confirmación de propuestas.
- **Issue #16:** Reporte de horas asignadas vs carga contractual.
- **Issue #18:** Detección de cruces, exceso de carga, asignaturas sin docente y docentes sin horario.
- **Issue #39:** Calendario semanal filtrable por plan y jornada.
- **Issue #40:** Horario individual de docente en vista semanal.
- **Issue #40:** Endpoint para colocar una asignación en un día y bloque horario.

## Pendiente o futuro

- Validación automática de conflictos al crear asignaciones.
- Interfaz visual del frontend.
- Reglas avanzadas de selección de materia adicional TAPSI diurna.

---

# Checklist antes de Pull Request

```txt
[ ] dotnet build sin errores.
[ ] dotnet test sin fallos.
[ ] Swagger abre correctamente.

# Issue #10
[ ] POST /api/asignaciones/manual crea con estado "AsignadaManual".
[ ] Límite de carga se respeta. Duplicado rechazado con 400.

# Issue #11
[ ] Reduce disponibilidad solo cuando hay doble jornada.
[ ] Con una sola jornada responde bloquesEliminados: 0.

# Issue #12
[ ] Ajuste solo aplica a Propuesta y AsignadaManual.
[ ] Confirmación en bloque reporta fallidas sin interrumpir exitosas.
[ ] Cancelar rechaza Confirmadas con 400.

# Issue #16
[ ] Reporte general del semestre con todos los docentes.
[ ] Reporte individual con detalle de asignaturas.
[ ] EstadoCarga clasifica correctamente.

# Issue #18
[ ] Detecta CruceHorario, ExcesoCarga, AsignaturaSinDocente, DocenteSinHorario.
[ ] Bloques en días distintos no generan cruce.
[ ] tieneConflictos es false cuando no hay conflictos.

# Issue #39
[ ] GET /api/horarios/calendario retorna 6 días siempre.
[ ] Filtro por jornada funciona (Diurna / Nocturna).
[ ] Filtro por idPlan funciona.
[ ] Asignaciones sin horario no aparecen en el calendario.
[ ] Retorna 400 con jornada inválida.

# Issue #40
[ ] GET /api/horarios/calendario/docente/{id} retorna 6 días y datos del docente.
[ ] Solo muestra asignaciones del docente consultado.
[ ] TotalHorasSemanales y TotalAsignaturas correctos.
[ ] PATCH /api/asignaciones/{id}/asignar-dia guarda día correctamente.
[ ] Con día y hora, la asignación aparece en el calendario.
[ ] Sin hora, no aparece en el calendario pero sí cuenta en TotalAsignaturas.
[ ] Rechaza si solo se envía una de las dos horas.
[ ] Rechaza si horaInicio >= horaFin.
[ ] Rechaza si la asignación está Confirmada.
[ ] Retorna 404 si el docente no existe.

# General
[ ] No se sube horarios.db ni bin/ ni obj/.
[ ] README actualizado.
[ ] PR apunta a develop.
[ ] Issues enlazados al PR.
```