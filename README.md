# ApplicationSchedule API

Backend para la gestión de horarios académicos universitarios.

Este proyecto hace parte de una aplicación de escritorio para la planeación, generación, revisión y exportación de horarios académicos. El backend está construido en **C#**, **ASP.NET Core Web API**, **Entity Framework Core** y **SQLite**.

La finalidad del backend es exponer servicios REST para que el equipo de frontend pueda integrarlos en una aplicación de escritorio. Aunque la aplicación final será visual y local, el backend funciona como una API local que centraliza reglas de negocio, persistencia y validaciones.

---

## 1. Contexto del proyecto

La universidad requiere un sistema que apoye la organización de horarios académicos considerando restricciones reales de coordinación, docentes, asignaturas, planes de estudio y jornadas.

El sistema permite gestionar:

- Usuarios con rol administrador o coordinador.
- Docentes.
- Asignaturas.
- Planes de estudio.
- Disponibilidad docente.
- Currículo docente.
- Asignaciones manuales.
- Generación automática de propuestas de horario.
- Revisión y ajuste de propuestas.
- Validación de conflictos.
- Exportación de horarios.
- Historial de asignaciones.
- Bloqueo de franjas horarias para asignaturas específicas.

---

## 2. Tecnologías utilizadas

| Tecnología | Uso |
|---|---|
| C# | Lenguaje principal del backend |
| .NET | Plataforma de ejecución |
| ASP.NET Core Web API | Exposición de endpoints REST |
| Entity Framework Core | Acceso a datos |
| SQLite | Base de datos local |
| Swagger / OpenAPI | Pruebas y documentación interactiva de endpoints |
| xUnit | Pruebas automatizadas |
| FluentAssertions | Validaciones legibles en pruebas |
| Excel .xlsx | Importación de disponibilidad y currículo docente |
| Git | Control de versiones |
| GitHub | Repositorio remoto y Pull Requests |
| GitHub Desktop | Flujo visual para ramas, commits y push |

---

## 3. Arquitectura general

El proyecto está organizado por capas para separar responsabilidades y facilitar mantenimiento.

```txt
src/
├── ApplicationSchedule.Api
│   ├── Controllers
│   └── Program.cs
│
├── ApplicationSchedule.Application
│   ├── DTOs
│   └── Interfaces
│
├── ApplicationSchedule.Domain
│   └── Entities
│
├── ApplicationSchedule.Infrastructure
│   ├── Data
│   └── services
│
└── ApplicationSchedule.Tests
    ├── Controllers
    └── Infrastructure
```

---

## 4. Responsabilidad de cada capa

### 4.1. ApplicationSchedule.Api

Contiene los controladores y la configuración principal de la API.

Responsabilidades:

- Recibir peticiones HTTP.
- Validar entrada básica.
- Devolver respuestas HTTP.
- Configurar Swagger.
- Configurar autenticación.
- Registrar servicios en inyección de dependencias.
- Inicializar la base de datos local.

Ejemplos de controladores:

```txt
Controllers/
├── AuthController.cs
├── UsuariosController.cs
├── ProfesoresController.cs
├── AsignaturasController.cs
├── AsignacionesController.cs
├── HorariosController.cs
├── BloqueosFranjaAsignaturaController.cs
```

---

### 4.2. ApplicationSchedule.Application

Contiene DTOs e interfaces.

Responsabilidades:

- Definir qué datos recibe la API.
- Definir qué datos devuelve la API.
- Definir interfaces de servicios.
- Evitar que los controladores dependan directamente de la capa Infrastructure.

Ejemplos:

```txt
DTOs/
├── Asignaturas
├── Profesores
├── Asignaciones
├── Horarios
├── Bloqueos
└── Usuarios

Interfaces/
├── IAsignaturaService.cs
├── IProfesorService.cs
├── IAsignacionService.cs
├── IGeneradorHorarioService.cs
├── IConflictoAsignacionService.cs
├── IBloqueoFranjaAsignaturaService.cs
```

---

### 4.3. ApplicationSchedule.Domain

Contiene las entidades principales del dominio.

Responsabilidades:

- Representar los objetos principales del sistema.
- Mantener la estructura base del modelo.
- No depender de controladores ni de infraestructura.

Ejemplos:

```txt
Entities/
├── Usuario.cs
├── Rol.cs
├── Docente.cs
├── Asignatura.cs
├── PlanEstudio.cs
├── Disponibilidad.cs
├── Asignacion.cs
├── DocenteHabilitado.cs
├── BloqueoFranjaAsignatura.cs
```

---

### 4.4. ApplicationSchedule.Infrastructure

Contiene la implementación concreta de los servicios, acceso a datos y reglas de negocio.

Responsabilidades:

- Implementar servicios.
- Consultar y modificar SQLite mediante Entity Framework Core.
- Parsear archivos Excel.
- Aplicar reglas de negocio.
- Generar propuestas de horario.
- Detectar conflictos.
- Exportar reportes.

Ejemplos:

```txt
Data/
└── AppDbContext.cs

services/
├── AsignaturaService.cs
├── ProfesorService.cs
├── AsignacionService.cs
├── GeneradorHorarioService.cs
├── ConflictoAsignacionService.cs
├── BloqueoFranjaAsignaturaService.cs
├── DisponibilidadExcelParser.cs
```

---

## 5. Base de datos

El sistema utiliza **SQLite** como base de datos local.

Tablas principales:

```txt
roles
usuarios
planes_estudio
docentes
asignaturas
disponibilidades
asignaciones
docentes_habilitados
bloqueos_franja_asignatura
```

SQLite permite que la aplicación funcione localmente sin depender de un servidor externo de base de datos.

---

## 6. Estado de requerimientos implementados

| Issue / Req | Estado | Descripción |
|---|---:|---|
| #1 | Implementado | Crear base de datos del sistema. |
| #2 | Implementado | Crear y gestionar cuentas con rol administrador o coordinador. |
| #3 | Implementado | Registrar docentes con nombre, identificación y tipo de contrato. |
| #4 | Implementado | Registrar asignaturas con nombre, código, créditos, semestre y plan de estudios. |
| #5 | Implementado | Limitar carga según contrato: tiempo completo máximo 5 asignaturas, tiempo parcial máximo 3. |
| #6 | Implementado | Importar disponibilidad docente desde archivo Excel actual de coordinación. |
| #7 | Implementado | Marcar las 5 materias obligatorias TAPSI como fijas en la generación de horarios. |
| #8 | Implementado | Contemplar asignatura adicional requerida para TAPSI jornada diurna. |
| #9 | Implementado | Generar automáticamente propuestas de asignación para los 4 planes/jornadas. |
| #10 | Implementado | Permitir asignar asignaturas a docentes de forma manual. |
| #11 | Implementado | Reducir disponibilidad de un docente que dicta la misma materia en jornada diurna y nocturna. |
| #12 | Implementado | Permitir al coordinador revisar y ajustar manualmente la propuesta generada antes de confirmarla. |
| #13 | Implementado | Exportar horario filtrado por semestre. |
| #14 | Implementado | Exportar horario filtrado por docente. |
| #15 | Implementado | Exportar horario filtrado por asignatura. |
| #16 | Implementado | Generar reporte de horas de clase asignadas vs carga contractual por docente. |
| #17 | Implementado | Mostrar horario individual de cada docente. |
| #18 | Implementado | Alertar cuando una asignación genera conflicto. |
| #19 | Implementado | Conservar historial de asignaciones de semestres anteriores para consulta. |
| #20 | Implementado | Cargar currículo docente y determinar automáticamente asignaturas que puede dictar. |
| #34 | Implementado | Autenticación con correo y contraseña. |
| #36 | Implementado | Bloquear franjas horarias para una asignatura específica. |
| #38 | Implementado | Exportación disponible al menos en formato Excel .xlsx. |
| #39 | Implementado | Mostrar horario en vista de calendario semanal filtrable por plan. |
| #40 | Implementado | Mostrar horario individual de cada docente. |

---

## 7. Requerimiento 36: bloqueo de franjas horarias por asignatura

### 7.1. Descripción

El coordinador puede bloquear una franja horaria para una asignatura específica.

Esto significa que una asignatura no podrá ser programada en un periodo, día y rango de horas definido.

Ejemplo:

```txt
Asignatura: Cálculo Diferencial
Periodo: 2026-1
Día: Lunes
Bloqueo: 08:00 - 10:00
```

Resultado:

