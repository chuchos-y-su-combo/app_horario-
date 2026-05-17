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

---

## Objetivo del sistema

El objetivo del sistema es apoyar la gestión académica necesaria para la construcción de horarios universitarios.

El sistema permite:

- Gestionar usuarios administradores y coordinadores.
- Registrar docentes.
- Registrar asignaturas.
- Asociar asignaturas a planes de estudio.
- Controlar la carga docente según tipo de contrato.
- Importar currículo docente desde Excel.
- Importar disponibilidad docente desde el Excel actual de coordinación.
- Marcar materias TAPSI obligatorias como fijas.
- Contemplar una materia adicional para estudiantes TAPSI de jornada diurna.
- Registrar asignaciones académicas.
- Consultar disponibilidad y asignaturas habilitadas por docente.
- Asignar asignaturas a docentes de forma manual, con validación de currículo y límite de carga.
- Consultar las asignaturas disponibles para asignar a un docente en un semestre determinado.
- Reducir automáticamente la disponibilidad de un docente que dicta la misma asignatura en jornada diurna y nocturna.
- Revisar, ajustar y confirmar manualmente las propuestas de asignación generadas.
- Generar reporte de horas de clase asignadas vs carga contractual por docente.
- Detectar y alertar conflictos en las asignaciones del semestre.

---

## Base de datos

El proyecto utiliza SQLite.

La base de datos fue adaptada desde el modelo oficial inicialmente planteado para MySQL Workbench.

### Adaptaciones realizadas para SQLite

| MySQL original | SQLite |
|---|---|
| `CREATE DATABASE` | No se usa |
| `USE gestion_horarios` | No se usa |
| `CHAR(36)` | `TEXT` |
| `VARCHAR` | `TEXT` |
| `ENUM` | `TEXT CHECK (...)` |
| `TINYINT(1)` | `INTEGER CHECK (0, 1)` |
| `TIME` | `TEXT` con formato `HH:mm` |
| `AUTO_INCREMENT` | `INTEGER PRIMARY KEY AUTOINCREMENT` |

---

## Tablas principales del modelo

```txt
roles
usuarios
planes_estudio
docentes
asignaturas
docentes_habilitados
disponibilidad
asignaciones
bloqueos
```

---

## Estructura del proyecto

```txt
src/
├── ApplicationSchedule.Api
│   └── Controllers
├── ApplicationSchedule.Application
│   ├── DTOs
│   └── Interfaces
├── ApplicationSchedule.Domain
│   └── Entities
├── ApplicationSchedule.Infrastructure
│   ├── Data
│   └── Services
└── ApplicationSchedule.Tests
    ├── Controllers
    └── Infrastructure

database/
docs/
README.md
.gitignore
```

---

## Capas del proyecto

### ApplicationSchedule.Api

Contiene los controladores de la API, configuración principal, Swagger y punto de entrada del backend.

### ApplicationSchedule.Application

Contiene DTOs e interfaces. En esta capa se definen los contratos que luego implementa la infraestructura.

### ApplicationSchedule.Domain

Contiene las entidades principales del sistema:

```txt
Usuario, Rol, Docente, Asignatura, Disponibilidad, Asignacion, PlanEstudio, DocenteHabilitado
```

### ApplicationSchedule.Infrastructure

Contiene la conexión con SQLite, configuración de Entity Framework Core y servicios que acceden a la base de datos.

### ApplicationSchedule.Tests

Contiene pruebas automáticas del sistema.

---

## Configuración de la base de datos

La cadena de conexión se encuentra en:

