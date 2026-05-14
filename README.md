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

El backend actualmente cubre los siguientes requerimientos:

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

El modelo adaptado a SQLite contempla las siguientes tablas:

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

Contiene DTOs e interfaces.

En esta capa se definen los contratos que luego implementa la infraestructura.

### ApplicationSchedule.Domain

Contiene las entidades principales del sistema.

Ejemplos:

```txt
Usuario
Rol
Docente
Asignatura
Disponibilidad
Asignacion
PlanEstudio
DocenteHabilitado
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

El archivo `horarios.db` es local y no debe subirse al repositorio.

Se recomienda tener en `.gitignore`:

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

Si se cambia la estructura de la base de datos y se está trabajando en ambiente local de desarrollo, se puede borrar `horarios.db` para que se regenere con el modelo actualizado.

Si la base de datos ya contiene información importante, no debe borrarse. En ese caso se debe ejecutar el script SQL de actualización correspondiente.

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
- No se almacena la contraseña en texto plano.

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
- Si el contrato es `TC`, el máximo queda en 5.
- Si el contrato es `TP`, el máximo queda en 3.
- No se puede eliminar un docente con asignaciones registradas.

---

# Requerimiento 4: registro de asignaturas

## Descripción

El sistema permite registrar asignaturas con:

- Nombre
- Código
- Créditos
- Semestre
- Plan de estudios al que pertenecen

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
  "semestre": 1,
  "minEstudiantes": 15,
  "esFijaTapsi": false,
  "esOpcionalTapsiDiurna": false
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
  "esFijaTapsi": false,
  "esOpcionalTapsiDiurna": false
}
```

## Validaciones

- El plan de estudios debe existir.
- El código debe ser único.
- Los créditos deben estar entre 1 y 20.
- El semestre debe estar entre 1 y 12.
- El mínimo de estudiantes por defecto es 15.

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

# Requerimiento 6: carga de currículo docente desde Excel

## Descripción

El sistema permite importar desde Excel las asignaturas que cada docente está habilitado para dictar.

El archivo Excel corresponde al archivo actual utilizado por coordinación.

## Tabla usada

```txt
docentes_habilitados
```

Esta tabla relaciona:

```txt
docente
asignatura
```

## Endpoint de importación

```http
POST /api/profesores/importar-excel
```

Este endpoint recibe un archivo `.xlsx`.

## Endpoint de consulta

```http
GET /api/profesores/{idProfesor}/asignaturas-habilitadas
```

## Funcionamiento general

Al importar el Excel, el sistema:

1. Lee el archivo `.xlsx`.
2. Identifica las hojas por docente.
3. Lee las asignaturas asociadas al docente.
4. Busca el docente en la base de datos.
5. Busca las asignaturas existentes en la base de datos.
6. Crea la relación en `docentes_habilitados`.
7. Evita duplicar relaciones ya existentes.
8. Reporta docentes o asignaturas no encontradas.

## Observación

Para que el sistema pueda relacionar correctamente la información importada, los docentes y asignaturas deben existir previamente en la base de datos.

---

# Importación de disponibilidad docente desde Excel

## Descripción

El sistema permite importar la disponibilidad docente desde el archivo Excel actual de coordinación.

La disponibilidad importada se almacena en la tabla:

```txt
disponibilidad
```

## Tabla usada

```txt
disponibilidad
```

Campos principales:

| Campo | Descripción |
|---|---|
| `id_disponibilidad` | Identificador único del registro |
| `id_docente` | Docente asociado |
| `dia_semana` | Día de la semana entre 1 y 6 |
| `hora_inicio` | Hora de inicio en formato `HH:mm` |
| `hora_fin` | Hora de fin en formato `HH:mm` |

Equivalencia de días:

| Número | Día |
|---|---|
| 1 | Lunes |
| 2 | Martes |
| 3 | Miércoles |
| 4 | Jueves |
| 5 | Viernes |
| 6 | Sábado |

## Endpoint de importación

```http
POST /api/profesores/importar-excel
```

El mismo endpoint importa:

- Currículo docente.
- Asignaturas habilitadas por docente.
- Disponibilidad docente.

## Endpoint de consulta

```http
GET /api/profesores/{idProfesor}/disponibilidad
```

## Funcionamiento general

Al importar el Excel, el sistema:

