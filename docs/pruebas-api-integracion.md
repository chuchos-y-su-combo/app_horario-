# Documentación de Pruebas de Integración de la API

## Objetivo

Esta suite valida la API de **ApplicationSchedule** a nivel funcional e integrado, cubriendo los controladores existentes, sus rutas HTTP, reglas de negocio visibles desde API y respuestas esperadas.

La documentación resume la cobertura de pruebas automatizadas implementada en `src/ApplicationSchedule.Tests`.

## Alcance

La suite actual cubre:

- `UsuariosController`
- `AsignaturasController`
- `ProfesoresController`

En total se validan:

- 17 endpoints HTTP
- 55 casos de prueba ejecutados
- Escenarios de éxito, error, validación y consistencia de respuestas

## Stack de pruebas

- Framework de pruebas: xUnit
- Aserciones: FluentAssertions
- Integración HTTP: `WebApplicationFactory`
- Base de datos de pruebas: Entity Framework Core InMemory
- Aislamiento: una base de datos en memoria por ejecución de test

## Arquitectura de pruebas

### Infraestructura base

Las pruebas de integración usan los siguientes componentes:

- [CustomWebApplicationFactory](../src/ApplicationSchedule.Tests/Infrastructure/CustomWebApplicationFactory.cs)
- [IntegrationTestBase](../src/ApplicationSchedule.Tests/Infrastructure/IntegrationTestBase.cs)

### Qué hace `CustomWebApplicationFactory`

- Arranca la API en memoria sin depender de un servidor externo
- Reemplaza el contexto real por una base de datos `InMemory`
- Siembra roles base al iniciar
- Limpia usuarios, profesores y asignaturas después de cada test
- Evita interferencias entre pruebas

### Qué hace `IntegrationTestBase`

- Crea el `HttpClient` por prueba
- Inicializa la base de datos antes de cada caso
- Garantiza aislamiento entre escenarios

## Endpoints cubiertos

### Usuarios

Ruta base: `api/usuarios`

- `GET /api/usuarios`
- `GET /api/usuarios/{idUsuario}`
- `POST /api/usuarios`
- `PUT /api/usuarios/{idUsuario}`
- `PATCH /api/usuarios/{idUsuario}/password`
- `DELETE /api/usuarios/{idUsuario}`

### Asignaturas

Ruta base: `api/asignaturas`

- `GET /api/asignaturas`
- `GET /api/asignaturas/plan/{idPlanEstudios}`
- `GET /api/asignaturas/{idAsignatura}`
- `POST /api/asignaturas`
- `PUT /api/asignaturas/{idAsignatura}`
- `DELETE /api/asignaturas/{idAsignatura}`

### Profesores

Ruta base: `api/profesores`

- `GET /api/profesores`
- `GET /api/profesores/{idProfesor}`
- `POST /api/profesores`
- `PUT /api/profesores/{idProfesor}`
- `DELETE /api/profesores/{idProfesor}`

## Cobertura por controlador

### 1. Usuarios

Archivo: [UsuariosControllerTests.cs](../src/ApplicationSchedule.Tests/Controllers/UsuariosControllerTests.cs)

Casos cubiertos:

- Lista vacía cuando no existen usuarios
- Listado con múltiples usuarios y ordenación por `NombreCompleto`
- Obtener usuario por ID existente
- Obtener usuario por ID inexistente
- Crear usuario con datos válidos
- Rechazar correo duplicado
- Rechazar rol inexistente
- Validaciones de entrada en `CrearUsuarioRequest`
- Normalización del correo a minúsculas
- Actualizar usuario exitosamente
- Actualizar usuario inexistente
- Rechazar correo duplicado al actualizar
- Cambiar contraseña exitosamente
- Cambiar contraseña de usuario inexistente
- Eliminar usuario exitosamente
- Eliminar usuario inexistente

Validaciones comprobadas:

- `200 OK`
- `201 Created`
- `204 NoContent`
- `400 BadRequest`
- `404 NotFound`

### 2. Asignaturas