```txt
src/ApplicationSchedule.Api/appsettings.json
```

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=horarios.db"
  }
}
```

La base se crea automáticamente al ejecutar la API mediante `context.Database.EnsureCreated()`.

---

## Importante sobre `horarios.db`

El archivo `horarios.db` es local y no debe subirse al repositorio.

`.gitignore` recomendado:

```gitignore
*.db
*.db-shm
*.db-wal
*.sqlite
src/ApplicationSchedule.Api/horarios.db
bin/
obj/
.vs/
.idea/
*.user
*.dll
*.pdb
```

Si se cambia la estructura de la base de datos en desarrollo, se puede borrar `horarios.db` para que se regenere. Si ya contiene información importante, ejecutar el script SQL de actualización correspondiente.

---

## Datos base

El sistema crea automáticamente los roles base:

| ID | Rol |
|---|---|
| 1 | Administrador |
| 2 | Coordinador |

Y dos planes de estudio base:

| ID | Nombre | Jornada |
|---|---|---|
| `11111111-1111-1111-1111-111111111111` | Plan de Estudios 1020 Jornada Diurna | Diurna |
| `22222222-2222-2222-2222-222222222222` | Plan de Estudios Jornada Noche | Nocturna |

---

# Requerimiento 2: usuarios con rol administrador o coordinador

El sistema permite crear y gestionar cuentas de usuario con rol Administrador o Coordinador.

## Endpoints

```http
GET    /api/usuarios
POST   /api/usuarios
GET    /api/usuarios/{idUsuario}
PUT    /api/usuarios/{idUsuario}
PATCH  /api/usuarios/{idUsuario}/password
DELETE /api/usuarios/{idUsuario}
```

## Validaciones

- El correo debe ser único.
- El rol debe existir (1 = Administrador, 2 = Coordinador).
- La contraseña se almacena como hash usando BCrypt.
- El correo se normaliza a minúsculas.

---

# Requerimiento 3: registro de docentes

El sistema permite registrar docentes con nombre, identificación y tipo de contrato.

| Entrada permitida | Valor guardado |
|---|---|
| `TC` o `Tiempo Completo` | `TC` |
| `TP` o `Parcial` | `TP` |

## Endpoints

```http
GET    /api/profesores
POST   /api/profesores
GET    /api/profesores/{idProfesor}
PUT    /api/profesores/{idProfesor}
DELETE /api/profesores/{idProfesor}
```

## Crear docente

```json
{
  "nombre": "Carlos Pérez",
  "identificacion": "1001",
  "tipoContrato": "TC"
}
```

Respuesta: `"maxAsignaturas": 5` para TC, `3` para TP.

## Validaciones

- La identificación debe ser única.
- El máximo de asignaturas se calcula automáticamente según el contrato.
- No se puede eliminar un docente con asignaciones registradas.

---

# Requerimiento 4: registro de asignaturas

El sistema permite registrar asignaturas con nombre, código, créditos, semestre y plan de estudios.

## Endpoints

```http
GET    /api/asignaturas
POST   /api/asignaturas
GET    /api/asignaturas/{idAsignatura}
GET    /api/asignaturas/plan/{idPlan}
PUT    /api/asignaturas/{idAsignatura}
DELETE /api/asignaturas/{idAsignatura}
```

## Crear asignatura

```json
{
  "idPlan": "11111111-1111-1111-1111-111111111111",
  "codigo": "MAT001",
  "nombre": "Cálculo I",
  "creditos": 4,
  "semestre": 1,
  "minEstudiantes": 15,
  "esFijaTapsi": false,
  "esOpcionalTapsiDiurna": false
}
```

## Validaciones

- El plan de estudios debe existir.
- El código debe ser único.
- Créditos entre 1 y 20. Semestre entre 1 y 12.

---

# Requerimiento 5: límite de carga docente

El sistema limita la carga docente según el tipo de contrato.

| Tipo de contrato | Máximo de asignaturas |
|---|---|
| `TC` | 5 |
| `TP` | 3 |

El sistema cuenta asignaturas **distintas**, no bloques horarios.

## Endpoints

```http
GET    /api/asignaciones
POST   /api/asignaciones
GET    /api/asignaciones/docente/{idDocente}
GET    /api/asignaciones/docente/{idDocente}/resumen?periodo=2026-1
DELETE /api/asignaciones/{idAsignacion}
```

## Crear asignación con bloque horario

```json
{
  "idDocente": "ID_DEL_DOCENTE",
  "idAsignatura": "ID_DE_LA_ASIGNATURA",
  "dia": 1,
  "horaInicio": "08:00",
  "horaFin": "10:00",
  "periodo": "2026-1"
}
```

---

# Requerimiento 6: carga de currículo docente desde Excel

El sistema permite importar desde Excel las asignaturas que cada docente está habilitado para dictar.

## Endpoints

```http
POST /api/profesores/importar-excel
GET  /api/profesores/{idProfesor}/asignaturas-habilitadas
```

---

# Importación de disponibilidad docente desde Excel

El sistema importa la disponibilidad docente desde el archivo Excel actual de coordinación.

Equivalencia de días: 1 = Lunes, 2 = Martes, 3 = Miércoles, 4 = Jueves, 5 = Viernes, 6 = Sábado.

## Endpoints

```http
POST /api/profesores/importar-excel
GET  /api/profesores/{idProfesor}/disponibilidad
```

---

# Requerimiento 7: materias TAPSI fijas

El sistema marca automáticamente como fijas las 5 materias obligatorias TAPSI:

| Código | Asignatura |
|---|---|
| 104030 | Cálculo Diferencial |
| 103007 | Técnicas de Programación |
| 103018 | Programación Orientada a Objetos |
| 103004 | Teoría de Sistemas |
| 103027 | Sistemas Operativos |

## Endpoints

```http
GET  /api/asignaturas/tapsi/fijas
POST /api/asignaturas/tapsi/marcar-fijas
```

---

# Requerimiento 8: TAPSI jornada diurna

Para jornada diurna, los estudiantes TAPSI deben añadir una asignatura entre:

| Código | Asignatura |
|---|---|
| 103093 | Ingeniería de Software II |
| 103126 | Redes LAN |
| 109183 | Programación Back End |

| Jornada | Tope de créditos |
|---|---|
| Diurna | 18 |
| Extendida / Nocturna | 15 |

## Endpoints

```http
GET  /api/asignaturas/tapsi/diurna/opciones-adicionales
GET  /api/asignaturas/tapsi/diurna/plan
POST /api/asignaturas/tapsi/diurna/marcar-opciones-adicionales
```

---

# Requerimiento 9: generación automática de propuestas

El sistema genera propuestas de asignación para los cuatro escenarios:

- Ingeniería diurna (`ING_DIURNA`)
- Ingeniería nocturna (`ING_NOCTURNA`)
- TAPSI diurna (`TAPSI_DIURNA`)
- TAPSI nocturna (`TAPSI_NOCTURNA`)

Las propuestas se almacenan en `asignaciones` con estado `Propuesta`.

## Endpoint

```http
POST /api/horarios/generar-propuestas
```

---

# Issue #10: asignación manual de asignaturas a docentes

## Descripción

El coordinador puede asignar manualmente una asignatura a un docente para un semestre, sin necesidad de definir el bloque horario en ese momento.

## Reglas de negocio

- El docente debe existir.
- La asignatura debe existir.
- Se respeta el límite de carga según contrato (TC: 5, TP: 3).
- Si el docente tiene currículo cargado, se valida que esté habilitado para esa asignatura.
  - `ForzarSinCurriculo = true` permite omitir esta validación de forma excepcional.
- No se permite asignar la misma asignatura al mismo docente dos veces en el mismo semestre.
- La asignación queda con estado `AsignadaManual`, `Dia = 0`, `HoraInicio = ""`, `HoraFin = ""`.

## Endpoints

```http
POST /api/asignaciones/manual
GET  /api/asignaciones/docente/{idDocente}/asignaturas-disponibles?periodo=2026-1
```

### POST /api/asignaciones/manual

Body:

```json
{
  "idDocente": "uuid-del-docente",
  "idAsignatura": "uuid-de-la-asignatura",
  "periodo": "2026-1",
  "forzarSinCurriculo": false
}
```

Respuesta exitosa (201 Created):

```json
{
  "idAsignacion": "...",
  "nombreDocente": "Juan Pérez",
  "tipoContrato": "TC",
  "maxAsignaturas": 5,
  "asignaturasActuales": 1,
  "nombreAsignatura": "Cálculo I",
  "dia": 0,
  "horaInicio": "",
  "horaFin": "",
  "periodo": "2026-1",
  "estado": "AsignadaManual"
}
```

Errores posibles (400): docente no encontrado, asignatura no encontrada, contrato no válido, no habilitado por currículo, límite de carga alcanzado, asignatura ya asignada en el semestre.

### GET /api/asignaciones/docente/{idDocente}/asignaturas-disponibles

Devuelve todas las asignaturas del sistema con dos indicadores:

- `habilitadaPorCurriculo`: si el docente está habilitado por currículo para esa materia.
- `yaAsignadaEnPeriodo`: si ya tiene esa materia asignada en el semestre consultado.

## Flujo de uso

```
1. GET  /api/asignaciones/docente/{id}/asignaturas-disponibles?periodo=2026-1
2. POST /api/asignaciones/manual  → queda con estado "AsignadaManual"
3. (Opcional) Completar el bloque horario luego desde el flujo del Issue #12.
```

---

# Issue #11: reducción de disponibilidad por doble jornada

## Descripción

El sistema detecta cuando un docente dicta la misma asignatura en jornada diurna y nocturna en el mismo semestre, y elimina automáticamente los bloques de disponibilidad que se solapan con la jornada diurna ya asignada.

## Reglas de negocio

- Si el docente no tiene asignaciones en ambas jornadas para esa asignatura en el semestre, no se realiza ninguna reducción.
- Se eliminan los bloques de disponibilidad que coinciden en día y se solapan en horario con algún bloque de la jornada diurna.
- Jornadas diurnas: `ING_DIURNA`, `TAPSI_DIURNA`. Jornadas nocturnas: `ING_NOCTURNA`, `TAPSI_NOCTURNA`.

## Endpoint

```http
POST /api/curriculos-docentes/{idDocente}/reducir-disponibilidad?idAsignatura={id}&periodo=2026-1
```

Respuesta (200 OK):

```json
{
  "idDocente": "...",
  "nombreDocente": "Carlos Pérez",
  "nombreAsignatura": "Cálculo I",
  "periodo": "2026-1",
  "bloquesEliminados": 1,
  "bloquesAfectados": ["Día 1 08:00-10:00"],
  "mensaje": "Se eliminaron 1 bloque(s) de disponibilidad por doble jornada."
}
```

---

# Issue #12: revisión y ajuste manual de propuestas antes de confirmar

## Descripción

El coordinador puede revisar las propuestas generadas, ajustar cualquier campo antes de confirmarlas, cancelar las que no deben incluirse y confirmarlas individualmente o en bloque.

## Estados posibles de una asignación

| Estado | Origen | Puede ajustarse | Puede confirmarse | Puede cancelarse |
|--------|---------|:-:|:-:|:-:|
| `Propuesta` | Generación automática | ✅ | ✅ | ✅ |
| `AsignadaManual` | Asignación manual | ✅ | ✅ | ✅ |
| `Confirmada` | Confirmación por coordinador | ❌ | ❌ | ❌ |
| `Cancelada` | Cancelación | ❌ | ❌ | ❌ |

## Endpoints

```http
GET   /api/asignaciones/propuestas?periodo=2026-1
PATCH /api/asignaciones/{idAsignacion}/ajustar
POST  /api/asignaciones/confirmar
PATCH /api/asignaciones/{idAsignacion}/cancelar
```

### PATCH /api/asignaciones/{idAsignacion}/ajustar

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
  "errores": [
    { "idAsignacion": "uuid-3", "motivo": "No se puede confirmar una asignación cancelada." }
  ]
}
```

