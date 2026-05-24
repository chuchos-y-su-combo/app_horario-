-- =========================================================
-- REQ 9 - GENERACIÓN AUTOMÁTICA DE PROPUESTAS POR ESCENARIO
-- SQLite
-- =========================================================

PRAGMA foreign_keys = ON;

-- =========================================================
-- Agregar escenario a las asignaciones
-- =========================================================
-- Ejecutar solo si la columna todavía no existe.
-- Si SQLite dice "duplicate column name", significa que ya estaba creada.

ALTER TABLE asignaciones
ADD COLUMN escenario TEXT NOT NULL DEFAULT 'ING_DIURNA'
CHECK (escenario IN (
    'ING_DIURNA',
    'ING_NOCTURNA',
    'TAPSI_DIURNA',
    'TAPSI_NOCTURNA'
));