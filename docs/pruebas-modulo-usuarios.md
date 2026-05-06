# Guía de pruebas - Módulo de Usuarios y Roles

## 1. Información general del módulo

Este documento describe cómo probar el módulo de **Seguridad - Gestión de Usuarios y Roles** del sistema de organización de horarios universitarios.

El módulo implementado corresponde al siguiente requerimiento del Sprint:

> El sistema debe permitir crear y gestionar cuentas con rol administrador o coordinador.

El módulo permite:

- Crear usuarios.
- Consultar todos los usuarios.
- Consultar un usuario por ID.
- Actualizar datos básicos de un usuario.
- Cambiar la contraseña de un usuario.
- Eliminar usuarios.
- Validar que el correo sea único.
- Validar que el rol asignado exista.
- Almacenar contraseñas de forma segura mediante hash.

---

## 2. Tecnologías usadas

El módulo fue desarrollado con:

- C#
- ASP.NET Core Web API
- Entity Framework Core
- MySQL
- Pomelo.EntityFrameworkCore.MySql
- BCrypt.Net-Next

---

## 3. Estructura del módulo

La estructura utilizada para este módulo es por capas:

```txt
src/
├── ApplicationSchedule.Api/
│   ├── Controllers/
│   │   └── UsuariosController.cs
│   ├── Program.cs
│   └── appsettings.json
│
├── ApplicationSchedule.Application/
│   ├── DTOs/
│   │   └── Usuarios/
│   │       ├── CrearUsuarioRequest.cs
│   │       ├── ActualizarUsuarioRequest.cs
│   │       ├── CambiarPasswordRequest.cs
│   │       └── UsuarioResponse.cs
│   └── Interfaces/
│       └── IUsuarioService.cs
│
├── ApplicationSchedule.Domain/
│   └── Entities/
│       ├── Rol.cs
│       └── Usuario.cs
│
└── ApplicationSchedule.Infrastructure/
    ├── Data/
    │   └── AppDbContext.cs
    └── Services/
        └── UsuarioService.cs
```

---

## 4. Tablas utilizadas

El módulo trabaja con las siguientes tablas del modelo relacional.

### Tabla `Roles`

| Campo | Tipo | Descripción |
|---|---|---|
| id_rol | INT | Identificador único del rol |
| nombre_rol | VARCHAR(50) | Nombre del rol |

Roles iniciales:

| id_rol | nombre_rol |
|---|---|
| 1 | Administrador |
| 2 | Coordinador |

---

### Tabla `Usuarios`

| Campo | Tipo | Descripción |
|---|---|---|
| id_usuario | CHAR(36) | Identificador único del usuario |
| id_rol | INT | Llave foránea hacia la tabla Roles |
| correo | VARCHAR(100) | Correo único del usuario |
| password_hash | VARCHAR(255) | Contraseña almacenada mediante hash |
| nombre_completo | VARCHAR(150) | Nombre completo del usuario |

---

## 5. Requisitos previos para probar

Antes de probar el módulo, se debe tener instalado:

- .NET SDK.
- MySQL Server.
- Visual Studio, Visual Studio Code o Rider.
- Git.
- GitHub Desktop, opcional.
- Postman, Insomnia o Swagger.

---

## 6. Configuración de base de datos

En el archivo:

```txt
src/ApplicationSchedule.Api/appsettings.json
```