## Flujo de uso completo

```
1. POST /api/horarios/generar-propuestas
2. GET  /api/asignaciones/propuestas?periodo=2026-1
3. PATCH /api/asignaciones/{id}/ajustar         (corregir lo que sea necesario)
4. PATCH /api/asignaciones/{id}/cancelar         (descartar las que no aplican)
5. POST  /api/asignaciones/confirmar             (aprobar todo en bloque)
```

---

# Issue #16: reporte de horas asignadas vs carga contractual

## Descripción

El sistema genera un reporte que compara las horas de clase asignadas a cada docente contra su carga contractual, para que el coordinador pueda identificar docentes con carga incompleta, completa o excedida.

## Lógica de cálculo

Las horas se calculan sumando la diferencia entre `HoraFin` y `HoraInicio` de cada bloque de asignación activo (`Propuesta`, `AsignadaManual`, `Confirmada`).

Las asignaciones sin bloque horario definido (creadas con el flujo manual del Issue #10) suman 0 horas pero sí cuentan como asignatura asignada.

## Horas contractuales de referencia

| Tipo de contrato | Horas semanales de referencia |
|---|---|
| `TC` | 40 horas |
| `TP` | 20 horas |

## Clasificación del estado de carga

| Condición | EstadoCarga |
|---|---|
| Sin asignaturas en el semestre | `Sin asignaciones` |
| Horas asignadas > horas contractuales | `Excedida` |
| Horas asignadas ≥ 90% de las contractuales | `Completa` |
| Horas asignadas < 90% de las contractuales | `Parcial` |

## Endpoints

```http
GET /api/reportes/carga-docente?semestre=2026-1
GET /api/reportes/carga-docente/{idDocente}?semestre=2026-1
```

### GET /api/reportes/carga-docente

Devuelve el reporte completo con todos los docentes del semestre y un resumen de conteos.

Respuesta (200 OK):

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
      "idDocente": "...",
      "nombreDocente": "Carlos Pérez",
      "identificacion": "1001",
      "tipoContrato": "TC",
      "maxAsignaturas": 5,
      "asignaturasAsignadas": 3,
      "totalHorasSemanales": 6.0,
      "horasContractuales": 40.0,
      "diferenciaHoras": -34.0,
      "porcentajeCarga": 15.0,
      "estadoCarga": "Parcial",
      "semestre": "2026-1",
      "asignaturas": [
        {
          "nombreAsignatura": "Cálculo I",
          "codigoAsignatura": "MAT001",
          "escenario": "ING_DIURNA",
          "estado": "Confirmada",
          "horasSemanales": 2.0
        }
      ]
    }
  ]
}
```

### GET /api/reportes/carga-docente/{idDocente}

Devuelve el reporte individual de un docente con el detalle por asignatura.

Errores posibles (404): docente no encontrado.

## Flujo de uso recomendado

```
1. Al final del proceso de asignación, ejecutar:
   GET /api/reportes/carga-docente?semestre=2026-1