1. Recibe el archivo `.xlsx`.
2. Busca la hoja de disponibilidad.
3. Lee los docentes y su texto de disponibilidad.
4. Intenta convertir el texto libre en bloques de día, hora inicio y hora fin.
5. Busca cada docente en la base de datos.
6. Elimina la disponibilidad anterior del docente.
7. Registra la nueva disponibilidad importada.
8. Reporta los casos que no pudo interpretar.

## Formato actual de coordinación

El sistema acepta el Excel actual de coordinación.

En el formato actual, la disponibilidad puede venir escrita como texto libre.

Ejemplos:

```txt
Todo el día L a J
Viernes en la mañana
Después de las 6pm
Lunes a Viernes de 7am a 10am
Martes y jueves de 4 en adelante
```

El sistema interpreta los formatos conocidos y reporta los casos ambiguos.

## Recomendación para mejorar el Excel

Aunque el sistema acepta el Excel actual, se recomienda que a futuro la hoja de disponibilidad tenga un formato normalizado:

| DOCENTE | DIA | HORA INICIO | HORA FIN |
|---|---|---|---|
| Carlos Pérez | Lunes | 07:00 | 09:00 |
| Carlos Pérez | Miércoles | 18:00 | 22:30 |
| Ana Gómez | Viernes | 07:00 | 12:00 |

Esto reduce errores de interpretación y mejora la confiabilidad de la generación de horarios.

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
| 104030 | Cálculo Diferencial |
| 103007 | Técnicas de Programación |
| 103018 | Programación Orientada a Objetos |
| 103004 | Teoría de Sistemas |
| 103027 | Sistemas Operativos |

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
  "nombre": "Técnicas de Programación",
  "creditos": 3,
  "semestre": 1,
  "minEstudiantes": 15,
  "esFijaTapsi": false,
  "esOpcionalTapsiDiurna": false
}
```

Respuesta esperada:

```json
{
  "codigo": "103007",
  "nombre": "Técnicas de Programación",
  "esFijaTapsi": true
}
```

## Validaciones

- El sistema reconoce materias TAPSI por código.
- El sistema reconoce materias TAPSI por nombre.
- La comparación de nombres ignora tildes, mayúsculas y espacios dobles.
- No se puede eliminar una materia marcada como TAPSI fija.

---

# Requerimiento 8: TAPSI jornada diurna

## Descripción

El sistema contempla la asignatura adicional requerida para estudiantes provenientes de TAPSI en jornada diurna.

## Regla implementada

Los estudiantes provenientes de TAPSI tienen 5 materias fijas:

- Cálculo Diferencial
- Técnicas de Programación
- Programación Orientada a Objetos
- Teoría de Sistemas
- Sistemas Operativos

Para jornada diurna, además de esas 5 materias, deben adicionar una asignatura entre:

- Ingeniería de Software II
- Redes LAN
- Programación Back End

## Campo usado

```txt
es_opcional_tapsi_diurna
```

Este campo permite identificar las asignaturas que pueden seleccionarse como opción adicional TAPSI para jornada diurna.

## Asignaturas adicionales TAPSI diurna

| Código | Asignatura |
|---|---|
| 103093 | Ingeniería de Software II |
| 103126 | Redes LAN |
| 109183 | Programación Back End |

## Créditos considerados

| Jornada | Tope considerado |
|---|---|
| Diurna | 18 créditos |
| Extendida / Nocturna | 15 créditos |

Para TAPSI diurna:

```txt
5 materias fijas x 3 créditos = 15 créditos
1 materia adicional x 3 créditos = 3 créditos
Total = 18 créditos
```

## Endpoints

```http
GET /api/asignaturas/tapsi/diurna/opciones-adicionales
GET /api/asignaturas/tapsi/diurna/plan
POST /api/asignaturas/tapsi/diurna/marcar-opciones-adicionales
```

## Consultar opciones adicionales TAPSI diurna

```http
GET /api/asignaturas/tapsi/diurna/opciones-adicionales
```

Respuesta esperada:

```json
[
  {
    "codigo": "103093",
    "nombre": "Ingeniería de Software II",
    "creditos": 3,
    "esOpcionalTapsiDiurna": true
  },
  {
    "codigo": "103126",
    "nombre": "Redes LAN",
    "creditos": 3,
    "esOpcionalTapsiDiurna": true
  },
  {
    "codigo": "109183",
    "nombre": "Programación Back End",
    "creditos": 3,
    "esOpcionalTapsiDiurna": true
  }
]
```

## Consultar plan TAPSI diurna

```http
GET /api/asignaturas/tapsi/diurna/plan
```

Respuesta esperada:

```json
{
  "jornada": "Diurna",
  "topeCreditosDiurna": 18,
  "topeCreditosJornadaExtendida": 15,
  "creditosFijosTapsi": 15,
  "creditosAdicionalesRequeridos": 3,
  "creditosTotalesRequeridosDiurna": 18,
  "regla": "Para TAPSI jornada diurna se deben tomar las 5 materias fijas sin cruce y adicionar exactamente una opción entre Ingeniería de Software II, Redes LAN o Programación Back End."
}
```

## Marcar opciones adicionales existentes

```http
POST /api/asignaturas/tapsi/diurna/marcar-opciones-adicionales
```

Respuesta esperada:

```json
{
  "mensaje": "Asignaturas adicionales TAPSI para jornada diurna marcadas correctamente.",
  "cantidadMarcada": 3
}
```

## Validaciones

- El sistema reconoce las opciones adicionales por código.
- El sistema reconoce las opciones adicionales por nombre.
- La comparación ignora tildes, mayúsculas y espacios dobles.
- Si se crea una de estas asignaturas, se marca automáticamente como opción adicional TAPSI diurna.
- Si ya existían en la base de datos, se pueden marcar con el endpoint correspondiente.

---

# Cambios recientes de base de datos

Para el Req. 8 y disponibilidad docente se deja constancia en el script:

```txt
database/req8_tapsi_diurna_disponibilidad_sqlite.sql
```

## Columna agregada a asignaturas

```sql
ALTER TABLE asignaturas
ADD COLUMN es_opcional_tapsi_diurna INTEGER NOT NULL DEFAULT 0
CHECK (es_opcional_tapsi_diurna IN (0, 1));
```

## Marcado de asignaturas adicionales TAPSI diurna

```sql
UPDATE asignaturas
SET es_opcional_tapsi_diurna = 1
WHERE codigo IN ('103093', '103126', '109183')
   OR UPPER(nombre) IN (
        'INGENIERÍA DE SOFTWARE II',
        'INGENIERIA DE SOFTWARE II',
        'REDES LAN',
        'PROGRAMACIÓN BACK END',
        'PROGRAMACION BACK END',
        'PROGRAMACIÓN BACKEND',
        'PROGRAMACION BACKEND'
   );
