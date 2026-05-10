-- ==========================================
-- BASE DE DATOS SQLITE - GESTIÓN DE HORARIOS
-- Adaptación del modelo oficial a SQLite
-- ==========================================

PRAGMA foreign_keys = ON;

-- ==========================================
-- MÓDULO: SEGURIDAD
-- ==========================================

CREATE TABLE IF NOT EXISTS roles (
    id_rol INTEGER PRIMARY KEY AUTOINCREMENT,
    nombre_rol TEXT NOT NULL UNIQUE
);

CREATE TABLE IF NOT EXISTS usuarios (
    id_usuario TEXT PRIMARY KEY,
    id_rol INTEGER NOT NULL,
    correo TEXT NOT NULL UNIQUE,
    password_hash TEXT NOT NULL,
    nombre_completo TEXT NOT NULL,
    FOREIGN KEY (id_rol) REFERENCES roles(id_rol)
);

-- ==========================================
-- MÓDULO: ACADÉMICO
-- ==========================================

CREATE TABLE IF NOT EXISTS planes_estudio (
    id_plan TEXT PRIMARY KEY,
    nombre_plan TEXT NOT NULL,
    jornada TEXT NOT NULL CHECK (jornada IN ('Diurna', 'Nocturna'))
);

CREATE TABLE IF NOT EXISTS docentes (
    id_docente TEXT PRIMARY KEY,
    identificacion TEXT NOT NULL UNIQUE,
    nombre TEXT NOT NULL,
    tipo_contrato TEXT NOT NULL CHECK (tipo_contrato IN ('TC', 'TP')),
    max_asignaturas INTEGER NOT NULL CHECK (
        (tipo_contrato = 'TC' AND max_asignaturas = 5)
        OR (tipo_contrato = 'TP' AND max_asignaturas = 3)
    )
);

CREATE TABLE IF NOT EXISTS asignaturas (
    id_asignatura TEXT PRIMARY KEY,
    id_plan TEXT NOT NULL,
    codigo TEXT NOT NULL UNIQUE,
    nombre TEXT NOT NULL,
    creditos INTEGER NOT NULL,
    semestre INTEGER NOT NULL,
    min_estudiantes INTEGER NOT NULL DEFAULT 15,
    es_fija_tapsi INTEGER NOT NULL DEFAULT 0 CHECK (es_fija_tapsi IN (0, 1)),
    FOREIGN KEY (id_plan) REFERENCES planes_estudio(id_plan)
);

CREATE TABLE IF NOT EXISTS docentes_habilitados (
    id_docente TEXT NOT NULL,
    id_asignatura TEXT NOT NULL,
    fecha_habilitacion TEXT DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (id_docente, id_asignatura),
    FOREIGN KEY (id_docente) REFERENCES docentes(id_docente) ON DELETE CASCADE,
    FOREIGN KEY (id_asignatura) REFERENCES asignaturas(id_asignatura) ON DELETE CASCADE
);

-- ==========================================
-- MÓDULO: PROGRAMACIÓN
-- ==========================================

CREATE TABLE IF NOT EXISTS disponibilidad (
    id_disponibilidad TEXT PRIMARY KEY,
    id_docente TEXT NOT NULL,
    dia_semana INTEGER NOT NULL CHECK (dia_semana BETWEEN 1 AND 6),
    hora_inicio TEXT NOT NULL,
    hora_fin TEXT NOT NULL,
    FOREIGN KEY (id_docente) REFERENCES docentes(id_docente)
);

CREATE TABLE IF NOT EXISTS asignaciones (
    id_asignacion TEXT PRIMARY KEY,
    id_docente TEXT NOT NULL,
    id_asignatura TEXT NOT NULL,
    dia INTEGER NOT NULL CHECK (dia BETWEEN 1 AND 6),
    hora_inicio TEXT NOT NULL,
    hora_fin TEXT NOT NULL,
    periodo TEXT NOT NULL,
    estado TEXT NOT NULL DEFAULT 'Propuesta' CHECK (estado IN ('Propuesta', 'Confirmado')),
    FOREIGN KEY (id_docente) REFERENCES docentes(id_docente),
    FOREIGN KEY (id_asignatura) REFERENCES asignaturas(id_asignatura)
);

CREATE TABLE IF NOT EXISTS bloqueos (
    id_bloqueo TEXT PRIMARY KEY,
    id_asignatura TEXT NOT NULL,
    dia INTEGER NOT NULL CHECK (dia BETWEEN 1 AND 6),
    hora_inicio TEXT NOT NULL,
    hora_fin TEXT NOT NULL,
    FOREIGN KEY (id_asignatura) REFERENCES asignaturas(id_asignatura)
);

-- ==========================================
-- DATOS BASE
-- ==========================================

INSERT OR IGNORE INTO roles (id_rol, nombre_rol)
VALUES
(1, 'Administrador'),
(2, 'Coordinador');

INSERT OR IGNORE INTO planes_estudio (id_plan, nombre_plan, jornada)
VALUES
('11111111-1111-1111-1111-111111111111', 'Plan de Estudios 1020 Jornada Diurna', 'Diurna'),
('22222222-2222-2222-2222-222222222222', 'Plan de Estudios Jornada Noche', 'Nocturna');