```txt
La asignatura Cálculo Diferencial no podrá programarse el lunes entre 08:00 y 10:00 durante el periodo 2026-1.
```

---

### 7.2. Tabla creada

```txt
bloqueos_franja_asignatura
```

Campos principales:

| Campo | Descripción |
|---|---|
| id_bloqueo | Identificador único del bloqueo |
| id_asignatura | Asignatura afectada |
| periodo | Periodo académico |
| dia | Día de la semana |
| hora_inicio | Hora inicial bloqueada |
| hora_fin | Hora final bloqueada |
| motivo | Motivo opcional del bloqueo |
| fecha_creacion_utc | Fecha de creación del registro |

---

### 7.3. Reglas implementadas

El sistema valida que:

- La asignatura exista.
- El periodo sea obligatorio.
- El día esté entre 1 y 6.
- La hora de inicio sea menor que la hora de fin.
- No exista otro bloqueo solapado para la misma asignatura, periodo y día.
- El generador automático no use franjas bloqueadas.
- Las asignaciones manuales no puedan hacerse sobre franjas bloqueadas.
- Los ajustes manuales no puedan mover una asignatura a una franja bloqueada.
- El reporte de conflictos detecte asignaciones existentes que caen dentro de una franja bloqueada.

---

### 7.4. Integración con Excel

El archivo Excel de coordinación se usa para importar disponibilidad docente y datos relacionados con los profesores.

El Req 36 **no reemplaza** esa disponibilidad. La complementa.

Flujo correcto:

```txt
Excel indica cuándo puede dictar clase un docente.
Req 36 indica cuándo NO se puede dictar una asignatura específica.
El generador cruza ambas restricciones.
```

Ejemplo:

```txt
Docente disponible:
Lunes 08:00 - 12:00

Bloqueo de asignatura:
Lunes 08:00 - 10:00

Resultado:
La asignatura no se programa de 08:00 a 10:00.
El sistema puede intentar ubicarla de 10:00 a 12:00 o en otra disponibilidad válida.
```

Si una asignación ya existía antes de crear el bloqueo, el sistema no la elimina automáticamente. En ese caso, el reporte de conflictos debe alertar que hay una asignación ubicada dentro de una franja bloqueada.

---

## 8. Endpoints principales

### 8.1. Autenticación

```http
POST /api/auth/login
```

Body de ejemplo:

```json
{
  "correo": "coordinador@test.com",
  "password": "Password123"
}
```

---

### 8.2. Usuarios

```http
POST   /api/usuarios
GET    /api/usuarios
GET    /api/usuarios/{id}
PUT    /api/usuarios/{id}
DELETE /api/usuarios/{id}
```

Body de ejemplo para crear usuario:

```json
{
  "nombreCompleto": "Coordinador Pruebas",
  "correo": "coordinador@test.com",
  "password": "Password123",
  "idRol": 2
}
```

---

### 8.3. Docentes

```http
POST   /api/profesores
GET    /api/profesores
GET    /api/profesores/{id}
PUT    /api/profesores/{id}
DELETE /api/profesores/{id}
```

Body de ejemplo:

```json
{
  "nombre": "Docente Prueba",
  "identificacion": "123456789",
  "tipoContrato": "TC"
}
```

Tipos de contrato:

```txt
TC = Tiempo completo
TP = Tiempo parcial
```

Reglas:

```txt
TC: máximo 5 asignaturas
TP: máximo 3 asignaturas
```

---

### 8.4. Asignaturas

```http
POST   /api/asignaturas
GET    /api/asignaturas
GET    /api/asignaturas/{id}
PUT    /api/asignaturas/{id}
DELETE /api/asignaturas/{id}
```

Body de ejemplo:

```json
{
  "idPlan": "11111111-1111-1111-1111-111111111111",
  "codigo": "103007",
  "nombre": "Técnicas de programación",
  "creditos": 3,
  "semestre": 1,
  "minEstudiantes": 15,
  "esFijaTapsi": true,
  "esOpcionalTapsiDiurna": false
}
```

---

### 8.5. Asignaciones manuales

```http
POST   /api/asignaciones
GET    /api/asignaciones
GET    /api/asignaciones/{id}
PUT    /api/asignaciones/{id}
DELETE /api/asignaciones/{id}
```

Body de ejemplo:

```json
{
  "idDocente": "ID_DOCENTE",
  "idAsignatura": "ID_ASIGNATURA",
  "dia": 1,
  "horaInicio": "08:00",
  "horaFin": "10:00",
  "periodo": "2026-1"
}
```

Validaciones principales:

- El docente debe existir.
- La asignatura debe existir.
- No debe exceder la carga contractual.
- No debe existir cruce horario del docente.
- No debe existir duplicado exacto.
- No debe caer en una franja bloqueada por el Req 36.

---

### 8.6. Generación automática de horarios

```http
POST /api/horarios/generar-propuestas
```

Body de ejemplo:

```json
{
  "periodo": "2026-1",
  "escenarios": [
    "ING_DIURNA",
    "ING_NOCTURNA",
    "TAPSI_DIURNA",
    "TAPSI_NOCTURNA"
  ],
  "semestreIngenieria": 1,
  "borrarPropuestasPrevias": true
}
```

El generador evalúa:

- Plan de estudios.
- Jornada.
- Semestre.
- Disponibilidad docente.
- Docentes habilitados.
- Carga máxima por contrato.
- Cruces de horario.
- Materias fijas TAPSI.
- Materias adicionales TAPSI.
- Homologaciones TAPSI.
- Bloqueos de franja por asignatura.

---

### 8.7. Bloqueos de franja por asignatura

```http
GET    /api/asignaturas/bloqueos-franja?periodo=2026-1
GET    /api/asignaturas/{idAsignatura}/bloqueos-franja?periodo=2026-1
POST   /api/asignaturas/{idAsignatura}/bloqueos-franja
DELETE /api/asignaturas/bloqueos-franja/{idBloqueo}
```

Body de ejemplo para crear bloqueo:

```json
{
  "periodo": "2026-1",
  "dia": 1,
  "horaInicio": "08:00",
  "horaFin": "10:00",
  "motivo": "Laboratorio no disponible para esta asignatura"
}
```

Respuesta esperada:

```json
{
  "idBloqueo": "GUID",
  "idAsignatura": "ID_ASIGNATURA",
  "codigoAsignatura": "103007",
  "nombreAsignatura": "Técnicas de programación",
  "periodo": "2026-1",
  "dia": 1,
  "diaNombre": "Lunes",
  "horaInicio": "08:00",
  "horaFin": "10:00",
  "motivo": "Laboratorio no disponible para esta asignatura",
  "fechaCreacionUtc": "2026-05-18T00:00:00Z"
}
```

---

### 8.8. Conflictos de horario

```http
GET /api/horarios/conflictos?periodo=2026-1
```

Conflictos detectados:

- Cruce de horario.
- Exceso de carga.
- Asignatura sin docente.
- Docente con asignatura sin horario.
- Franja bloqueada por asignatura.

---

### 8.9. Exportaciones

El sistema permite exportar horarios en formato Excel `.xlsx`.

Filtros soportados:

```txt
Por semestre
Por docente
Por asignatura
```

---

### 8.10. Horario individual docente

El sistema permite consultar el horario individual de cada docente.

Uso esperado:

```txt
El frontend podrá mostrar una vista semanal del docente con sus asignaciones.
```

---

## 9. Materias fijas TAPSI

El sistema contempla materias obligatorias TAPSI que deben tratarse como fijas en la generación de horarios.

Materias TAPSI fijas:

| Código | Asignatura |
|---|---|
| 104030 | Cálculo Diferencial |
| 103007 | Técnicas de Programación |
| 103018 | Programación Orientada a Objetos |
| 103004 | Teoría de Sistemas |
| 103027 | Sistemas Operativos |

Estas materias:

- No deben moverse arbitrariamente.
- No deben cruzarse.
- Deben respetarse como restricciones fuertes.
- Deben ubicarse en franjas separadas.

---

## 10. Homologaciones TAPSI

Para la generación de horarios TAPSI, el sistema asume que ciertas asignaturas homologables ya fueron homologadas.

Esto evita que el generador intente programar materias que no deberían aparecer como pendientes para TAPSI.

Ejemplos de asignaturas homologables:

```txt
Matemáticas básicas
Álgebra lineal
Fundamentos de ingeniería
Lógica de programación
Fundamentos de programación orientada a objetos
```

---

## 11. Importación desde Excel