```

## Tabla disponibilidad

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
GET /api/usuarios
POST /api/usuarios
GET /api/usuarios/{idUsuario}
PUT /api/usuarios/{idUsuario}
PATCH /api/usuarios/{idUsuario}/password
DELETE /api/usuarios/{idUsuario}
```

## Docentes

```http
GET /api/profesores
POST /api/profesores
GET /api/profesores/{idProfesor}
PUT /api/profesores/{idProfesor}
DELETE /api/profesores/{idProfesor}
```

## Asignaturas

```http
GET /api/asignaturas
POST /api/asignaturas
GET /api/asignaturas/{idAsignatura}
GET /api/asignaturas/plan/{idPlan}
PUT /api/asignaturas/{idAsignatura}
DELETE /api/asignaturas/{idAsignatura}
```

## Asignaciones

```http
GET /api/asignaciones
POST /api/asignaciones
GET /api/asignaciones/docente/{idDocente}
GET /api/asignaciones/docente/{idDocente}/resumen?periodo=2026-1
DELETE /api/asignaciones/{idAsignacion}
```

## Excel y currículo docente

```http
POST /api/profesores/importar-excel
GET /api/profesores/{idProfesor}/asignaturas-habilitadas
GET /api/profesores/{idProfesor}/disponibilidad
```

## TAPSI

```http
GET /api/asignaturas/tapsi/fijas
POST /api/asignaturas/tapsi/marcar-fijas
GET /api/asignaturas/tapsi/diurna/opciones-adicionales
GET /api/asignaturas/tapsi/diurna/plan
POST /api/asignaturas/tapsi/diurna/marcar-opciones-adicionales
```

---

# Cómo ejecutar el proyecto

Desde la raíz del proyecto:

```powershell
dotnet restore .\src\ApplicationSchedule.Api\ApplicationSchedule.Api.csproj
```

Luego:

```powershell
dotnet build .\src\ApplicationSchedule.Api\ApplicationSchedule.Api.csproj
```

Finalmente:

```powershell
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

# Pruebas manuales recomendadas en Swagger o Postman

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

## 5. Crear asignatura TAPSI fija

```http
POST /api/asignaturas
```

```json
{
  "idPlan": "11111111-1111-1111-1111-111111111111",
  "codigo": "103007",
  "nombre": "Técnicas de Programación",
  "creditos": 3,
  "semestre": 1,
  "minEstudiantes": 15,
  "esFijaTapsi": false,
  "esOpcionalTapsiDiurna": false
}
```

Debe devolver:

```json
"esFijaTapsi": true
```

## 6. Crear asignatura adicional TAPSI diurna

```http
POST /api/asignaturas
```

```json
{
  "idPlan": "11111111-1111-1111-1111-111111111111",
  "codigo": "109183",
  "nombre": "Programación Back End",
  "creditos": 3,
  "semestre": 6,
  "minEstudiantes": 15,
  "esFijaTapsi": false,
  "esOpcionalTapsiDiurna": false
}
```

Debe devolver:

```json
"esOpcionalTapsiDiurna": true
```

## 7. Consultar materias TAPSI fijas

```http
GET /api/asignaturas/tapsi/fijas
```

## 8. Consultar opciones adicionales TAPSI diurna

```http
GET /api/asignaturas/tapsi/diurna/opciones-adicionales
```

## 9. Consultar plan TAPSI diurna

```http
GET /api/asignaturas/tapsi/diurna/plan
```

Debe devolver:

```json
"creditosTotalesRequeridosDiurna": 18
```

## 10. Importar Excel de coordinación

```http
POST /api/profesores/importar-excel
```

Seleccionar archivo `.xlsx`.

El sistema debe procesar:

- Asignaturas habilitadas por docente.
- Disponibilidad docente.
- Docentes no encontrados.
- Asignaturas no encontradas.
- Casos de disponibilidad no interpretados.

## 11. Consultar disponibilidad de un docente

```http
GET /api/profesores/{idProfesor}/disponibilidad
```

Respuesta esperada:

```json
[
  {
    "idDisponibilidad": "guid-generado",
    "idDocente": "ID_DEL_DOCENTE",
    "diaSemana": 1,
    "diaNombre": "Lunes",
    "horaInicio": "07:00",
    "horaFin": "12:00"
  }
]
```

## 12. Crear asignación

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

## 13. Probar límite docente TP

Crear 3 asignaciones con asignaturas distintas para un docente `TP`.

Al intentar una cuarta asignatura distinta, debe responder:

```http
400 Bad Request
```

## 14. Probar límite docente TC

Crear 5 asignaciones con asignaturas distintas para un docente `TC`.

Al intentar una sexta asignatura distinta, debe responder:

```http
400 Bad Request
```

---

# Git Flow recomendado

El trabajo debe ir en una rama por requerimiento.

Para este requerimiento se recomienda:

```txt
feature/req8-tapsi-diurna-disponibilidad
```

El Pull Request debe ir hacia:

```txt
develop
```

## Crear rama desde develop

```bash
git checkout develop
git pull origin develop
git checkout -b feature/req8-tapsi-diurna-disponibilidad
```

## Guardar cambios

```bash
git add .
git commit -m "feat(tapsi): implement req8 and availability import"
git push origin feature/req8-tapsi-diurna-disponibilidad
```

## Commits recomendados

```txt
feat(tapsi): implement req8 and availability import
docs(readme): update backend documentation
test(tapsi): validate req8 behavior
fix(swagger): support Excel upload endpoint
```

---

# Trabajo con GitHub Desktop

Flujo recomendado:

1. Abrir el repositorio en GitHub Desktop.
2. Verificar que la rama actual sea la rama del requerimiento.
3. Hacer `Fetch origin`.
4. Actualizar `develop` con `Pull origin`.
5. Volver a la rama del requerimiento.
6. Hacer merge de `develop` en la rama actual.
7. Revisar los archivos modificados.
8. Confirmar que no se suba `horarios.db`, `bin/` ni `obj/`.
9. Crear commit.
10. Hacer `Push origin` o `Publish branch`.
11. Crear Pull Request en GitHub Web hacia `develop`.

---

# Pull Request recomendado

## Título

```txt
feat(tapsi): implementar Req. 8 e importación de disponibilidad docente
```

## Descripción sugerida

```md
## Descripción

Se implementa el requerimiento relacionado con TAPSI jornada diurna y se fortalece la importación de disponibilidad docente desde el Excel actual de coordinación.

## Cambios realizados