Debe existir una cadena de conexión parecida a esta:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;port=3306;database=horario_universidad;user=root;password=CAMBIAR_PASSWORD;"
  },
  "AllowedHosts": "*"
}
```

Cambiar:

```txt
CAMBIAR_PASSWORD
```

por la contraseña real del usuario MySQL local.

---

## 7. Ejecución del proyecto

Desde la raíz del proyecto, ejecutar:

```bash
dotnet restore
```

Luego:

```bash
dotnet build
```

Después ejecutar la API:

```bash
dotnet run --project src/ApplicationSchedule.Api
```

Si todo está correcto, la API debe iniciar localmente.

El puerto puede variar dependiendo de la configuración del archivo:

```txt
src/ApplicationSchedule.Api/Properties/launchSettings.json
```

Ejemplos posibles:

```txt
http://localhost:5000
https://localhost:7000
http://localhost:5186
https://localhost:7186
```

---

## 8. Acceso a Swagger

Cuando la API esté corriendo, abrir en el navegador una URL parecida a:

```txt
https://localhost:PUERTO/swagger
```

o:

```txt
http://localhost:PUERTO/swagger
```

Ejemplo:

```txt
https://localhost:7186/swagger
```

Desde Swagger se pueden probar todos los endpoints del módulo.

---

## 9. Endpoints disponibles

| Método | Endpoint | Descripción |
|---|---|---|
| GET | `/api/usuarios` | Lista todos los usuarios |
| GET | `/api/usuarios/{idUsuario}` | Consulta un usuario por ID |
| POST | `/api/usuarios` | Crea un usuario |
| PUT | `/api/usuarios/{idUsuario}` | Actualiza un usuario |
| PATCH | `/api/usuarios/{idUsuario}/password` | Cambia la contraseña |
| DELETE | `/api/usuarios/{idUsuario}` | Elimina un usuario |

---

# 10. Casos de prueba

---

## Caso de prueba 1: Crear usuario administrador

### Objetivo

Validar que el sistema permita crear una cuenta con rol Administrador.

### Método

```txt
POST
```

### Endpoint

```txt
/api/usuarios
```

### Body

```json
{
  "nombreCompleto": "Administrador Principal",
  "correo": "admin@universidad.edu",
  "password": "Admin12345",
  "idRol": 1
}
```

### Resultado esperado

Código HTTP esperado:

```txt
201 Created
```

La respuesta debe devolver un usuario creado con esta estructura:

```json
{
  "idUsuario": "guid-generado",
  "nombreCompleto": "Administrador Principal",
  "correo": "admin@universidad.edu",
  "idRol": 1,
  "nombreRol": "Administrador"
}
```

### Validaciones importantes

- El usuario debe quedar guardado en la base de datos.
- El correo debe quedar en minúsculas.
- La respuesta no debe mostrar la contraseña.
- La respuesta no debe mostrar `passwordHash`.
- En la base de datos, la contraseña debe estar guardada como hash.

---

## Caso de prueba 2: Crear usuario coordinador

### Objetivo

Validar que el sistema permita crear una cuenta con rol Coordinador.

### Método

```txt
POST
```

### Endpoint

```txt
/api/usuarios
```

### Body

```json
{
  "nombreCompleto": "Coordinador Académico",
  "correo": "coordinador@universidad.edu",
  "password": "Coord12345",
  "idRol": 2
}
```

### Resultado esperado

Código HTTP esperado:

```txt
201 Created
```

Respuesta esperada:

```json
{
  "idUsuario": "guid-generado",
  "nombreCompleto": "Coordinador Académico",
  "correo": "coordinador@universidad.edu",
  "idRol": 2,
  "nombreRol": "Coordinador"
}
```

### Validaciones importantes

- El usuario debe quedar asociado al rol Coordinador.
- No se debe devolver la contraseña.
- No se debe devolver el hash de la contraseña.

---

## Caso de prueba 3: Listar usuarios

### Objetivo

Validar que el sistema permita consultar todos los usuarios registrados.

### Método

```txt
GET
```

### Endpoint

```txt
/api/usuarios
```

### Body

No aplica.

### Resultado esperado

Código HTTP esperado:

```txt
200 OK
```

Respuesta esperada:

```json
[
  {
    "idUsuario": "guid-generado",
    "nombreCompleto": "Administrador Principal",
    "correo": "admin@universidad.edu",
    "idRol": 1,
    "nombreRol": "Administrador"
  },
  {
    "idUsuario": "guid-generado",
    "nombreCompleto": "Coordinador Académico",
    "correo": "coordinador@universidad.edu",
    "idRol": 2,
    "nombreRol": "Coordinador"
  }
]
```

### Validaciones importantes

- Debe devolver una lista.
- Cada usuario debe tener su rol correspondiente.
- No debe aparecer `password`.
- No debe aparecer `passwordHash`.

---

## Caso de prueba 4: Consultar usuario por ID existente

### Objetivo

Validar que el sistema permita consultar un usuario específico mediante su ID.

### Método

```txt
GET
```

### Endpoint

```txt
/api/usuarios/{idUsuario}
```

### Ejemplo

```txt
/api/usuarios/1f6f40c9-95d1-4dc7-ae95-c820d70f1c23
```

### Resultado esperado

Código HTTP esperado:

```txt
200 OK
```

Respuesta esperada:

```json
{
  "idUsuario": "1f6f40c9-95d1-4dc7-ae95-c820d70f1c23",
  "nombreCompleto": "Administrador Principal",
  "correo": "admin@universidad.edu",
  "idRol": 1,
  "nombreRol": "Administrador"
}
```

### Validaciones importantes

- Debe devolver solo el usuario solicitado.
- No debe devolver contraseña.
- No debe devolver `passwordHash`.

---

## Caso de prueba 5: Consultar usuario por ID inexistente

### Objetivo

Validar que el sistema responda correctamente cuando se consulta un usuario que no existe.

### Método

```txt
GET
```

### Endpoint

```txt
/api/usuarios/{idUsuario}
```

### Ejemplo

```txt
/api/usuarios/00000000-0000-0000-0000-000000000000
```

### Resultado esperado

Código HTTP esperado:

```txt
404 Not Found
```

Respuesta esperada:

```json
{
  "mensaje": "Usuario no encontrado."
}
```

---

## Caso de prueba 6: Actualizar usuario existente

### Objetivo

Validar que el sistema permita actualizar los datos básicos de un usuario.

### Método

```txt
PUT
```

### Endpoint

```txt
/api/usuarios/{idUsuario}
```

### Body

```json
{
  "nombreCompleto": "Coordinador Académico Actualizado",
  "correo": "coordinador.actualizado@universidad.edu",
  "idRol": 2
}
```

### Resultado esperado

Código HTTP esperado:

```txt
204 No Content
```

### Validaciones importantes

Después de actualizar, consultar el usuario con:

```txt
GET /api/usuarios/{idUsuario}
```

El usuario debe mostrar los nuevos datos:

```json
{
  "idUsuario": "guid-del-usuario",
  "nombreCompleto": "Coordinador Académico Actualizado",
  "correo": "coordinador.actualizado@universidad.edu",
  "idRol": 2,
  "nombreRol": "Coordinador"
}
```

---

## Caso de prueba 7: Actualizar usuario inexistente

### Objetivo

Validar que el sistema responda correctamente cuando se intenta actualizar un usuario inexistente.

### Método

```txt
PUT
```

### Endpoint

```txt
/api/usuarios/00000000-0000-0000-0000-000000000000
```

### Body

```json
{
  "nombreCompleto": "Usuario Inexistente",
  "correo": "inexistente@universidad.edu",
  "idRol": 1
}
```

### Resultado esperado

Código HTTP esperado:

```txt
404 Not Found
```

Respuesta esperada:

```json
{
  "mensaje": "Usuario no encontrado."
}
```

---

## Caso de prueba 8: Cambiar contraseña

### Objetivo

Validar que el sistema permita cambiar la contraseña de un usuario existente.

### Método

```txt
PATCH
```

### Endpoint

```txt
/api/usuarios/{idUsuario}/password
```

### Body

```json
{
  "nuevaPassword": "NuevaClave123"
}
```

### Resultado esperado

Código HTTP esperado:

```txt
204 No Content
```

### Validaciones importantes

- La contraseña debe cambiarse correctamente.
- La nueva contraseña debe guardarse como hash en la base de datos.
- La API no debe devolver la contraseña.
- La API no debe devolver el hash.

---

## Caso de prueba 9: Cambiar contraseña de usuario inexistente

### Objetivo

Validar que el sistema responda correctamente cuando se intenta cambiar la contraseña de un usuario inexistente.

### Método

```txt
PATCH
```

### Endpoint

```txt
/api/usuarios/00000000-0000-0000-0000-000000000000/password
```

### Body

```json
{
  "nuevaPassword": "NuevaClave123"
}
```

### Resultado esperado

Código HTTP esperado:

```txt
404 Not Found
```

Respuesta esperada:

```json
{
  "mensaje": "Usuario no encontrado."
}
```

---

## Caso de prueba 10: Eliminar usuario existente

### Objetivo

Validar que el sistema permita eliminar un usuario existente.

### Método

```txt
DELETE
```

### Endpoint

```txt
/api/usuarios/{idUsuario}
```

### Resultado esperado

Código HTTP esperado:

```txt
204 No Content
```

### Validaciones importantes

Después de eliminar, consultar el usuario con:

```txt
GET /api/usuarios/{idUsuario}
```

Debe responder:

```txt
404 Not Found
```

---

## Caso de prueba 11: Eliminar usuario inexistente

### Objetivo

Validar que el sistema responda correctamente cuando se intenta eliminar un usuario que no existe.

### Método

```txt
DELETE
```

### Endpoint

```txt
/api/usuarios/00000000-0000-0000-0000-000000000000
```

### Resultado esperado

Código HTTP esperado:

```txt
404 Not Found
```

Respuesta esperada:

```json
{
  "mensaje": "Usuario no encontrado."
}
```

---

# 11. Casos de validación

---

## Validación 1: Correo duplicado

### Objetivo

Validar que no se puedan crear dos usuarios con el mismo correo.

### Pasos

1. Crear un usuario con este correo:

```txt
admin@universidad.edu
```

2. Intentar crear otro usuario con el mismo correo.

### Body

```json
{
  "nombreCompleto": "Otro Administrador",
  "correo": "admin@universidad.edu",
  "password": "Admin12345",
  "idRol": 1
}
```

### Resultado esperado

Código HTTP esperado:

```txt
400 Bad Request
```

Respuesta esperada:

```json
{
  "mensaje": "Ya existe un usuario registrado con ese correo."
}
```

---

## Validación 2: Rol inexistente

### Objetivo

Validar que no se pueda crear un usuario con un rol que no existe.

### Método

```txt
POST
```

### Endpoint

```txt
/api/usuarios
```

### Body

```json
{
  "nombreCompleto": "Usuario Sin Rol",
  "correo": "sinrol@universidad.edu",
  "password": "Usuario123",
  "idRol": 99
}
```

### Resultado esperado

Código HTTP esperado:

```txt
400 Bad Request
```

Respuesta esperada:

```json
{
  "mensaje": "El rol seleccionado no existe."
}
```

---

## Validación 3: Correo inválido

### Objetivo

Validar que el sistema rechace correos con formato incorrecto.

### Método

```txt
POST
```

### Endpoint

```txt
/api/usuarios
```

### Body

```json
{
  "nombreCompleto": "Usuario Correo Malo",
  "correo": "correo-invalido",
  "password": "Usuario123",
  "idRol": 1
}
```

### Resultado esperado

Código HTTP esperado:

```txt
400 Bad Request
```

La respuesta debe indicar que el correo no tiene un formato válido.

---

## Validación 4: Contraseña con menos de 8 caracteres

### Objetivo

Validar que el sistema rechace contraseñas demasiado cortas.

### Método

```txt
POST
```

### Endpoint

```txt
/api/usuarios
```

### Body

```json
{
  "nombreCompleto": "Usuario Password Corta",
  "correo": "passwordcorta@universidad.edu",
  "password": "123",
  "idRol": 1
}
```

### Resultado esperado

Código HTTP esperado:

```txt
400 Bad Request
```

La respuesta debe indicar que la contraseña debe tener mínimo 8 caracteres.

---

## Validación 5: Nombre completo vacío

### Objetivo

Validar que el sistema no permita crear usuarios sin nombre completo.

### Método

```txt
POST
```

### Endpoint

```txt
/api/usuarios
```

### Body

```json
{
  "nombreCompleto": "",
  "correo": "sinnombre@universidad.edu",
  "password": "Usuario123",
  "idRol": 1
}
```

### Resultado esperado

Código HTTP esperado:

```txt
400 Bad Request
```

La respuesta debe indicar que el nombre completo es obligatorio.

---

# 12. Validaciones de seguridad

Durante las pruebas se debe verificar especialmente lo siguiente:

| Validación | Resultado esperado |
|---|---|
| La contraseña no se devuelve al crear usuario | Correcto |
| La contraseña no se devuelve al listar usuarios | Correcto |
| La contraseña no se devuelve al consultar usuario por ID | Correcto |
| El campo `passwordHash` no se expone en la API | Correcto |
| La contraseña se almacena como hash en MySQL | Correcto |
| No se permite correo duplicado | Correcto |
| No se permite rol inexistente | Correcto |

---

# 13. Verificación en base de datos

Para verificar los datos directamente en MySQL, se pueden usar estas consultas.

## Consultar roles

```sql
SELECT * FROM Roles;
```

Resultado esperado:

```txt
1 | Administrador
2 | Coordinador
```

---

## Consultar usuarios

```sql
SELECT id_usuario, id_rol, correo, password_hash, nombre_completo
FROM Usuarios;
```

### Resultado esperado

Debe aparecer información parecida a:

```txt
id_usuario                            | id_rol | correo                       | password_hash | nombre_completo
--------------------------------------|--------|------------------------------|---------------|-------------------------
guid-generado                         | 1      | admin@universidad.edu        | $2a$...       | Administrador Principal
guid-generado                         | 2      | coordinador@universidad.edu  | $2a$...       | Coordinador Académico
```

El campo `password_hash` no debe mostrar la contraseña original.

Incorrecto:

```txt
Admin12345
```

Correcto:

```txt
$2a$11$...
```

---

# 14. Checklist general de pruebas

Marcar cada punto cuando esté validado.

## Ejecución

- [ ] El proyecto compila correctamente con `dotnet build`.
- [ ] La API ejecuta correctamente con `dotnet run --project src/ApplicationSchedule.Api`.
- [ ] Swagger abre correctamente.
- [ ] La conexión con MySQL funciona.

## Roles

- [ ] Existe el rol Administrador.
- [ ] Existe el rol Coordinador.
- [ ] No se puede crear usuario con rol inexistente.

## Usuarios

- [ ] Se puede crear usuario Administrador.
- [ ] Se puede crear usuario Coordinador.
- [ ] Se pueden listar usuarios.
- [ ] Se puede consultar usuario por ID.
- [ ] Se puede actualizar usuario.
- [ ] Se puede cambiar contraseña.
- [ ] Se puede eliminar usuario.

## Validaciones

- [ ] No permite correo duplicado.
- [ ] No permite correo inválido.
- [ ] No permite contraseña menor a 8 caracteres.
- [ ] No permite nombre completo vacío.
- [ ] Devuelve 404 cuando el usuario no existe.

## Seguridad

- [ ] No se devuelve la contraseña en ninguna respuesta.
- [ ] No se devuelve `passwordHash` en ninguna respuesta.
- [ ] La contraseña se almacena como hash en la base de datos.

---

# 15. Observaciones para el tester

Este módulo no incluye todavía autenticación por login ni generación de token JWT.

El objetivo del Sprint actual es únicamente permitir la gestión de cuentas con rol Administrador o Coordinador.

Por lo tanto, las pruebas deben enfocarse en:

- CRUD de usuarios.
- Asociación de usuarios con roles.
- Validación de datos.
- Protección básica de contraseñas mediante hash.
- Correcta respuesta de los endpoints REST.

---

# 16. Estado esperado del módulo

El módulo se considera aprobado si:

- La API ejecuta localmente sin errores.
- Los endpoints responden correctamente.
- Los usuarios se guardan correctamente en MySQL.
- Los roles Administrador y Coordinador funcionan.
- No se expone información sensible.
- Los casos de error devuelven respuestas adecuadas.