2. Identificar docentes con EstadoCarga "Parcial" o "Sin asignaciones"
   y completar su carga con el flujo del Issue #10 o #12.

3. Para ver el detalle de un docente específico:
   GET /api/reportes/carga-docente/{idDocente}?semestre=2026-1
```

---

# Issue #18: alertas de conflictos en asignaciones

## Descripción

El sistema analiza todas las asignaciones activas de un semestre y detecta cuatro tipos de conflictos, clasificados por severidad, para que el coordinador pueda corregirlos antes de confirmar el horario.

## Tipos de conflicto detectados

| Tipo | Severidad | Descripción |
|---|---|---|
| `CruceHorario` | Error | Dos asignaciones del mismo docente se solapan en día y hora |
| `ExcesoCarga` | Error | El docente supera el número máximo de asignaturas de su contrato |
| `AsignaturaSinDocente` | Advertencia | Una asignatura no tiene ningún docente asignado en el semestre |
| `DocenteSinHorario` | Advertencia | Una asignatura asignada no tiene bloque horario definido aún |

Los estados analizados son: `Propuesta`, `AsignadaManual` y `Confirmada`. Las asignaciones `Cancelada` se ignoran.

## Endpoint

```http
GET /api/reportes/conflictos?semestre=2026-1
```

Respuesta sin conflictos (200 OK):

```json
{
  "semestre": "2026-1",
  "totalConflictos": 0,
  "tieneConflictos": false,
  "conflictos": []
}
```

Respuesta con conflictos (200 OK):

```json
{
  "semestre": "2026-1",
  "totalConflictos": 3,
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
    },
    {
      "tipoConflicto": "ExcesoCarga",
      "severidad": "Error",
      "descripcion": "El docente 'Ana Gómez' (TP) tiene 4 asignaturas asignadas, superando su límite de 3.",
      "idDocente": "...",
      "nombreDocente": "Ana Gómez"
    },
    {
      "tipoConflicto": "AsignaturaSinDocente",
      "severidad": "Advertencia",
      "descripcion": "La asignatura 'Física I' (FIS001) no tiene ningún docente asignado en el semestre 2026-1.",
      "nombreAsignatura": "Física I"
    },
    {
      "tipoConflicto": "DocenteSinHorario",
      "severidad": "Advertencia",
      "descripcion": "El docente 'Luis Torres' tiene la asignatura 'Programación I' sin bloque horario definido (estado: AsignadaManual).",
      "idDocente": "...",
      "nombreDocente": "Luis Torres",
      "idAsignacion1": "...",
      "nombreAsignatura": "Programación I"
    }
  ]
}
```

Errores posibles (400): falta el parámetro `semestre`.

## Flujo de uso recomendado

```
1. Después de generar o ajustar propuestas, ejecutar:
   GET /api/reportes/conflictos?semestre=2026-1

