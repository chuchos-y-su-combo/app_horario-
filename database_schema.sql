-- Crear la base de datos si no existe
CREATE DATABASE IF NOT EXISTS gestion_horarios;
USE gestion_horarios;

-- ==========================================
-- MÓDULO: SEGURIDAD (RF-01, RF-02, RF-03)
-- ==========================================
CREATE TABLE roles (
    id_rol INT AUTO_INCREMENT PRIMARY KEY,
    nombre_rol VARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE usuarios (
    id_usuario CHAR(36) PRIMARY KEY,
    id_rol INT NOT NULL,
    correo VARCHAR(100) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL, -- RNF-03: Cifrado bcrypt
    nombre_completo VARCHAR(150) NOT NULL,
    FOREIGN KEY (id_rol) REFERENCES roles(id_rol)
);

-- ==========================================
-- MÓDULO: ACADÉMICO (RF-05, RF-11, RF-07)
-- ==========================================
CREATE TABLE planes_estudio (
    id_plan CHAR(36) PRIMARY KEY,
    nombre_plan VARCHAR(100) NOT NULL,
    jornada ENUM('Diurna', 'Nocturna') NOT NULL -- R-05
);

CREATE TABLE docentes (
    id_docente CHAR(36) PRIMARY KEY,
    identificacion VARCHAR(20) NOT NULL UNIQUE,
    nombre VARCHAR(150) NOT NULL,
    tipo_contrato ENUM('TC', 'TP') NOT NULL, -- R-02
    max_asignaturas INT NOT NULL -- TC: 5, TP: 3 (RF-06)
);

CREATE TABLE asignaturas (
    id_asignatura CHAR(36) PRIMARY KEY,
    id_plan CHAR(36) NOT NULL,
    codigo VARCHAR(20) NOT NULL UNIQUE,
    nombre VARCHAR(100) NOT NULL,
    creditos INT NOT NULL,
    semestre INT NOT NULL,
    min_estudiantes INT DEFAULT 15, -- R-07
    es_fija_tapsi TINYINT(1) DEFAULT 0, -- RF-12
    FOREIGN KEY (id_plan) REFERENCES planes_estudio(id_plan)
);

-- Tabla de competencias (RF-07: Determina materias según currículo)
CREATE TABLE docentes_habilitados (
    id_docente CHAR(36) NOT NULL,
    id_asignatura CHAR(36) NOT NULL,
    fecha_habilitacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (id_docente, id_asignatura),
    FOREIGN KEY (id_docente) REFERENCES docentes(id_docente) ON DELETE CASCADE,
    FOREIGN KEY (id_asignatura) REFERENCES asignaturas(id_asignatura) ON DELETE CASCADE
);

-- ==========================================
-- MÓDULO: PROGRAMACIÓN (RF-08, RF-15, RF-18)
-- ==========================================
CREATE TABLE disponibilidad (
    id_disponibilidad CHAR(36) PRIMARY KEY,
    id_docente CHAR(36) NOT NULL,
    dia_semana INT CHECK (dia_semana BETWEEN 1 AND 6),
    hora_inicio TIME NOT NULL,
    hora_fin TIME NOT NULL,
    FOREIGN KEY (id_docente) REFERENCES docentes(id_docente)
);

CREATE TABLE asignaciones (
    id_asignacion CHAR(36) PRIMARY KEY,
    id_docente CHAR(36) NOT NULL,
    id_asignatura CHAR(36) NOT NULL,
    dia INT NOT NULL,
    hora_inicio TIME NOT NULL,
    hora_fin TIME NOT NULL,
    periodo VARCHAR(10) NOT NULL, -- RF-28 (Historial)
    estado ENUM('Propuesta', 'Confirmado') DEFAULT 'Propuesta',
    FOREIGN KEY (id_docente) REFERENCES docentes(id_docente),
    FOREIGN KEY (id_asignatura) REFERENCES asignaturas(id_asignatura)
);

CREATE TABLE bloqueos (
    id_bloqueo CHAR(36) PRIMARY KEY,
    id_asignatura CHAR(36) NOT NULL,
    dia INT NOT NULL,
    hora_inicio TIME NOT NULL,
    hora_fin TIME NOT NULL, -- RF-18
    FOREIGN KEY (id_asignatura) REFERENCES asignaturas(id_asignatura)
);