- Se agregó soporte para marcar asignaturas adicionales TAPSI jornada diurna.
- Se contemplan como opciones adicionales:
  - Ingeniería de Software II
  - Redes LAN
  - Programación Back End.
- Se mantiene la regla de las 5 asignaturas fijas TAPSI.
- Se agregó la entidad `Disponibilidad`.
- Se configuró la tabla `disponibilidad` en Entity Framework.
- Se agregó lectura de disponibilidad docente desde la hoja de disponibilidad del Excel actual de coordinación.
- Se creó un parser para interpretar disponibilidad escrita en texto libre.
- Se agregó endpoint para consultar disponibilidad importada por docente.
- Se actualizó el README.

## Endpoints relacionados

```http
POST /api/profesores/importar-excel
GET /api/profesores/{idProfesor}/disponibilidad
GET /api/asignaturas/tapsi/diurna/opciones-adicionales
GET /api/asignaturas/tapsi/diurna/plan
POST /api/asignaturas/tapsi/diurna/marcar-opciones-adicionales
```

## Pruebas realizadas

- `dotnet build`
- `dotnet test`
- Pruebas manuales desde Swagger
- Creación de docentes TC y TP
- Validación de máximo de asignaturas según contrato
- Creación de asignaturas TAPSI fijas
- Creación de asignaturas adicionales TAPSI diurna
- Consulta del plan TAPSI diurna
- Importación del archivo Excel actual de coordinación
- Consulta de disponibilidad docente importada

## Observación técnica

El Excel actual de coordinación se conserva como fuente válida. Sin embargo, la disponibilidad está escrita en texto libre, por lo que el sistema interpreta las frases conocidas y reporta los casos ambiguos para revisión manual.

Se recomienda que a futuro coordinación use una hoja normalizada con columnas:

- DOCENTE
- DIA
- HORA INICIO
- HORA FIN
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
- Carga de currículo docente desde Excel.
- Importación de disponibilidad docente desde Excel.
- Req. 8 TAPSI jornada diurna.
- Consulta de disponibilidad por docente.
- Pruebas automáticas.
- Documentación actualizada.

## Pendiente o futuro

- Validación completa de cruces horarios.
- Generación automática completa de horarios.
- Interfaz visual del frontend.
- Mejora futura del formato de Excel para evitar ambigüedades.
- Reglas avanzadas de selección automática de la materia adicional TAPSI diurna.

---

# Checklist antes de Pull Request

Antes de solicitar revisión:

```txt
[ ] La API compila correctamente.
[ ] Los tests compilan correctamente.
[ ] Los tests pasan.
[ ] Swagger abre correctamente.
[ ] El endpoint de importar Excel funciona.
[ ] El endpoint de disponibilidad por docente funciona.
[ ] El plan TAPSI diurna responde correctamente.
[ ] No se sube horarios.db.
[ ] No se suben archivos bin/ ni obj/.
[ ] No se suben archivos .dll ni .pdb.
[ ] README actualizado.
[ ] El PR apunta hacia develop.
[ ] El issue del requerimiento queda enlazado al PR.
```

---

# Cierre del requerimiento

Para cerrar el requerimiento en GitHub se recomienda dejar este comentario en el issue:

```md
Se finaliza el requerimiento.

## Evidencia de cumplimiento

El sistema permite importar la disponibilidad docente desde el Excel actual de coordinación mediante:

```http
POST /api/profesores/importar-excel
```

La información importada se almacena en la tabla `disponibilidad` y puede consultarse por docente mediante:

```http
GET /api/profesores/{idProfesor}/disponibilidad
```

También se implementó el Req. 8 para TAPSI jornada diurna:

- Se mantienen las 5 asignaturas fijas TAPSI.
- Se contempla una asignatura adicional para jornada diurna entre:
  - Ingeniería de Software II
  - Redes LAN
  - Programación Back End.
- Se respeta el tope de 18 créditos para jornada diurna y 15 créditos para jornada extendida/nocturna.

## Pruebas realizadas

- `dotnet build` ejecutado correctamente.
- `dotnet test` ejecutado correctamente.
- Pruebas manuales en Swagger realizadas correctamente.
- Importación de Excel validada.
- Consulta de disponibilidad docente validada.
- Consulta del plan TAPSI diurna validada.

## Observación

El Excel actual de coordinación se mantiene como base. Debido a que la disponibilidad viene en texto libre, el sistema interpreta los formatos conocidos y deja trazabilidad de casos ambiguos. Se recomienda normalizar la hoja de disponibilidad en futuras versiones.
```