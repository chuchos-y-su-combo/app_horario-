## Descripción

Se implementa el módulo inicial de Seguridad correspondiente al Sprint 1.

## Requerimiento trabajado

- El sistema debe permitir crear y gestionar cuentas con rol administrador o coordinador.

## Cambios realizados

- Se crea la estructura base del backend en C#.
- Se agregan las capas Api, Application, Domain e Infrastructure.
- Se implementan las entidades Rol y Usuario según el modelo relacional.
- Se configura AppDbContext para MySQL.
- Se agregan DTOs para crear, actualizar, cambiar contraseña y responder usuarios.
- Se implementa servicio de usuarios con validaciones.
- Se implementa controlador UsuariosController.
- Se agrega script SQL inicial para Roles y Usuarios.
- Se configuran roles base: Administrador y Coordinador.
- Se protegen contraseñas mediante hashing con BCrypt.

## Endpoints incluidos

- GET /api/usuarios
- GET /api/usuarios/{idUsuario}
- POST /api/usuarios
- PUT /api/usuarios/{idUsuario}
- PATCH /api/usuarios/{idUsuario}/password
- DELETE /api/usuarios/{idUsuario}



## Notas para el equipo

El frontend podrá consumir el módulo desde los endpoints REST locales.  
Los compañeros pueden agregar sus módulos de Docentes y Asignaturas reutilizando la misma estructura por capas.