El sistema permite trabajar con el archivo Excel actual de coordinación.

El Excel puede contener:

- Disponibilidad general.
- Hojas por docente.
- Horarios disponibles.
- Información de asignaturas o espacios asociados.
- Información útil para cargar disponibilidad al sistema.

El archivo Excel no se reemplaza por otro formato. Se mantiene como entrada válida del sistema porque corresponde al archivo usado por coordinación.

Flujo esperado:

```txt
1. Coordinación entrega el Excel.
2. El backend procesa el archivo.
3. Se cargan disponibilidades y/o currículo docente.
4. El generador automático usa esa información.
5. El coordinador puede revisar y ajustar.
6. El coordinador puede bloquear franjas específicas por asignatura.
7. El sistema valida conflictos.
8. El sistema genera exportaciones.
```

---

## 12. Flujo completo de uso

### Paso 1: ejecutar la API

```powershell
dotnet run --project .\src\ApplicationSchedule.Api\ApplicationSchedule.Api.csproj
```

Swagger:

```txt
http://localhost:5213/swagger
```

---

### Paso 2: crear usuario coordinador

```http
POST /api/usuarios
```

```json
{
  "nombreCompleto": "Coordinador Pruebas",
  "correo": "coordinador@test.com",
  "password": "Password123",
  "idRol": 2
}
```

---

### Paso 3: iniciar sesión

```http
POST /api/auth/login
```

```json
{
  "correo": "coordinador@test.com",
  "password": "Password123"
}
```

Copiar el token y usarlo en Swagger o Postman como:

```txt
Bearer TOKEN_GENERADO
```

---

### Paso 4: registrar docentes

```http
POST /api/profesores
```

```json
{
  "nombre": "Docente Prueba",
  "identificacion": "123456789",
  "tipoContrato": "TC"
}
```

---

### Paso 5: registrar asignaturas

```http
POST /api/asignaturas
```

```json
{
  "idPlan": "11111111-1111-1111-1111-111111111111",
  "codigo": "REQ36001",
  "nombre": "Asignatura Prueba Bloqueo",
  "creditos": 2,
  "semestre": 1,
  "minEstudiantes": 15,
  "esFijaTapsi": false,
  "esOpcionalTapsiDiurna": false
}
```

---

### Paso 6: importar disponibilidad desde Excel

Usar el endpoint correspondiente de importación disponible en Swagger.

El archivo esperado es un `.xlsx`.

El Excel se usa para alimentar disponibilidad y datos relacionados con docentes. El Req 36 funciona como una restricción adicional guardada en SQLite.

---

### Paso 7: crear bloqueo de franja por asignatura

```http
POST /api/asignaturas/{idAsignatura}/bloqueos-franja
```

```json
{
  "periodo": "2026-1",
  "dia": 1,
  "horaInicio": "08:00",
  "horaFin": "10:00",
  "motivo": "Franja bloqueada por coordinación"
}
```

---

### Paso 8: generar propuestas de horario

```http
POST /api/horarios/generar-propuestas
```

```json
{
  "periodo": "2026-1",
  "escenarios": [
    "ING_DIURNA",
    "ING_NOCTURNA",
    "TAPSI_DIURNA",
    "TAPSI_NOCTURNA"
  ],
  "semestreIngenieria": 1,
  "borrarPropuestasPrevias": true
}
```

---

### Paso 9: revisar conflictos

```http
GET /api/horarios/conflictos?periodo=2026-1
```

---

### Paso 10: ajustar, confirmar y exportar

El coordinador puede:

- Revisar la propuesta.
- Ajustar horarios.
- Confirmar asignaciones.
- Exportar horarios filtrados.

---

## 13. Instalación y ejecución local

### 13.1. Restaurar paquetes

```powershell
dotnet restore .\src\ApplicationSchedule.Api\ApplicationSchedule.Api.csproj
```

---

### 13.2. Compilar

```powershell
dotnet build .\src\ApplicationSchedule.Api\ApplicationSchedule.Api.csproj
```

---

### 13.3. Ejecutar pruebas

```powershell
dotnet test .\src\ApplicationSchedule.Tests\ApplicationSchedule.Tests.csproj
```

---

### 13.4. Ejecutar API

```powershell
dotnet run --project .\src\ApplicationSchedule.Api\ApplicationSchedule.Api.csproj
```

