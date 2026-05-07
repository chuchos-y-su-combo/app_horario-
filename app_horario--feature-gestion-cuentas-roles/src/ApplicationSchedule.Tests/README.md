# Guía de Pruebas Automatizadas - ApplicationSchedule.Tests

## 📋 Descripción General

Este proyecto contiene **pruebas de integración completamente aisladas y determinísticas** para la API de gestión de horarios académicos, enfocadas en el módulo de Usuarios.

- **Framework:** xUnit
- **BD Testing:** Entity Framework Core InMemory (completamente aislada por test)
- **Librerías de Assertion:** FluentAssertions
- **Patrón:** WebApplicationFactory para pruebas in-memory (sin localhost)
- **Aislamiento:** 100% garantizado - tests paralelos sin interferencias

---

## � Garantías de Aislamiento

### ✅ NO Dependencia de Datos Persistentes
- Cada test recibe su propia BD en memoria **completamente nueva**
- DbContext se crea fresco para cada operación
- NO se reutilizan datos entre tests
- `DisposeAsync()` limpia automáticamente después de cada test

### ✅ Base de Datos en Memoria Correctamente Configurada
- Usa `UseInMemoryDatabase()` con nombre **único por test** (GUID + timestamp)
- `EnsureDeleted() + EnsureCreated()` garantiza BD limpia
- Seed inicial solo de roles base (imprescindibles)
- **Aislamiento perfecto:** BD1 ≠ BD2 incluso con ejecución paralela

### ✅ Determinismo Garantizado
- **Siempre comienza igual:** Los mismos roles base (Admin + Coordinador)
- **Datos dinámicos:** Cada test genera correos únicos con `Guid.NewGuid()`
- **Reproducible:** Ejecutar 100 veces = 100 resultados idénticos
- **Tests independientes:** Pueden ejecutarse en cualquier orden

---

## �🗂️ Estructura del Proyecto

```
ApplicationSchedule.Tests/
├── Infrastructure/
│   ├── CustomWebApplicationFactory.cs    # Factory con BD en memoria única
│   │   ├── Nombre BD: TestDb_{GUID}_{Timestamp}
│   │   ├── EnsureDeleted() + EnsureCreated()
│   │   ├── SeedInitialRoles() (determinístico)
│   │   └── ResetDatabaseAsync() (limpieza)
│   │
│   └── IntegrationTestBase.cs            # Clase base para tests
│       ├── IAsyncLifetime (limpieza automática)
│       ├── ExecuteDbContextAsync (DbContext nuevo cada vez)
│       └── Métodos auxiliares
│
├── Usuarios/
│   └── UsuariosApiTests.cs               # 18 tests (100% aislados)
│
├── ApplicationSchedule.Tests.csproj      # Configuración
├── README.md                             # Este archivo
└── VERIFICATION.md                       # Auditoría completa de aislamiento
```

---

## 🚀 Cómo Ejecutar los Tests

### Opción 1: Desde Visual Studio
```
1. Click derecho en el proyecto ApplicationSchedule.Tests
2. Seleccionar "Run Tests" o "Run All Tests"
```

### Opción 2: Desde VS Code / Línea de Comandos
```bash
# Ejecutar todos los tests
dotnet test

# Ejecutar con verbose
dotnet test --verbosity detailed

# Ejecutar solo el proyecto de tests
dotnet test src/ApplicationSchedule.Tests/ApplicationSchedule.Tests.csproj

# Ejecutar una clase específica
dotnet test --filter "FullyQualifiedName~UsuariosApiTests"

# Ejecutar con report de cobertura
dotnet test /p:CollectCoverage=true
```

### Opción 3: Desde GitHub Actions (CI/CD)
El pipeline ya está configurado y ejecutará automáticamente:
```bash
dotnet build --configuration Release
dotnet test --no-build --configuration Release
```

---

## 📝 Suite de Pruebas - UsuariosApiTests

### GET /api/usuarios

| Test | Escenario | Resultado Esperado |
|------|-----------|-------------------|
| `ObtenerTodos_RetornListaVacia_CuandoNoHayUsuarios` | BD vacía | 200 OK, lista vacía |
| `ObtenerTodos_RetornListaUsuarios_CuandoHayMultiplesUsuarios` | Múltiples usuarios | 200 OK, lista ordenada |

### GET /api/usuarios/{id}

| Test | Escenario | Resultado Esperado |
|------|-----------|-------------------|
| `ObtenerPorId_RetornUsuario_CuandoIdExiste` | ID válido | 200 OK, datos del usuario |
| `ObtenerPorId_Retorn404_CuandoIdNoExiste` | ID inexistente | 404 Not Found |

### POST /api/usuarios (Crear)

| Test | Escenario | Resultado Esperado |
|------|-----------|-------------------|
| `Crear_Retorn201_CuandoDatosValidos` | Datos válidos | 201 Created |
| `Crear_Retorn400_CuandoCorreoDuplicado` | Email duplicado | 400 Bad Request |
| `Crear_Retorn400_CuandoRolNoExiste` | Rol inválido | 400 Bad Request |
| `Crear_Retorn400_CuandoNombreVacio` | Nombre vacío | 400 Bad Request |
| `Crear_Retorn400_CuandoCorreoFormatoInvalido` | Email mal formado | 400 Bad Request |
| `Crear_Retorn400_CuandoPasswordMenor8Caracteres` | Password muy corta | 400 Bad Request |

### PUT /api/usuarios/{id} (Actualizar)