2. Revisar los conflictos de severidad "Error" primero:
   - CruceHorario  → ajustar el bloque horario con PATCH /api/asignaciones/{id}/ajustar
   - ExcesoCarga   → eliminar una asignación con DELETE /api/asignaciones/{id}

3. Revisar las advertencias:
   - AsignaturaSinDocente → asignar un docente con POST /api/asignaciones/manual
   - DocenteSinHorario    → completar el horario con PATCH /api/asignaciones/{id}/ajustar

4. Repetir el análisis hasta que totalConflictos sea 0.

5. Confirmar las propuestas limpias:
   POST /api/asignaciones/confirmar
```

---

# Cambios de base de datos acumulados

## Estado del campo `estado` en la tabla `asignaciones`

```sql
estado TEXT NOT NULL CHECK(estado IN ('Propuesta', 'Confirmada', 'Cancelada', 'AsignadaManual'))
```

## Columna agregada a `asignaturas` (Req 8)

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

# Endpoints principales

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
```

## Asignaturas

```http
GET    /api/asignaturas
POST   /api/asignaturas
GET    /api/asignaturas/{idAsignatura}
GET    /api/asignaturas/plan/{idPlan}
PUT    /api/asignaturas/{idAsignatura}
DELETE /api/asignaturas/{idAsignatura}
```

