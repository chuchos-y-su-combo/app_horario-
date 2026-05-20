using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ApplicationSchedule.Infrastructure.Migrations
{
    /// <summary>
    /// Migración inicial que crea las tablas base del esquema de la aplicación.
    /// - Crea tablas: `docentes`, `planes_estudio`, `roles`, `disponibilidad`, `asignaturas`, `usuarios`, `asignaciones`, `docentes_habilitados`.
    /// - Inserta datos semilla en `planes_estudio` y `roles`.
    /// Este archivo se genera por EF Core y describe los cambios a aplicar al esquema en su método <see cref="Up"/> y cómo revertirlos en <see cref="Down"/>.
    /// </summary>
    public partial class InitialCreate : Migration
    {
        /// <summary>
        /// Aplica la migración: crea tablas, índices y datos semilla necesarios para la aplicación.
        /// </summary>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "docentes",
                columns: table => new
                {
                    id_docente = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    identificacion = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    nombre = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    tipo_contrato = table.Column<string>(type: "TEXT", maxLength: 2, nullable: false),
                    max_asignaturas = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_docentes", x => x.id_docente);
                });

            migrationBuilder.CreateTable(
                name: "planes_estudio",
                columns: table => new
                {
                    id_plan = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    nombre_plan = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    jornada = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_planes_estudio", x => x.id_plan);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id_rol = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    nombre_rol = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.id_rol);
                });

            migrationBuilder.CreateTable(
                name: "disponibilidad",
                columns: table => new
                {
                    id_disponibilidad = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    id_docente = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    dia_semana = table.Column<int>(type: "INTEGER", nullable: false),
                    hora_inicio = table.Column<string>(type: "TEXT", maxLength: 5, nullable: false),
                    hora_fin = table.Column<string>(type: "TEXT", maxLength: 5, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_disponibilidad", x => x.id_disponibilidad);
                    table.ForeignKey(
                        name: "FK_disponibilidad_docentes_id_docente",
                        column: x => x.id_docente,
                        principalTable: "docentes",
                        principalColumn: "id_docente",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "asignaturas",
                columns: table => new
                {
                    id_asignatura = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    id_plan = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    codigo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    creditos = table.Column<int>(type: "INTEGER", nullable: false),
                    semestre = table.Column<int>(type: "INTEGER", nullable: false),
                    min_estudiantes = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 15),
                    es_fija_tapsi = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    es_opcional_tapsi_diurna = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_asignaturas", x => x.id_asignatura);
                    table.ForeignKey(
                        name: "FK_asignaturas_planes_estudio_id_plan",
                        column: x => x.id_plan,
                        principalTable: "planes_estudio",
                        principalColumn: "id_plan",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    id_usuario = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    id_rol = table.Column<int>(type: "INTEGER", nullable: false),
                    correo = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    password_hash = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    nombre_completo = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.id_usuario);
                    table.ForeignKey(
                        name: "FK_usuarios_roles_id_rol",
                        column: x => x.id_rol,
                        principalTable: "roles",
                        principalColumn: "id_rol",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "asignaciones",
                columns: table => new
                {
                    id_asignacion = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    id_docente = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    id_asignatura = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    dia = table.Column<int>(type: "INTEGER", nullable: false),
                    hora_inicio = table.Column<string>(type: "TEXT", maxLength: 5, nullable: false),
                    hora_fin = table.Column<string>(type: "TEXT", maxLength: 5, nullable: false),
                    periodo = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    estado = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false, defaultValue: "Propuesta"),
                    escenario = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false, defaultValue: "ING_DIURNA")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_asignaciones", x => x.id_asignacion);
                    table.ForeignKey(
                        name: "FK_asignaciones_asignaturas_id_asignatura",
                        column: x => x.id_asignatura,
                        principalTable: "asignaturas",
                        principalColumn: "id_asignatura",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_asignaciones_docentes_id_docente",
                        column: x => x.id_docente,
                        principalTable: "docentes",
                        principalColumn: "id_docente",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "docentes_habilitados",
                columns: table => new
                {
                    id_docente = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    id_asignatura = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    fecha_habilitacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    fuente = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "Excel")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_docentes_habilitados", x => new { x.id_docente, x.id_asignatura });
                    table.ForeignKey(
                        name: "FK_docentes_habilitados_asignaturas_id_asignatura",
                        column: x => x.id_asignatura,
                        principalTable: "asignaturas",
                        principalColumn: "id_asignatura",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_docentes_habilitados_docentes_id_docente",
                        column: x => x.id_docente,
                        principalTable: "docentes",
                        principalColumn: "id_docente",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "planes_estudio",
                columns: new[] { "id_plan", "jornada", "nombre_plan" },
                values: new object[,]
                {
                    { "11111111-1111-1111-1111-111111111111", "Diurna", "Plan de Estudios 1020 Jornada Diurna" },
                    { "22222222-2222-2222-2222-222222222222", "Nocturna", "Plan de Estudios Jornada Noche" }
                });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "id_rol", "nombre_rol" },
                values: new object[,]
                {
                    { 1, "Administrador" },
                    { 2, "Coordinador" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_asignaciones_id_asignatura",
                table: "asignaciones",
                column: "id_asignatura");

            migrationBuilder.CreateIndex(
                name: "IX_asignaciones_id_docente",
                table: "asignaciones",
                column: "id_docente");

            migrationBuilder.CreateIndex(
                name: "IX_asignaturas_codigo",
                table: "asignaturas",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_asignaturas_id_plan",
                table: "asignaturas",
                column: "id_plan");

            migrationBuilder.CreateIndex(
                name: "IX_disponibilidad_id_docente",
                table: "disponibilidad",
                column: "id_docente");

            migrationBuilder.CreateIndex(
                name: "IX_docentes_identificacion",
                table: "docentes",
                column: "identificacion",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_docentes_habilitados_id_asignatura",
                table: "docentes_habilitados",
                column: "id_asignatura");

            migrationBuilder.CreateIndex(
                name: "IX_roles_nombre_rol",
                table: "roles",
                column: "nombre_rol",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_correo",
                table: "usuarios",
                column: "correo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_id_rol",
                table: "usuarios",
                column: "id_rol");
        }

        /// <summary>
        /// Revierte la migración eliminando las tablas creadas por <see cref="Up"/>.
        /// Usado cuando se necesita deshacer la migración.
        /// </summary>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "asignaciones");

            migrationBuilder.DropTable(
                name: "disponibilidad");

            migrationBuilder.DropTable(
                name: "docentes_habilitados");

            migrationBuilder.DropTable(
                name: "usuarios");

            migrationBuilder.DropTable(
                name: "asignaturas");

            migrationBuilder.DropTable(
                name: "docentes");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "planes_estudio");
        }
    }
}
