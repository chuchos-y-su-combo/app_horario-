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
- Consultar las asignaturas disponibles para asignar a un docente en un periodo determinado.
- Reducir automáticamente la disponibilidad de un docente que dicta la misma asignatura en jornada diurna y nocturna.
- Revisar, ajustar y confirmar manualmente las propuestas de asignación generadas.

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

El coordinador puede asignar manualmente una asignatura a un docente para un periodo, sin necesidad de definir el bloque horario en ese momento.

## Reglas de negocio

- El docente debe existir.
- La asignatura debe existir.
- Se respeta el límite de carga según contrato (TC: 5, TP: 3).
- Si el docente tiene currículo cargado, se valida que esté habilitado para esa asignatura.
  - `ForzarSinCurriculo = true` permite omitir esta validación de forma excepcional.
- No se permite asignar la misma asignatura al mismo docente dos veces en el mismo periodo.
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

Errores posibles (400):

- Docente no encontrado.
- Asignatura no encontrada.
- Contrato del docente no válido.
- Docente no habilitado por currículo para la asignatura.
- Docente ya alcanzó el límite de asignaturas para el periodo.
- La asignatura ya está asignada al docente en el periodo.

### GET /api/asignaciones/docente/{idDocente}/asignaturas-disponibles

Devuelve todas las asignaturas del sistema con dos indicadores:

- `habilitadaPorCurriculo`: si el docente está habilitado por currículo para esa materia.
- `yaAsignadaEnPeriodo`: si ya tiene esa materia asignada en el periodo consultado.

Respuesta (200 OK):

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
  }
]
```

## Flujo de uso

```
1. GET  /api/asignaciones/docente/{id}/asignaturas-disponibles?periodo=2026-1
   → Ver qué asignaturas puede recibir el docente y cuáles ya tiene.

2. POST /api/asignaciones/manual
   → Registrar la asignación. Queda con estado "AsignadaManual".

3. (Opcional) Completar el bloque horario luego desde el flujo del Issue #12.
```

---

# Issue #11: reducción de disponibilidad por doble jornada

## Descripción

El sistema detecta cuando un docente dicta la misma asignatura en jornada diurna y nocturna en el mismo periodo, y elimina automáticamente los bloques de disponibilidad que se solapan con la jornada diurna ya asignada.

## Reglas de negocio

- El docente debe existir.
- La asignatura debe existir.
- Si el docente no tiene asignaciones en ambas jornadas para esa asignatura en el periodo, no se realiza ninguna reducción.
- Se eliminan los bloques de disponibilidad que coinciden en día y se solapan en horario con algún bloque de la jornada diurna.

Jornadas diurnas reconocidas: `ING_DIURNA`, `TAPSI_DIURNA`.
Jornadas nocturnas reconocidas: `ING_NOCTURNA`, `TAPSI_NOCTURNA`.

## Endpoint

```http
POST /api/curriculos-docentes/{idDocente}/reducir-disponibilidad?idAsignatura={id}&periodo=2026-1
```

Respuesta cuando hay reducción (200 OK):

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

Respuesta cuando no hay reducción necesaria (200 OK):

```json
{
  "bloquesEliminados": 0,
  "bloquesAfectados": [],
  "mensaje": "El docente no dicta esta asignatura en ambas jornadas. No se realizó ninguna reducción."
}
```

## Flujo de uso

```
1. Verificar si un docente tiene la misma asignatura en jornada diurna y nocturna.

2. POST /api/curriculos-docentes/{id}/reducir-disponibilidad?idAsignatura={id}&periodo=2026-1
   → El sistema elimina los bloques de disponibilidad solapados con la jornada diurna.