## Asignaciones

```http
GET    /api/asignaciones
POST   /api/asignaciones
GET    /api/asignaciones/docente/{idDocente}
GET    /api/asignaciones/docente/{idDocente}/resumen?periodo=2026-1
DELETE /api/asignaciones/{idAsignacion}

# Issue #10 — Asignación manual
POST /api/asignaciones/manual
GET  /api/asignaciones/docente/{idDocente}/asignaturas-disponibles?periodo=2026-1

# Issue #12 — Revisión y confirmación
GET   /api/asignaciones/propuestas?periodo=2026-1
PATCH /api/asignaciones/{idAsignacion}/ajustar
POST  /api/asignaciones/confirmar
PATCH /api/asignaciones/{idAsignacion}/cancelar
```

## Excel, currículo y disponibilidad

```http
POST /api/profesores/importar-excel
GET  /api/profesores/{idProfesor}/asignaturas-habilitadas
GET  /api/profesores/{idProfesor}/disponibilidad

# Issue #11 — Reducción de disponibilidad por doble jornada
POST /api/curriculos-docentes/{idDocente}/reducir-disponibilidad?idAsignatura={id}&periodo=2026-1
```

## TAPSI

```http
GET  /api/asignaturas/tapsi/fijas
POST /api/asignaturas/tapsi/marcar-fijas
GET  /api/asignaturas/tapsi/diurna/opciones-adicionales
GET  /api/asignaturas/tapsi/diurna/plan
POST /api/asignaturas/tapsi/diurna/marcar-opciones-adicionales
```

## Horarios

```http
POST /api/horarios/generar-propuestas
```

## Reportes (Issues #16 y #18)

```http
# Issue #16 — Carga docente
GET /api/reportes/carga-docente?semestre=2026-1
GET /api/reportes/carga-docente/{idDocente}?semestre=2026-1

# Issue #18 — Conflictos
GET /api/reportes/conflictos?semestre=2026-1
```

---

# Cómo ejecutar el proyecto

Desde la raíz del proyecto:

```powershell
dotnet restore .\src\ApplicationSchedule.Api\ApplicationSchedule.Api.csproj
dotnet build   .\src\ApplicationSchedule.Api\ApplicationSchedule.Api.csproj
dotnet run --project .\src\ApplicationSchedule.Api\ApplicationSchedule.Api.csproj
```

La API queda disponible en `http://localhost:5213`.
Swagger queda disponible en `http://localhost:5213/swagger`.

---

# Cómo ejecutar las pruebas

```powershell
dotnet build .\src\ApplicationSchedule.Api\ApplicationSchedule.Api.csproj
dotnet build .\src\ApplicationSchedule.Tests\ApplicationSchedule.Tests.csproj
dotnet test  .\src\ApplicationSchedule.Tests\ApplicationSchedule.Tests.csproj
```

