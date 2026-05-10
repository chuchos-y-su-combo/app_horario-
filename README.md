# ApplicationSchedule API

Backend para la gestión de horarios académicos.

El sistema permite administrar usuarios, roles, docentes, asignaturas y asignaciones académicas.  
Está desarrollado en C# con ASP.NET Core, Entity Framework Core y SQLite.

---

## Tecnologías utilizadas

- C#
- ASP.NET Core
- Entity Framework Core
- SQLite
- Swagger / OpenAPI
- xUnit
- FluentAssertions
- BCrypt para hash de contraseñas

---

## Estado actual del proyecto

El backend actualmente cubre los siguientes requerimientos:

| Requerimiento | Estado | Descripción |
|---|---|---|
| Req 2 | Implementado | Crear y gestionar cuentas con rol administrador o coordinador |
| Req 3 | Implementado | Registrar docentes con nombre, identificación y tipo de contrato |
| Req 4 | Implementado | Registrar asignaturas con nombre, código, créditos, semestre y plan de estudios |
| Req 5 | Implementado | Limitar carga docente según contrato |
| Req 7 | Implementado | Marcar materias obligatorias TAPSI como fijas |

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

## Tablas principales del modelo oficial

El modelo oficial adaptado a SQLite contempla las siguientes tablas:

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

Actualmente el backend trabaja principalmente con:

```txt
roles
usuarios
planes_estudio
docentes
asignaturas
asignaciones
```

Las demás tablas quedan contempladas para futuras funcionalidades, como importación de disponibilidad, bloqueos y generación completa de horarios.

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
```

---

## Configuración de la base de datos

La cadena de conexión se encuentra en:

```txt
src/ApplicationSchedule.Api/appsettings.json
```

Ejemplo:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=horarios.db"
  }
}
```

La base se crea automáticamente al ejecutar la API mediante:

```csharp
context.Database.EnsureCreated();
```

---

## Importante sobre `horarios.db`

El archivo `horarios.db` es local y no debería subirse al repositorio.

Se recomienda tener en `.gitignore`:

```gitignore
*.db
*.db-shm
*.db-wal
src/ApplicationSchedule.Api/horarios.db
```

Si se cambia la estructura de la base de datos, se recomienda borrar el archivo local `horarios.db` para que se regenere con el modelo actualizado.

---

## Datos base

El sistema crea automáticamente los roles base:

| ID | Rol |
|---|---|
| 1 | Administrador |
| 2 | Coordinador |

También crea dos planes de estudio base:

| ID | Nombre | Jornada |
|---|---|---|
| `11111111-1111-1111-1111-111111111111` | Plan de Estudios 1020 Jornada Diurna | Diurna |
| `22222222-2222-2222-2222-222222222222` | Plan de Estudios Jornada Noche | Nocturna |

---

# Requerimiento 2: usuarios con rol administrador o coordinador

## Descripción

El sistema permite crear y gestionar cuentas de usuario con rol:

- Administrador
- Coordinador

## Tablas usadas

```txt
usuarios
roles
```

## Endpoints

```http
GET /api/usuarios
POST /api/usuarios
GET /api/usuarios/{idUsuario}
PUT /api/usuarios/{idUsuario}
PATCH /api/usuarios/{idUsuario}/password
DELETE /api/usuarios/{idUsuario}
```

## Crear usuario administrador

```http
POST /api/usuarios
```

Body:

```json
{
  "nombreCompleto": "Admin Principal",
  "correo": "admin@test.com",
  "password": "Password123",
  "idRol": 1
}
```

## Crear usuario coordinador

```http
POST /api/usuarios
```

Body:

```json
{
  "nombreCompleto": "Coordinador Principal",
  "correo": "coordinador@test.com",
  "password": "Password123",
  "idRol": 2
}
```

## Validaciones

- El correo debe ser único.
- El rol debe existir.
- La contraseña se almacena como hash usando BCrypt.
- El correo se normaliza a minúsculas.

---

# Requerimiento 3: registro de docentes

