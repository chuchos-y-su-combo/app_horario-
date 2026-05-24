-- =========================================================
-- REQ 8 - TAPSI DIURNA + DISPONIBILIDAD DOCENTE
-- Base de datos: SQLite
-- =========================================================

PRAGMA foreign_keys = ON;

-- =========================================================
-- 1. Columna para marcar asignaturas adicionales TAPSI diurna
-- =========================================================
-- IMPORTANTE:
-- Ejecutar solo si la columna todavía NO existe.
-- Si SQLite dice "duplicate column name", significa que ya estaba creada.

ALTER TABLE asignaturas
ADD COLUMN es_opcional_tapsi_diurna INTEGER NOT NULL DEFAULT 0
CHECK (es_opcional_tapsi_diurna IN (0, 1));

-- =========================================================
-- 2. Marcar las opciones adicionales TAPSI diurna
-- =========================================================

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

-- =========================================================
-- 3. Tabla de disponibilidad docente
-- =========================================================
-- Si la tabla ya existe, no la vuelve a crear.

CREATE TABLE IF NOT EXISTS disponibilidad (
    id_disponibilidad TEXT PRIMARY KEY,
    id_docente TEXT NOT NULL,
    dia_semana INTEGER NOT NULL CHECK (dia_semana BETWEEN 1 AND 6),
    hora_inicio TEXT NOT NULL,
    hora_fin TEXT NOT NULL,
    FOREIGN KEY (id_docente) REFERENCES docentes(id_docente) ON DELETE CASCADE
);