Con detalle:

```powershell
dotnet test .\src\ApplicationSchedule.Tests\ApplicationSchedule.Tests.csproj --logger "console;verbosity=detailed"
```

---

# Guía de pruebas manuales en Swagger

## Paso 1 — Crear un docente

```http
POST /api/profesores
```

```json
{
  "nombre": "Carlos Pérez",
  "identificacion": "1001",
  "tipoContrato": "TC"
}
```

Guardar el `idProfesor` de la respuesta.

## Paso 2 — Crear asignaturas

```http
POST /api/asignaturas
```

```json
{
  "idPlan": "11111111-1111-1111-1111-111111111111",
  "codigo": "MAT001",
  "nombre": "Cálculo I",
  "creditos": 4,
  "semestre": 1
}
```

Guardar el `idAsignatura` de la respuesta.

## Paso 3 — Ver asignaturas disponibles para el docente (Issue #10)

```http
GET /api/asignaciones/docente/{idDocente}/asignaturas-disponibles?periodo=2026-1
```

## Paso 4 — Asignar una asignatura manualmente (Issue #10)

```http
POST /api/asignaciones/manual
```

```json
{
  "idDocente": "ID_DEL_DOCENTE",
  "idAsignatura": "ID_DE_LA_ASIGNATURA",
  "periodo": "2026-1",
  "forzarSinCurriculo": false
}
```

La respuesta debe tener `"estado": "AsignadaManual"`, `"dia": 0`, `"horaInicio": ""`.

## Paso 5 — Consultar propuestas pendientes (Issue #12)

```http
GET /api/asignaciones/propuestas?periodo=2026-1
```

## Paso 6 — Ajustar una propuesta (Issue #12)

```http
PATCH /api/asignaciones/{idAsignacion}/ajustar
```

```json
{
  "dia": 3,
  "horaInicio": "10:00",
  "horaFin": "12:00"
}
```

## Paso 7 — Detectar conflictos antes de confirmar (Issue #18)

```http
GET /api/reportes/conflictos?semestre=2026-1
```

Revisar y corregir todos los conflictos de severidad `Error` antes de continuar.

## Paso 8 — Cancelar propuestas que no aplican (Issue #12)

```http
PATCH /api/asignaciones/{idAsignacion}/cancelar
```

## Paso 9 — Confirmar propuestas aprobadas en bloque (Issue #12)

```http
POST /api/asignaciones/confirmar
```

```json
{
  "idsAsignacion": ["uuid-1", "uuid-2", "uuid-3"]
}
```

## Paso 10 — Generar el reporte de carga docente (Issue #16)

```http
GET /api/reportes/carga-docente?semestre=2026-1
```

Identifica docentes con carga `Parcial` o `Sin asignaciones` para completar su carga.

## Paso 11 — Reducir disponibilidad por doble jornada (Issue #11)

```http
POST /api/curriculos-docentes/{idDocente}/reducir-disponibilidad?idAsignatura={idAsignatura}&periodo=2026-1
```

## Paso 12 — Volver a verificar conflictos (Issue #18)

```http
GET /api/reportes/conflictos?semestre=2026-1
```

Repetir hasta que `totalConflictos` sea 0 y luego confirmar.

---

# Git Flow recomendado

## Ramas por issue

| Issue | Rama sugerida |
|---|---|
| Issue #10 | `feature/asignacion-manual-docente` |
| Issue #11 | `feature/reduccion-disponibilidad-doble-jornada` |
| Issue #12 | `feature/revision-ajuste-confirmacion-propuestas` |
| Issue #16 | `feature/reporte-carga-docente` |
| Issue #18 | `feature/alertas-conflictos-asignaciones` |

## Crear rama desde develop

```bash
git checkout develop
git pull origin develop
git checkout -b feature/alertas-conflictos-asignaciones
```

## Guardar y publicar cambios

```bash
git add .
git commit -m "feat(reportes): alertas de conflictos en asignaciones"
git push origin feature/alertas-conflictos-asignaciones
```

---

# Alcance actual

## Implementado