Archivo: [AsignaturasControllerTests.cs](../src/ApplicationSchedule.Tests/Controllers/AsignaturasControllerTests.cs)

Casos cubiertos:

- Lista vacía inicial
- Listado ordenado por semestre y nombre
- Obtener asignaturas por plan de estudios existente
- Obtener asignaturas por plan sin datos
- Obtener asignatura por ID existente
- Obtener asignatura por ID inexistente
- Crear asignatura válida
- Normalización de código a mayúsculas
- Rechazar código duplicado
- Validar rangos de créditos y semestre
- Actualizar asignatura exitosamente
- Actualizar asignatura inexistente
- Rechazar código duplicado al actualizar
- Eliminar asignatura exitosamente
- Eliminar asignatura inexistente

Validaciones comprobadas:

- `200 OK`
- `201 Created`
- `204 NoContent`
- `400 BadRequest`
- `404 NotFound`

### 3. Profesores

Archivo: [ProfesoresControllerTests.cs](../src/ApplicationSchedule.Tests/Controllers/ProfesoresControllerTests.cs)

Casos cubiertos:

- Lista vacía inicial
- Listado de profesores existentes
- Obtener profesor por ID existente
- Obtener profesor por ID inexistente
- Crear profesor válido
- Rechazar identificación duplicada
- Validaciones de entrada en creación
- Validación de tipos de contrato permitidos
- Actualizar profesor exitosamente
- Actualizar profesor inexistente
- Rechazar identificación duplicada al actualizar
- Permitir mantener la misma identificación al actualizar otros campos
- Eliminar profesor exitosamente
- Eliminar profesor inexistente

Validaciones comprobadas:

- `200 OK`
- `201 Created`
- `204 NoContent`
- `400 BadRequest`
- `404 NotFound`

## Resumen de casos ejecutados

La suite completa ejecuta **55 casos**.

Desglose:

- Usuarios: 16 métodos de prueba / 18 casos ejecutados contando teorías
- Asignaturas: 15 métodos de prueba / 18 casos ejecutados contando teorías
- Profesores: 14 métodos de prueba / 19 casos ejecutados contando teorías

## Tipos de pruebas aplicadas

### Casos de éxito

Se valida el flujo normal de:

- Listado
- Consulta por ID
- Creación
- Actualización
- Eliminación
- Cambio de contraseña en usuarios

### Casos de error

Se valida el manejo de:

- IDs inexistentes
- Valores duplicados
- Registros no encontrados
- Rangos inválidos
- Formatos inválidos
- Reglas de negocio de integridad

### Validaciones de entrada

Se cubren campos obligatorios, rangos y formatos, especialmente en:

- `CrearUsuarioRequest`
- `ActualizarUsuarioRequest`
- `CrearAsignaturaRequest`
- `ActualizarAsignaturaRequest`
- `CrearProfesorRequest`
- `ActualizarProfesorRequest`

## Respuestas JSON verificadas

Las pruebas validan que la API devuelva estructuras JSON consistentes para:

- DTOs de respuesta
- Errores de negocio (`{ mensaje: "..." }`)
- Errores de validación del framework (`ValidationProblemDetails`)

## Hallazgos estructurales documentados

Durante la revisión se identificó y corrigió lo siguiente:

- Faltaban validaciones en DTOs de profesores
- La compatibilidad de paquetes EF Core estaba desalineada entre proyecto API, infraestructura y tests
- El proyecto de tests inicial incluía un archivo plantilla que rompía la compilación

## Ejecución local

Desde la carpeta `src/ApplicationSchedule.Tests`:

```bash
dotnet restore
dotnet test --no-restore
```

## Recomendaciones

- Mantener esta suite como base para cualquier cambio en controladores o servicios
- Agregar tests de autorización cuando exista un esquema de autenticación real
- Incluir tests de contrato si la API crece con más consumidores externos

## Estado actual

- Cobertura funcional principal: implementada
- Cobertura de integración: implementada
- Ejecución validada: correcta
- Resultado de la última corrida: **55/55 tests aprobados**