3. La respuesta indica exactamente cuántos bloques fueron eliminados y cuáles.
```

---

# Issue #12: revisión y ajuste manual de propuestas antes de confirmar

## Descripción

El coordinador puede revisar las propuestas generadas automáticamente o creadas manualmente, ajustar cualquier campo antes de confirmarlas, cancelar las que no deben incluirse, y confirmarlas individualmente o en bloque.

## Estados posibles de una asignación

| Estado | Origen | Puede ajustarse | Puede confirmarse | Puede cancelarse |
|--------|---------|:-:|:-:|:-:|
| `Propuesta` | Generación automática (Req 9) | ✅ | ✅ | ✅ |
| `AsignadaManual` | Asignación manual (Issue #10) | ✅ | ✅ | ✅ |
| `Confirmada` | Confirmación por coordinador | ❌ | ❌ | ❌ |
| `Cancelada` | Cancelación por coordinador | ❌ | ❌ | ❌ |

## Reglas de negocio

- Solo se pueden ajustar asignaciones en estado `Propuesta` o `AsignadaManual`.
- Solo se pueden confirmar asignaciones en estado `Propuesta` o `AsignadaManual`.
- Solo se pueden cancelar asignaciones en estado `Propuesta` o `AsignadaManual`.
- Una asignación `Confirmada` no puede ajustarse ni cancelarse; debe eliminarse si es necesario.
- Al cambiar el docente, se valida que el nuevo docente no supere su límite de carga.
- Al cambiar las horas, se valida que `HoraInicio` < `HoraFin`.
- La confirmación en bloque reporta por separado las exitosas y las fallidas sin interrumpir el proceso.

## Endpoints

```http
GET   /api/asignaciones/propuestas?periodo=2026-1
PATCH /api/asignaciones/{idAsignacion}/ajustar
POST  /api/asignaciones/confirmar
PATCH /api/asignaciones/{idAsignacion}/cancelar
```

### GET /api/asignaciones/propuestas

Devuelve todas las asignaciones en estado `Propuesta` o `AsignadaManual` del periodo, ordenadas por escenario, docente, día y hora.

Respuesta (200 OK):

```json
[
  {
    "idAsignacion": "...",
    "nombreDocente": "Carlos Pérez",
    "tipoContrato": "TC",
    "nombreAsignatura": "Cálculo I",
    "dia": 1,
    "horaInicio": "08:00",
    "horaFin": "10:00",
    "periodo": "2026-1",
    "estado": "Propuesta"
  }
]
```

### PATCH /api/asignaciones/{idAsignacion}/ajustar

Todos los campos son opcionales. Solo se actualiza lo que se envíe.

Body:

```json
{
  "idDocente": "nuevo-uuid-docente",
  "idAsignatura": "nuevo-uuid-asignatura",
  "dia": 3,
  "horaInicio": "10:00",
  "horaFin": "12:00",
  "periodo": "2026-1"
}
```

Respuesta exitosa (200 OK): devuelve la asignación actualizada completa.

Errores posibles (400):

- La asignación ya fue confirmada.
- La asignación está cancelada.
- El nuevo docente no fue encontrado.
- El nuevo docente ya alcanzó su límite de carga.
- La nueva asignatura no fue encontrada.
- La hora de inicio debe ser menor que la hora de fin.

### POST /api/asignaciones/confirmar

Confirma una o varias asignaciones en un solo llamado.

Body:

```json
{
  "idsAsignacion": [
    "uuid-asignacion-1",
    "uuid-asignacion-2",
    "uuid-asignacion-3"
  ]
}
```

Respuesta (200 OK):

```json
{
  "confirmadas": 2,
  "fallidas": 1,
  "idsConfirmadas": ["uuid-asignacion-1", "uuid-asignacion-2"],
  "errores": [
    {
      "idAsignacion": "uuid-asignacion-3",
      "motivo": "No se puede confirmar una asignación cancelada."
    }
  ]
}
```

### PATCH /api/asignaciones/{idAsignacion}/cancelar

No requiere body. Cambia el estado a `Cancelada`.

Respuesta exitosa (200 OK): devuelve la asignación con `"estado": "Cancelada"`.

Errores posibles (400):

- La asignación ya fue confirmada.
- La asignación ya está cancelada.

## Flujo de uso completo

```
1. Generar propuestas automáticas:
   POST /api/horarios/generar-propuestas

2. Consultar propuestas pendientes del periodo:
   GET /api/asignaciones/propuestas?periodo=2026-1

3. Por cada propuesta que requiera cambios, ajustarla:
   PATCH /api/asignaciones/{id}/ajustar

4. Cancelar las propuestas que no deben incluirse:
   PATCH /api/asignaciones/{id}/cancelar

5. Confirmar todas las propuestas aprobadas en una sola llamada:
   POST /api/asignaciones/confirmar
   { "idsAsignacion": ["id1", "id2", "id3", ...] }

6. El sistema devuelve cuántas se confirmaron y cuáles fallaron con su motivo.
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

Esta guía cubre el flujo completo desde crear datos base hasta confirmar propuestas.

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

La respuesta muestra todas las asignaturas del sistema con:

- `habilitadaPorCurriculo`: si el docente está habilitado por currículo.
- `yaAsignadaEnPeriodo`: si ya tiene esa asignatura en el periodo.

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

## Paso 5 — Consultar propuestas pendientes del periodo (Issue #12)

```http
GET /api/asignaciones/propuestas?periodo=2026-1
```

Devuelve todas las asignaciones en estado `Propuesta` o `AsignadaManual`. El coordinador ve aquí qué hay pendiente de revisión.