## Descripción

El sistema permite registrar docentes con:

- Nombre
- Identificación
- Tipo de contrato

Los tipos de contrato aceptados son:

| Entrada permitida | Valor guardado |
|---|---|
| `TC` | `TC` |
| `Tiempo Completo` | `TC` |
| `TP` | `TP` |
| `Parcial` | `TP` |

## Tabla usada

```txt
docentes
```

Aunque la tabla oficial se llama `docentes`, el endpoint se conserva como:

```http
/api/profesores
```

Esto permite mantener compatibilidad con la estructura anterior del backend y posibles conexiones del frontend.

## Endpoints

```http
GET /api/profesores
POST /api/profesores
GET /api/profesores/{idProfesor}
PUT /api/profesores/{idProfesor}
DELETE /api/profesores/{idProfesor}
```

## Crear docente tiempo completo

```http
POST /api/profesores
```

Body:

```json
{
  "nombre": "Carlos Pérez",
  "identificacion": "1001",
  "tipoContrato": "TC"
}
```

Respuesta esperada:

```json
{
  "idProfesor": "guid-generado",
  "nombre": "Carlos Pérez",
  "identificacion": "1001",
  "tipoContrato": "TC",
  "maxAsignaturas": 5
}
```

## Crear docente tiempo parcial

```http
POST /api/profesores
```

Body:

```json
{
  "nombre": "Ana Gómez",
  "identificacion": "1002",
  "tipoContrato": "TP"
}
```

Respuesta esperada:

```json
{
  "idProfesor": "guid-generado",
  "nombre": "Ana Gómez",
  "identificacion": "1002",
  "tipoContrato": "TP",
  "maxAsignaturas": 3
}
```

## Validaciones

- La identificación debe ser única.
- El tipo de contrato debe ser válido.
- El máximo de asignaturas se calcula automáticamente.
- No se puede eliminar un docente con asignaciones registradas.

---

# Requerimiento 4: registro de asignaturas

## Descripción

El sistema permite registrar asignaturas con:

- Nombre
- Código
- Créditos
- Semestre
- Plan de estudios

## Tablas usadas

```txt
asignaturas
planes_estudio
```

## Endpoints

```http
GET /api/asignaturas
POST /api/asignaturas
GET /api/asignaturas/{idAsignatura}
GET /api/asignaturas/plan/{idPlan}
PUT /api/asignaturas/{idAsignatura}
DELETE /api/asignaturas/{idAsignatura}
```

## Crear asignatura

```http
POST /api/asignaturas
```

Body:

```json
{
  "idPlan": "11111111-1111-1111-1111-111111111111",
  "codigo": "NORMAL101",
  "nombre": "Materia Normal",
  "creditos": 3,
  "semestre": 1
}
```

Respuesta esperada:

```json
{
  "idAsignatura": "guid-generado",
  "idPlan": "11111111-1111-1111-1111-111111111111",
  "codigo": "NORMAL101",
  "nombre": "Materia Normal",
  "creditos": 3,
  "semestre": 1,
  "minEstudiantes": 15,
  "esFijaTapsi": false
}
```

## Validaciones

- El plan de estudios debe existir.
- El código debe ser único.
- Los créditos deben estar entre 1 y 20.
- El semestre debe estar entre 1 y 12.
- El mínimo de estudiantes por defecto es 15.

---

# Requerimiento 7: materias TAPSI fijas

## Descripción

El sistema marca automáticamente como fijas las 5 materias obligatorias TAPSI.

## Tabla usada

```txt
asignaturas
```

Campo usado:

```txt
es_fija_tapsi
```

## Materias TAPSI fijas

| Código | Asignatura |
|---|---|
| 104030 | Cálculo diferencial |
| 103007 | Técnicas de programación |
| 103018 | Programación orientada a objetos |
| 103004 | Teoría de sistemas |
| 103027 | Sistemas operativos |

## Endpoints

```http
GET /api/asignaturas/tapsi/fijas
POST /api/asignaturas/tapsi/marcar-fijas
```