- Gestión de usuarios y roles.
- Registro de docentes con límite de carga por contrato.
- Registro de asignaturas con planes de estudio.
- Materias TAPSI fijas y opciones adicionales para jornada diurna.
- Importación de currículo docente desde Excel.
- Importación de disponibilidad docente desde Excel.
- Generación automática de propuestas para 4 escenarios (Req 9).
- Pruebas automáticas.
- **Issue #10:** Asignación manual de asignaturas a docentes.
- **Issue #10:** Consulta de asignaturas disponibles para un docente por semestre.
- **Issue #11:** Reducción de disponibilidad por doble jornada.
- **Issue #12:** Consulta de propuestas pendientes por semestre.
- **Issue #12:** Ajuste manual de propuestas campo por campo.
- **Issue #12:** Cancelación de propuestas.
- **Issue #12:** Confirmación individual y en bloque de propuestas.
- **Issue #16:** Reporte de horas asignadas vs carga contractual por docente.
- **Issue #16:** Reporte individual por docente con detalle de asignaturas.
- **Issue #18:** Detección de cruces horarios entre asignaciones del mismo docente.
- **Issue #18:** Detección de exceso de carga por contrato.
- **Issue #18:** Detección de asignaturas sin docente asignado en el semestre.
- **Issue #18:** Detección de docentes con asignaturas sin bloque horario definido.

## Pendiente o futuro

- Validación automática de conflictos al momento de crear asignaciones.
- Interfaz visual del frontend.
- Normalización del formato de Excel de disponibilidad.
- Reglas avanzadas de selección automática de la materia adicional TAPSI diurna.

---

# Checklist antes de Pull Request

```txt
[ ] La API compila correctamente (dotnet build).
[ ] Los tests compilan correctamente.
[ ] Los tests pasan (dotnet test).
[ ] Swagger abre correctamente en http://localhost:5213/swagger.

# Issue #10
[ ] POST /api/asignaciones/manual crea con estado "AsignadaManual".
[ ] GET  /api/asignaciones/docente/{id}/asignaturas-disponibles responde correctamente.
[ ] El límite de carga se respeta en asignaciones manuales.
[ ] El duplicado en el mismo semestre es rechazado con 400.

# Issue #11
[ ] POST /api/curriculos-docentes/{id}/reducir-disponibilidad funciona.
[ ] Solo elimina bloques cuando hay asignaciones en ambas jornadas.
[ ] Si no hay doble jornada, responde con bloquesEliminados: 0.

# Issue #12
[ ] GET  /api/asignaciones/propuestas devuelve solo Propuesta y AsignadaManual.
[ ] PATCH /api/asignaciones/{id}/ajustar actualiza solo los campos enviados.
[ ] PATCH /api/asignaciones/{id}/ajustar rechaza con 400 si ya está Confirmada.
[ ] PATCH /api/asignaciones/{id}/cancelar cambia estado a Cancelada.
[ ] PATCH /api/asignaciones/{id}/cancelar rechaza con 400 si ya está Confirmada.
[ ] POST  /api/asignaciones/confirmar confirma en bloque correctamente.
[ ] POST  /api/asignaciones/confirmar reporta fallidas sin interrumpir las exitosas.

# Issue #16
[ ] GET /api/reportes/carga-docente?semestre=2026-1 retorna todos los docentes.
[ ] GET /api/reportes/carga-docente/{id}?semestre=2026-1 retorna el docente correcto.
[ ] Las horas se calculan correctamente con los bloques horarios.
[ ] Las asignaciones sin horario suman 0 horas pero cuentan como asignatura asignada.
[ ] El EstadoCarga clasifica correctamente (Sin asignaciones / Parcial / Completa / Excedida).
[ ] Retorna 400 si falta el semestre.
[ ] Retorna 404 si el docente no existe.

# Issue #18
[ ] GET /api/reportes/conflictos?semestre=2026-1 detecta cruces horarios.
[ ] Detecta exceso de carga por contrato.
[ ] Detecta asignaturas sin docente en el semestre.
[ ] Detecta docentes con asignaturas sin bloque horario.
[ ] Bloques en días distintos no generan cruce.
[ ] Retorna 400 si falta el semestre.
[ ] tieneConflictos es false cuando no hay conflictos.

# General
[ ] No se sube horarios.db.
[ ] No se suben archivos bin/ ni obj/.
[ ] README actualizado.
[ ] El PR apunta hacia develop.
[ ] Los issues quedan enlazados al PR.
```