## Paso 6 — Ajustar una propuesta (Issue #12)

Si una propuesta tiene el día o el horario incorrecto, o el docente debe cambiarse:

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

Solo se envían los campos que se quieren modificar. Los demás quedan igual. La respuesta devuelve la asignación completa actualizada.

## Paso 7 — Cancelar una propuesta que no debe incluirse (Issue #12)

```http
PATCH /api/asignaciones/{idAsignacion}/cancelar
```

No requiere body. La respuesta devuelve la asignación con `"estado": "Cancelada"`.

## Paso 8 — Confirmar propuestas aprobadas en bloque (Issue #12)

```http
POST /api/asignaciones/confirmar
```

```json
{
  "idsAsignacion": [
    "uuid-1",
    "uuid-2",
    "uuid-3"
  ]
}
```

La respuesta indica cuántas se confirmaron y cuáles fallaron:

```json
{
  "confirmadas": 3,
  "fallidas": 0,
  "idsConfirmadas": ["uuid-1", "uuid-2", "uuid-3"],
  "errores": []
}
```

## Paso 9 — Reducir disponibilidad por doble jornada (Issue #11)

Si un docente tiene la misma asignatura asignada en jornada diurna y nocturna:

```http
POST /api/curriculos-docentes/{idDocente}/reducir-disponibilidad?idAsignatura={idAsignatura}&periodo=2026-1
```

El sistema elimina los bloques de disponibilidad que se solapan con la jornada diurna y reporta cuáles fueron eliminados.

## Paso 10 — Verificar protecciones (Issue #12)

Intentar ajustar una asignación ya confirmada debe responder `400`:

```json
{ "mensaje": "No se puede ajustar una asignación que ya fue confirmada." }
```

Intentar cancelar una asignación ya confirmada debe responder `400`:

```json
{ "mensaje": "No se puede cancelar una asignación ya confirmada. Elimínela si es necesario." }
```

Intentar confirmar una asignación cancelada la incluye en `errores` sin interrumpir las demás.

---

# Git Flow recomendado

## Ramas por issue

| Issue | Rama sugerida |
|---|---|
| Issue #10 | `feature/asignacion-manual-docente` |
| Issue #11 | `feature/reduccion-disponibilidad-doble-jornada` |
| Issue #12 | `feature/revision-ajuste-confirmacion-propuestas` |

## Crear rama desde develop

```bash
git checkout develop
git pull origin develop
git checkout -b feature/revision-ajuste-confirmacion-propuestas
```

## Guardar y publicar cambios

```bash
git add .
git commit -m "feat(propuestas): revisión, ajuste y confirmación manual de propuestas"
git push origin feature/revision-ajuste-confirmacion-propuestas
```

## Commits recomendados

```txt
# Issue #10
feat(asignaciones): asignación manual de asignaturas a docentes
test(asignaciones): pruebas de asignación manual
docs(readme): documentar Issue #10

# Issue #11
feat(disponibilidad): reducción por doble jornada diurna y nocturna
test(disponibilidad): pruebas de reducción por doble jornada
docs(readme): documentar Issue #11

# Issue #12
feat(propuestas): ajuste, cancelación y confirmación de propuestas
test(propuestas): pruebas de revisión y confirmación
docs(readme): documentar Issue #12
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
- **Issue #10:** Consulta de asignaturas disponibles para un docente por periodo.
- **Issue #11:** Reducción de disponibilidad por doble jornada.
- **Issue #12:** Consulta de propuestas pendientes por periodo.
- **Issue #12:** Ajuste manual de propuestas campo por campo.
- **Issue #12:** Cancelación de propuestas.
- **Issue #12:** Confirmación individual y en bloque de propuestas.

## Pendiente o futuro

- Validación completa de cruces horarios entre asignaciones confirmadas.
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
[ ] El endpoint de importar Excel funciona.
[ ] El endpoint de disponibilidad por docente funciona.
[ ] El plan TAPSI diurna responde correctamente.

# Issue #10
[ ] POST /api/asignaciones/manual crea con estado "AsignadaManual".
[ ] GET  /api/asignaciones/docente/{id}/asignaturas-disponibles responde correctamente.
[ ] El límite de carga se respeta en asignaciones manuales.
[ ] El duplicado en el mismo periodo es rechazado con 400.

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

# General
[ ] No se sube horarios.db.
[ ] No se suben archivos bin/ ni obj/.
[ ] README actualizado.
[ ] El PR apunta hacia develop.
[ ] Los issues #10, #11 y #12 quedan enlazados al PR.
```