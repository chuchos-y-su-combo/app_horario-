-- ==========================================
-- REQ 36 - Bloqueo de franjas horarias por asignatura
-- SQLite
-- ==========================================

PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS bloqueos_franja_asignatura (
    id_bloqueo TEXT PRIMARY KEY,
    id_asignatura TEXT NOT NULL,
    periodo TEXT NOT NULL,
    dia INTEGER NOT NULL CHECK (dia BETWEEN 1 AND 6),
    hora_inicio TEXT NOT NULL,
    hora_fin TEXT NOT NULL,
    motivo TEXT NULL,
    fecha_creacion_utc TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (id_asignatura) REFERENCES asignaturas(id_asignatura) ON DELETE CASCADE
);

CREATE UNIQUE INDEX IF NOT EXISTS IX_bloqueos_franja_asignatura_unico
ON bloqueos_franja_asignatura (id_asignatura, periodo, dia, hora_inicio, hora_fin);