---

### 13.5. Abrir Swagger

```txt
http://localhost:5213/swagger
```

---

## 14. Pruebas manuales del Req 36

### 14.1. Crear asignatura

```http
POST /api/asignaturas
```

```json
{
  "idPlan": "11111111-1111-1111-1111-111111111111",
  "codigo": "REQ36001",
  "nombre": "Asignatura Prueba Bloqueo",
  "creditos": 2,
  "semestre": 1,
  "minEstudiantes": 15,
  "esFijaTapsi": false,
  "esOpcionalTapsiDiurna": false
}
```

Guardar el `idAsignatura`.

---

### 14.2. Crear docente

```http
POST /api/profesores
```

```json
{
  "nombre": "Docente Prueba Req 36",
  "identificacion": "REQ36001",
  "tipoContrato": "TC"
}
```

Guardar el `idProfesor`.

---

### 14.3. Crear bloqueo

```http
POST /api/asignaturas/{idAsignatura}/bloqueos-franja
```

```json
{
  "periodo": "2026-1",
  "dia": 1,
  "horaInicio": "08:00",
  "horaFin": "10:00",
  "motivo": "Franja bloqueada para validar Req 36"
}
```

Resultado esperado:

```txt
201 Created
```

---

### 14.4. Intentar asignar dentro de la franja bloqueada

```http
POST /api/asignaciones
```

```json
{
  "idDocente": "PEGAR_ID_DOCENTE",
  "idAsignatura": "PEGAR_ID_ASIGNATURA",
  "dia": 1,
  "horaInicio": "09:00",
  "horaFin": "11:00",
  "periodo": "2026-1"
}
```

Resultado esperado:

```txt
400 Bad Request
```

Motivo:

```txt
La asignatura tiene bloqueada la franja del día 1 entre 08:00 y 10:00.
```

---

### 14.5. Intentar asignar fuera de la franja bloqueada

```http
POST /api/asignaciones
```

```json
{
  "idDocente": "PEGAR_ID_DOCENTE",
  "idAsignatura": "PEGAR_ID_ASIGNATURA",
  "dia": 1,
  "horaInicio": "10:00",
  "horaFin": "12:00",
  "periodo": "2026-1"
}
```

Resultado esperado:

```txt
201 Created
```

Motivo:

```txt
10:00 - 12:00 no se cruza con 08:00 - 10:00.
```

---

### 14.6. Consultar bloqueos

```http
GET /api/asignaturas/bloqueos-franja?periodo=2026-1
```

Resultado esperado:

```txt
200 OK
```

---

### 14.7. Eliminar bloqueo

```http
DELETE /api/asignaturas/bloqueos-franja/{idBloqueo}
```

Resultado esperado:

```txt
204 No Content
```

---

## 15. Pruebas automatizadas

El proyecto incluye pruebas automatizadas para validar funcionamiento general y reglas críticas del backend.

Ejecutar:

```powershell
dotnet test .\src\ApplicationSchedule.Tests\ApplicationSchedule.Tests.csproj
```

Pruebas recomendadas para esta rama:

```txt
- Crear bloqueo de franja.
- Consultar bloqueo de franja.
- Eliminar bloqueo de franja.
- Rechazar asignación manual en franja bloqueada.
- Permitir asignación manual fuera de la franja bloqueada.
- Verificar que el generador automático evite franjas bloqueadas.
- Verificar conflictos por franja bloqueada.
```

---

## 16. Validaciones principales del sistema

### 16.1. Carga contractual

```txt
TC: máximo 5 asignaturas.
TP: máximo 3 asignaturas.
```

---

### 16.2. Cruces de horario

El sistema evita que un mismo docente tenga dos asignaciones en el mismo día y horario.

---

### 16.3. Materias TAPSI fijas

El sistema identifica materias TAPSI obligatorias y las trata como restricciones fuertes durante la generación.

---

### 16.4. Disponibilidad docente

La disponibilidad importada desde Excel se usa para decidir en qué franjas puede dictar clase un docente.

---

### 16.5. Bloqueo de franjas por asignatura

Aunque un docente esté disponible, una asignatura no puede programarse en una franja bloqueada para ella.

---

## 17. Recomendaciones para frontend

El frontend puede consumir la API localmente desde:

```txt
http://localhost:5213
```

Pantallas sugeridas:

```txt
- Login.
- Gestión de usuarios.
- Gestión de docentes.
- Gestión de asignaturas.
- Importación de Excel.
- Vista semanal de horario.
- Generación automática de propuestas.
- Revisión de conflictos.
- Ajuste manual de asignaciones.
- Bloqueo de franjas por asignatura.
- Exportaciones.
```

Para el Req 36, se recomienda una pantalla donde el coordinador pueda:

```txt
1. Seleccionar periodo.
2. Seleccionar asignatura.
3. Seleccionar día.
4. Seleccionar hora inicio.
5. Seleccionar hora fin.
6. Escribir motivo opcional.
7. Guardar bloqueo.
8. Ver bloqueos existentes.
9. Eliminar bloqueos.
```

---

## 18. Consideraciones importantes

- El sistema está diseñado para ejecutarse localmente.
- La API puede ser consumida por una aplicación de escritorio.
- SQLite permite trabajar sin servidor externo de base de datos.
- El archivo Excel sigue siendo una entrada válida para coordinación.
- Los bloqueos del Req 36 se guardan en SQLite, no en el Excel.
- Los bloqueos no eliminan asignaciones existentes automáticamente.
- Si una asignación existente queda dentro de una franja bloqueada, debe aparecer como conflicto.

---

## 19. Solución de errores comunes

### 19.1. Error: no se puede encontrar un proyecto para restaurar

Ejecutar el comando apuntando al `.csproj`:

```powershell
dotnet restore .\src\ApplicationSchedule.Api\ApplicationSchedule.Api.csproj
```

---

### 19.2. Error: puerto ocupado

Cerrar la consola anterior donde se esté ejecutando la API o cambiar el puerto en `launchSettings.json`.

---

### 19.3. Error: no aparece tabla nueva en SQLite

Si la base ya existía antes del cambio, `EnsureCreated()` no siempre crea nuevas tablas sobre una base ya creada.

Soluciones:

```txt
1. Ejecutar nuevamente la API y verificar que Program.cs cree la tabla con CREATE TABLE IF NOT EXISTS.
2. Aplicar el script SQL manualmente.
3. En ambiente de pruebas, eliminar la base local si no hay datos importantes.
```

---

### 19.4. Error: 401 Unauthorized en Swagger o Postman

Debes iniciar sesión y enviar el token:

```txt
Authorization: Bearer TOKEN
```

---

## 20. Comandos útiles

```powershell
dotnet restore .\src\ApplicationSchedule.Api\ApplicationSchedule.Api.csproj
dotnet build .\src\ApplicationSchedule.Api\ApplicationSchedule.Api.csproj
dotnet test .\src\ApplicationSchedule.Tests\ApplicationSchedule.Tests.csproj
dotnet run --project .\src\ApplicationSchedule.Api\ApplicationSchedule.Api.csproj
```

---

## 21. Flujo Git recomendado para el Req 36

Crear rama:

```bash
git checkout -b feature/req36-bloqueo-franjas-asignatura
```

Agregar cambios:

```bash
git add .
```

Commit:

```bash
git commit -m "feat(horarios): bloquear franjas horarias por asignatura"
```

Subir rama:

```bash
git push origin feature/req36-bloqueo-franjas-asignatura
```

Pull Request:

```txt
Base: develop
Compare: feature/req36-bloqueo-franjas-asignatura
```

---

## 22. Descripción sugerida del Pull Request