## Crear materia TAPSI

```http
POST /api/asignaturas
```

Body:

```json
{
  "idPlan": "11111111-1111-1111-1111-111111111111",
  "codigo": "103007",
  "nombre": "Técnicas de programación",
  "creditos": 3,
  "semestre": 1
}
```

Respuesta esperada:

```json
{
  "codigo": "103007",
  "nombre": "Técnicas de programación",
  "esFijaTapsi": true
}
```

## Consultar materias TAPSI fijas

```http
GET /api/asignaturas/tapsi/fijas
```

## Marcar materias existentes como fijas

```http
POST /api/asignaturas/tapsi/marcar-fijas
```

## Validaciones

- El sistema reconoce materias TAPSI por código.
- El sistema reconoce materias TAPSI por nombre.
- La comparación de nombres ignora tildes, mayúsculas y espacios dobles.
- No se puede eliminar una materia marcada como TAPSI fija.

---

# Requerimiento 5: límite de carga docente

## Descripción

El sistema limita la carga docente según el tipo de contrato.

| Tipo de contrato | Máximo de asignaturas |
|---|---|
| `TC` | 5 |
| `TP` | 3 |

## Tablas usadas

```txt
docentes
asignaciones
```

La tabla `docentes` define el máximo permitido:

```txt
max_asignaturas
```

La tabla `asignaciones` registra qué asignaturas tiene cada docente en un periodo.

## Endpoints

```http
GET /api/asignaciones
POST /api/asignaciones
GET /api/asignaciones/docente/{idDocente}
GET /api/asignaciones/docente/{idDocente}/resumen?periodo=2026-1
DELETE /api/asignaciones/{idAsignacion}
```

## Crear asignación

```http
POST /api/asignaciones
```

Body:

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

## Regla de carga

Antes de crear una asignación, el sistema:

1. Busca el docente.
2. Lee su `max_asignaturas`.
3. Cuenta cuántas asignaturas distintas tiene asignadas en el periodo.
4. Si ya alcanzó el límite, rechaza la asignación.
5. Si no alcanzó el límite, permite la asignación.

## Importante

El sistema cuenta asignaturas distintas, no bloques horarios.

Ejemplo:

```txt
Programación Backend
Lunes 08:00 - 10:00
Miércoles 08:00 - 10:00
```

Eso puede generar dos registros en `asignaciones`, pero cuenta como una sola asignatura para la carga del docente.

## Respuesta cuando se supera el límite

Para docente `TP`:

```json
{
  "mensaje": "El docente con contrato TP ya alcanzó el límite de 3 asignaturas para el periodo 2026-1."
}
```

Para docente `TC`:

```json
{
  "mensaje": "El docente con contrato TC ya alcanzó el límite de 5 asignaturas para el periodo 2026-1."
}
```

## Consultar resumen de carga

```http
GET /api/asignaciones/docente/{idDocente}/resumen?periodo=2026-1
```

Respuesta esperada:

```json
{
  "idDocente": "ID_DEL_DOCENTE",
  "nombreDocente": "Ana Gómez",
  "tipoContrato": "TP",
  "maxAsignaturas": 3,
  "asignaturasActuales": 2,
  "puedeAsignarMas": true,
  "periodo": "2026-1"
}
```

---

# Cómo ejecutar el proyecto

Desde la raíz del proyecto:

```powershell
dotnet restore .\src\ApplicationSchedule.Api\ApplicationSchedule.Api.csproj
dotnet build .\src\ApplicationSchedule.Api\ApplicationSchedule.Api.csproj
dotnet run --project .\src\ApplicationSchedule.Api\ApplicationSchedule.Api.csproj
```

La API queda disponible en:

```txt
http://localhost:5213
```

Swagger queda disponible en:

```txt
http://localhost:5213/swagger
```

---

# Cómo ejecutar las pruebas

Compilar API:

```powershell
dotnet build .\src\ApplicationSchedule.Api\ApplicationSchedule.Api.csproj
```