| Test | Escenario | Resultado Esperado |
|------|-----------|-------------------|
| `Actualizar_Retorn204_CuandoDatosValidos` | Datos válidos | 204 No Content |
| `Actualizar_Retorn404_CuandoIdNoExiste` | ID inexistente | 404 Not Found |
| `Actualizar_Retorn400_CuandoCorreoDuplicado` | Email ya usado | 400 Bad Request |

### PATCH /api/usuarios/{id}/password (Cambiar Contraseña)

| Test | Escenario | Resultado Esperado |
|------|-----------|-------------------|
| `CambiarPassword_Retorn204_CuandoDatosValidos` | Datos válidos | 204 No Content |
| `CambiarPassword_Retorn404_CuandoIdNoExiste` | ID inexistente | 404 Not Found |

### DELETE /api/usuarios/{id} (Eliminar)

| Test | Escenario | Resultado Esperado |
|------|-----------|-------------------|
| `Eliminar_Retorn204_CuandoIdExiste` | ID válido | 204 No Content |
| `Eliminar_Retorn404_CuandoIdNoExiste` | ID inexistente | 404 Not Found |

### Flujo Completo

| Test | Escenario |
|------|-----------|
| `FlujoCOMPLETO_CrearActualizarEliminarUsuario` | Crear → Obtener → Actualizar → Cambiar Password → Eliminar |

---

## 🔍 Características Clave de los Tests

### ✅ Independencia Total
- Cada test crea su propia BD en memoria
- No hay datos hardcodeados
- Los datos se generan dinámicamente con `Guid.NewGuid()`
- Cada test es completamente aislado

### ✅ Datos Dinámicos
```csharp
// En lugar de hardcoded:
var correo = $"usuario_{Guid.NewGuid()}@test.com";
```

### ✅ Patrón AAA (Arrange-Act-Assert)
```csharp
// Arrange: Preparar datos
var request = new CrearUsuarioRequest { ... };

// Act: Ejecutar acción
var response = await Client.PostAsJsonAsync("/api/usuarios", request);

// Assert: Verificar resultado
response.StatusCode.Should().Be(HttpStatusCode.Created);
```

### ✅ Sin localhost
- Usa `WebApplicationFactory` que ejecuta la app en memoria
- No requiere que la API esté corriendo
- HttpClient configurado automáticamente

### ✅ Base de Datos en Memoria
- Usa Entity Framework Core InMemory
- Se crea y destruye para cada test
- Se popula con datos iniciales (roles base)

---

## 🛠️ Cómo Agregar Nuevas Pruebas

### Estructura básica:
```csharp
[Fact]
public async Task NombreDelTest_ResultadoEsperado_Condicion()
{
    // Arrange
    var request = new SomeRequest { ... };

    // Act
    var response = await Client.PostAsJsonAsync("/api/endpoint", request);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var result = await response.Content.ReadAsAsync<SomeResponse>();
    result.Should().NotBeNull();
}
```

### Acceso a la BD en test:
```csharp
// Si necesitas manipular la BD directamente
await ExecuteDbContextAction(async context =>
{
    var usuarios = await context.Usuarios.ToListAsync();
    usuarios.Should().HaveCount(1);
});
```

---

## 🔧 Troubleshooting

### Error: "No se encontró la cadena de conexión"
→ Normal y esperado, estamos usando BD en memoria. El Factory la configura automáticamente.

### Error: "The database connection is not initialized"
→ Verifica que `IntegrationTestBase` implementa `IAsyncLifetime`.

### Tests ejecutados en paralelo fallan
→ **No debería pasar.** Cada test tiene su propia BD con nombre único. Si falla, reportar el error específico.

### Un test está sucio (afecta a otros)
→ **Revisar DisposeAsync() está siendo llamado.** xUnit lo hace automáticamente con IAsyncLifetime. Si falla, hay un bug.

---

## 📊 Cobertura de Tests

Actual:
- ✅ GET /api/usuarios (2 casos)
- ✅ GET /api/usuarios/{id} (2 casos)
- ✅ POST /api/usuarios (6 casos)
- ✅ PUT /api/usuarios/{id} (3 casos)
- ✅ PATCH /api/usuarios/{id}/password (2 casos)
- ✅ DELETE /api/usuarios/{id} (2 casos)
- ✅ Flujo completo (1 caso)

**Total: 18 tests automatizados**

---

## 📈 Próximas Mejoras Recomendadas

1. **Agregar tests para otros módulos** (Docentes, Horarios)
2. **Tests de performance** con múltiples usuarios (stress testing)
3. **Tests de seguridad** (validación de autorización)
4. **Integración con SonarQube** para análisis de código
5. **Reports HTML** con cobertura detallada

---

## 📚 Documentación Adicional

- **[VERIFICATION.md](VERIFICATION.md)** - Auditoría completa de aislamiento, independencia y determinismo

---

## ✅ Verificación Rápida

Para verificar que TODO está funcionando correctamente:

```bash
# Ejecutar todos los tests
dotnet test

# Salida esperada:
# Test Run Successful.
# Total tests: 18
# Passed: 18 ✅
# Failed: 0
# Skipped: 0
```

---

## 🤝 Integración Continua

El pipeline de GitHub Actions ya ejecuta los tests automáticamente en cada:
- Push a `main` o `develop`
- Pull request hacia `main`

Ver `.github/workflows/ci.yml` para más detalles.

---

**Creado:** Proyecto de pruebas automatizadas profesional para .NET
**Mantener actualizado:** Agregar tests cuando se añadan nuevos endpoints
**Estado:** ✅ Listo para producción