```md
## Descripción

Se implementa el Req 36: el coordinador puede bloquear franjas horarias para una asignatura específica.

## Cambios principales

- Se crea la entidad `BloqueoFranjaAsignatura`.
- Se crea la tabla `bloqueos_franja_asignatura`.
- Se agregan DTOs para crear y consultar bloqueos.
- Se agrega la interfaz `IBloqueoFranjaAsignaturaService`.
- Se agrega el servicio `BloqueoFranjaAsignaturaService`.
- Se agrega el controlador `BloqueosFranjaAsignaturaController`.
- Se agregan endpoints para crear, consultar y eliminar bloqueos.
- Se integra la validación con asignaciones manuales.
- Se integra la validación con ajustes manuales.
- Se integra la validación con el generador automático.
- Se integra la validación con el reporte de conflictos.
- Se actualiza `README.md`.
- Se agregan pruebas automatizadas.

## Integración con Excel

El Excel sigue siendo la fuente para cargar disponibilidad docente.  
El Req 36 no modifica el Excel, sino que agrega una restricción adicional en SQLite.

Flujo:

```txt
Disponibilidad desde Excel
+ Bloqueos de franja por asignatura
+ Carga contractual
+ Cruces de horario
+ Reglas TAPSI
= Generación y validación de horarios
```

## Pruebas realizadas

```powershell
dotnet restore .\src\ApplicationSchedule.Api\ApplicationSchedule.Api.csproj
dotnet build .\src\ApplicationSchedule.Api\ApplicationSchedule.Api.csproj
dotnet test .\src\ApplicationSchedule.Tests\ApplicationSchedule.Tests.csproj
```

## Resultado esperado

- La API compila correctamente.
- Las pruebas pasan correctamente.
- El coordinador puede crear bloqueos de franja.
- El generador automático evita franjas bloqueadas.
- Las asignaciones manuales dentro de una franja bloqueada son rechazadas.
- El reporte de conflictos detecta asignaciones existentes en franjas bloqueadas.

## Issue relacionado

Closes #36
```

---

## 23. Cierre del requerimiento

Para cerrar el Req 36:

1. Confirmar que `dotnet build` funciona correctamente.
2. Confirmar que `dotnet test` funciona correctamente.
3. Confirmar pruebas manuales en Swagger o Postman.
4. Hacer commit en la rama feature.
5. Subir la rama a GitHub.
6. Crear Pull Request hacia `develop`.
7. Revisar pestaña `Files changed`.
8. Verificar que no se suban archivos innecesarios.
9. Hacer merge del Pull Request.
10. Mover el issue o tarjeta del proyecto a `Done`.

Comentario sugerido para cerrar el issue:

```md
Req 36 completado.

Se implementó el bloqueo de franjas horarias por asignatura, incluyendo:

- Persistencia en SQLite.
- Endpoints de creación, consulta y eliminación.
- Validación en asignaciones manuales.
- Validación en generación automática de horarios.
- Detección en reporte de conflictos.
- Pruebas ejecutadas correctamente.
- README actualizado.
```

---

## 24. Archivos que no se deben subir

Evitar subir archivos generados localmente:

```txt
bin/
obj/
.vs/
*.db
*.db-shm
*.db-wal
```

Estos archivos deben estar ignorados en `.gitignore`.

---

## 25. Estado actual del backend

El backend cuenta con funcionalidades para:

```txt
- Autenticación.
- Gestión de usuarios.
- Gestión de docentes.
- Gestión de asignaturas.
- Importación desde Excel.
- Gestión de disponibilidad.
- Gestión de currículo docente.
- Asignación manual.
- Generación automática.
- Validación de conflictos.
- Materias fijas TAPSI.
- Homologaciones TAPSI.
- Exportación de horarios.
- Historial de asignaciones.
- Bloqueo de franjas por asignatura.
```

---

## 26. Responsabilidad del backend y frontend

Este backend está diseñado para integrarse con el frontend de escritorio del proyecto.

Responsabilidad del backend:

```txt
- Exponer endpoints.
- Aplicar reglas de negocio.
- Persistir datos.
- Validar restricciones.
- Generar propuestas.
- Detectar conflictos.
- Exportar información.
```

Responsabilidad del frontend:

```txt
- Mostrar pantallas.
- Consumir endpoints.
- Permitir interacción visual del coordinador.
- Presentar calendarios, tablas y formularios.
```

---

## 27. Checklist final

```txt
[ ] dotnet restore ejecutado correctamente
[ ] dotnet build ejecutado correctamente
[ ] dotnet test ejecutado correctamente
[ ] API ejecuta correctamente
[ ] Swagger abre correctamente
[ ] README.md actualizado
[ ] Req 36 probado manualmente
[ ] Req 36 probado automáticamente
[ ] Rama feature creada
[ ] Commit realizado con prefijo feat
[ ] Push origin realizado
[ ] Pull Request creado hacia develop
[ ] PR contiene Closes #36
[ ] Files changed revisados
[ ] PR mergeado
[ ] Issue o tarjeta Req 36 cerrada/movida a Done
```