Compilar tests:

```powershell
dotnet build .\src\ApplicationSchedule.Tests\ApplicationSchedule.Tests.csproj
```

Ejecutar tests:

```powershell
dotnet test .\src\ApplicationSchedule.Tests\ApplicationSchedule.Tests.csproj
```

Ejecutar tests con detalle:

```powershell
dotnet test .\src\ApplicationSchedule.Tests\ApplicationSchedule.Tests.csproj --logger "console;verbosity=detailed"
```

---

# Pruebas manuales recomendadas en Postman

## 1. Crear usuario administrador

```http
POST /api/usuarios
```

```json
{
  "nombreCompleto": "Admin Principal",
  "correo": "admin@test.com",
  "password": "Password123",
  "idRol": 1
}
```

## 2. Crear usuario coordinador

```http
POST /api/usuarios
```

```json
{
  "nombreCompleto": "Coordinador Principal",
  "correo": "coordinador@test.com",
  "password": "Password123",
  "idRol": 2
}
```

## 3. Crear docente TC

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

Debe devolver:

```json
"maxAsignaturas": 5
```

## 4. Crear docente TP

```http
POST /api/profesores
```

```json
{
  "nombre": "Ana Gómez",
  "identificacion": "1002",
  "tipoContrato": "TP"
}
```

Debe devolver:

```json
"maxAsignaturas": 3
```

## 5. Crear asignatura TAPSI

```http
POST /api/asignaturas
```

```json
{
  "idPlan": "11111111-1111-1111-1111-111111111111",
  "codigo": "103007",
  "nombre": "Técnicas de programación",
  "creditos": 3,
  "semestre": 1
}
```

Debe devolver:

```json
"esFijaTapsi": true
```

## 6. Crear asignatura normal

```http
POST /api/asignaturas
```

```json
{
  "idPlan": "11111111-1111-1111-1111-111111111111",
  "codigo": "NORMAL101",
  "nombre": "Materia Normal",
  "creditos": 3,
  "semestre": 1
}
```

Debe devolver:

```json
"esFijaTapsi": false
```

## 7. Consultar materias TAPSI fijas

```http
GET /api/asignaturas/tapsi/fijas
```

## 8. Crear asignación

```http
POST /api/asignaciones
```

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

## 9. Probar límite docente TP

Crear 3 asignaciones con asignaturas distintas para un docente `TP`.

Al intentar una cuarta asignatura distinta, debe responder:

```http
400 Bad Request
```

## 10. Probar límite docente TC

Crear 5 asignaciones con asignaturas distintas para un docente `TC`.

Al intentar una sexta asignatura distinta, debe responder:

```http
400 Bad Request
```

---

# Git Flow recomendado

El trabajo debe ir en una rama:

```txt
feature/limite-carga
```

El Pull Request debe ir hacia:

```txt
develop
```

Commit recomendado para el requerimiento:

```txt
feat(asignaciones): limitar carga docente según contrato
```

Commit recomendado para documentación:

```txt
docs(readme): documentar funcionalidades y pruebas del backend
```

---

# Alcance actual

## Implementado

- Gestión de usuarios.
- Roles base.
- Registro de docentes.
- Registro de asignaturas.
- Materias TAPSI fijas.
- Límite de carga docente.
- Asignaciones académicas.
- Pruebas automáticas.

## Pendiente

- Importación de disponibilidad docente desde Excel.
- Registro completo de disponibilidad docente.
- Validación de cruces horarios.
- Generación automática completa de horarios.
- Interfaz visual del frontend.

---

# Checklist antes de Pull Request

Antes de solicitar revisión:

```txt
[ ] La API compila correctamente.
[ ] Los tests compilan correctamente.
[ ] Los tests pasan.
[ ] Swagger abre correctamente.
[ ] No se sube horarios.db.
[ ] No se suben archivos bin/ ni obj/.
[ ] README actualizado.
[ ] El PR apunta hacia develop.
[ ] El issue del requerimiento queda enlazado al PR.
```