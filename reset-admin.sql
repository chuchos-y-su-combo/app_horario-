-- reset-admin.sql
-- ─────────────────────────────────────────────────────────────────────────────
-- Restablece las credenciales del usuario administrador directamente en SQLite.
--
-- Uso (con sqlite3 CLI, desde la carpeta que contiene horarios.db):
--   sqlite3 horarios.db < reset-admin.sql
--
-- O con DB Browser for SQLite: abrir el archivo y ejecutar este script.
--
-- Credenciales resultantes:
--   Correo     : admin@universidad.edu
--   Contraseña : admin123
--
-- Hash BCrypt de "admin123" con cost=11:
--   $2a$11$hx92ucPuKW8dEX/Zc3/XqeEhVT8QG.iUVbIIscRANbx7Ynr5PZy7m
-- ─────────────────────────────────────────────────────────────────────────────

-- 1. Actualizar si ya existe
UPDATE usuarios
SET password_hash   = '$2a$11$hx92ucPuKW8dEX/Zc3/XqeEhVT8QG.iUVbIIscRANbx7Ynr5PZy7m',
    correo          = 'admin@universidad.edu',
    nombre_completo = 'Administrador Principal'
WHERE id_usuario = '11111111-1111-1111-1111-111111111111';

-- 2. Insertar si no existía (instalación corrupta o DB nueva sin seed)
INSERT OR IGNORE INTO usuarios (id_usuario, id_rol, correo, password_hash, nombre_completo)
VALUES (
    '11111111-1111-1111-1111-111111111111',
    1,
    'admin@universidad.edu',
    '$2a$11$hx92ucPuKW8dEX/Zc3/XqeEhVT8QG.iUVbIIscRANbx7Ynr5PZy7m',
    'Administrador Principal'
);

SELECT 'Admin restablecido. Credenciales: admin@universidad.edu / admin123' AS resultado;
