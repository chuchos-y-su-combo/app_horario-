CREATE DATABASE IF NOT EXISTS ApplicationSchedule
CHARACTER SET utf8mb4
COLLATE utf8mb4_unicode_ci;

USE ApplicationSchedule;

CREATE TABLE IF NOT EXISTS Roles (
    id_rol INT NOT NULL AUTO_INCREMENT,
    nombre_rol VARCHAR(50) NOT NULL,
    CONSTRAINT pk_roles PRIMARY KEY (id_rol),
    CONSTRAINT uq_roles_nombre UNIQUE (nombre_rol)
);

CREATE TABLE IF NOT EXISTS Usuarios (
    id_usuario CHAR(36) NOT NULL,
    id_rol INT NOT NULL,
    correo VARCHAR(100) NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    nombre_completo VARCHAR(150) NOT NULL,
    CONSTRAINT pk_usuarios PRIMARY KEY (id_usuario),
    CONSTRAINT uq_usuarios_correo UNIQUE (correo),
    CONSTRAINT fk_usuarios_roles FOREIGN KEY (id_rol)
        REFERENCES Roles(id_rol)
        ON DELETE RESTRICT
        ON UPDATE CASCADE
);

INSERT INTO Roles (id_rol, nombre_rol)
VALUES
    (1, 'Administrador'),
    (2, 'Coordinador')
ON DUPLICATE KEY UPDATE
    nombre_rol = VALUES(nombre_rol);