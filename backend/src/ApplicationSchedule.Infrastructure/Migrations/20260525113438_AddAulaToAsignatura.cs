using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ApplicationSchedule.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAulaToAsignatura : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_asignaturas_codigo",
                table: "asignaturas");

            migrationBuilder.AddColumn<string>(
                name: "aula",
                table: "asignaturas",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "es_area_profesional",
                table: "asignaturas",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "bloqueos_franja_asignatura",
                columns: table => new
                {
                    id_bloqueo = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    id_asignatura = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    periodo = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    dia = table.Column<int>(type: "INTEGER", nullable: false),
                    hora_inicio = table.Column<string>(type: "TEXT", maxLength: 5, nullable: false),
                    hora_fin = table.Column<string>(type: "TEXT", maxLength: 5, nullable: false),
                    motivo = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                    fecha_creacion_utc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bloqueos_franja_asignatura", x => x.id_bloqueo);
                    table.ForeignKey(
                        name: "FK_bloqueos_franja_asignatura_asignaturas_id_asignatura",
                        column: x => x.id_asignatura,
                        principalTable: "asignaturas",
                        principalColumn: "id_asignatura",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "asignaturas",
                columns: new[] { "id_asignatura", "aula", "codigo", "creditos", "id_plan", "min_estudiantes", "nombre", "semestre" },
                values: new object[,]
                {
                    { "a1000001-a100-a100-a100-a10000000000", "AULA-F101", "104066", 4, "11111111-1111-1111-1111-111111111111", 15, "Matemáticas Básicas", 1 },
                    { "a1000002-a100-a100-a100-a10000000000", "AULA-F102", "100010", 2, "11111111-1111-1111-1111-111111111111", 15, "Fundamentos de Ingeniería", 1 }
                });

            migrationBuilder.InsertData(
                table: "asignaturas",
                columns: new[] { "id_asignatura", "aula", "codigo", "creditos", "es_area_profesional", "id_plan", "min_estudiantes", "nombre", "semestre" },
                values: new object[,]
                {
                    { "a1000003-a100-a100-a100-a10000000000", "AULA-F103", "103004", 3, true, "11111111-1111-1111-1111-111111111111", 15, "Teoría de Sistemas", 1 },
                    { "a1000004-a100-a100-a100-a10000000000", "AULA-F104", "103002", 3, true, "11111111-1111-1111-1111-111111111111", 15, "Lógica de Programación", 1 }
                });

            migrationBuilder.InsertData(
                table: "asignaturas",
                columns: new[] { "id_asignatura", "aula", "codigo", "creditos", "id_plan", "min_estudiantes", "nombre", "semestre" },
                values: new object[,]
                {
                    { "a1000005-a100-a100-a100-a10000000000", "AULA-F105", "100059", 3, "11111111-1111-1111-1111-111111111111", 15, "Competencias Comunicativas", 1 },
                    { "a1000006-a100-a100-a100-a10000000000", "AULA-F106", "105038", 2, "11111111-1111-1111-1111-111111111111", 15, "Ética", 1 },
                    { "a1000007-a100-a100-a100-a10000000000", "AULA-F107", "151601", 1, "11111111-1111-1111-1111-111111111111", 15, "Inglés I", 1 }
                });

            migrationBuilder.InsertData(
                table: "asignaturas",
                columns: new[] { "id_asignatura", "aula", "codigo", "creditos", "es_area_profesional", "id_plan", "min_estudiantes", "nombre", "semestre" },
                values: new object[,]
                {
                    { "a1000008-a100-a100-a100-a10000000000", "AULA-F108", "103007", 3, true, "11111111-1111-1111-1111-111111111111", 15, "Técnicas de Programación", 2 },
                    { "a1000009-a100-a100-a100-a10000000000", "AULA-F109", "103008", 3, true, "11111111-1111-1111-1111-111111111111", 15, "Fundamentos de POO", 2 },
                    { "a1000010-a100-a100-a100-a10000000000", "AULA-F110", "104027", 4, true, "11111111-1111-1111-1111-111111111111", 15, "Matemáticas Discretas", 3 },
                    { "a1000011-a100-a100-a100-a10000000000", "AULA-F111", "103011", 3, true, "11111111-1111-1111-1111-111111111111", 15, "Estructura de Datos", 3 },
                    { "a1000012-a100-a100-a100-a10000000000", "AULA-F112", "103018", 3, true, "11111111-1111-1111-1111-111111111111", 15, "Programación Orientada a Objetos", 3 },
                    { "a1000013-a100-a100-a100-a10000000000", "AULA-F113", "103027", 3, true, "11111111-1111-1111-1111-111111111111", 15, "Sistemas Operativos", 4 },
                    { "a1000014-a100-a100-a100-a10000000000", "AULA-F114", "109180", 3, true, "11111111-1111-1111-1111-111111111111", 15, "Bases de Datos I", 4 },
                    { "a1000015-a100-a100-a100-a10000000000", "AULA-F115", "103022", 3, true, "11111111-1111-1111-1111-111111111111", 15, "Ingeniería de Software I", 4 },
                    { "a1000016-a100-a100-a100-a10000000000", "AULA-F116", "109182", 3, true, "11111111-1111-1111-1111-111111111111", 15, "Paradigmas de Lenguajes", 5 },
                    { "a1000017-a100-a100-a100-a10000000000", "AULA-F117", "103126", 3, true, "11111111-1111-1111-1111-111111111111", 15, "Redes LAN", 5 },
                    { "a1000018-a100-a100-a100-a10000000000", "AULA-F118", "103093", 3, true, "11111111-1111-1111-1111-111111111111", 15, "Ingeniería de Software II", 5 },
                    { "a1000019-a100-a100-a100-a10000000000", "AULA-F119", "109183", 3, true, "11111111-1111-1111-1111-111111111111", 15, "Programación Backend", 5 },
                    { "a1000020-a100-a100-a100-a10000000000", "AULA-F120", "109184", 3, true, "11111111-1111-1111-1111-111111111111", 15, "Electrónica Digital y Arq. Computadores", 6 },
                    { "a1000021-a100-a100-a100-a10000000000", "AULA-F121", "109185", 3, true, "11111111-1111-1111-1111-111111111111", 15, "Modelamiento y Simulación", 6 },
                    { "a1000022-a100-a100-a100-a10000000000", "AULA-F122", "109186", 2, true, "11111111-1111-1111-1111-111111111111", 15, "Procesadores de Lenguajes", 6 },
                    { "a1000023-a100-a100-a100-a10000000000", "AULA-F123", "109187", 3, true, "11111111-1111-1111-1111-111111111111", 15, "Programación Frontend", 6 },
                    { "a1000024-a100-a100-a100-a10000000000", "AULA-F124", "103021", 3, true, "11111111-1111-1111-1111-111111111111", 15, "Diseño de Algoritmos", 6 },
                    { "a1000025-a100-a100-a100-a10000000000", "AULA-F125", "109104", 3, true, "11111111-1111-1111-1111-111111111111", 15, "Sistemas Embebidos", 7 },
                    { "a1000026-a100-a100-a100-a10000000000", "AULA-F126", "103127", 3, true, "11111111-1111-1111-1111-111111111111", 15, "Énfasis Profesional", 7 },
                    { "a1000027-a100-a100-a100-a10000000000", "AULA-F127", "109188", 3, true, "11111111-1111-1111-1111-111111111111", 15, "Ciencia de los Datos", 7 },
                    { "a1000028-a100-a100-a100-a10000000000", "AULA-F128", "109181", 2, true, "11111111-1111-1111-1111-111111111111", 15, "Bases de Datos II", 7 },
                    { "a1000029-a100-a100-a100-a10000000000", "AULA-F129", "103125", 3, true, "11111111-1111-1111-1111-111111111111", 15, "Sistemas de Información y Organizaciones", 8 },
                    { "a1000030-a100-a100-a100-a10000000000", "AULA-F130", "103117", 3, true, "11111111-1111-1111-1111-111111111111", 15, "Inteligencia Artificial", 8 },
                    { "a1000031-a100-a100-a100-a10000000000", "AULA-F131", "109189", 3, true, "11111111-1111-1111-1111-111111111111", 15, "Programación de Dispositivos Móviles", 8 },
                    { "a1000032-a100-a100-a100-a10000000000", "AULA-F132", "103135", 2, true, "11111111-1111-1111-1111-111111111111", 15, "Proyecto de Desarrollo de Software", 8 },
                    { "a1000033-a100-a100-a100-a10000000000", "AULA-F133", "103136", 9, true, "11111111-1111-1111-1111-111111111111", 15, "Práctica Empresarial", 10 },
                    { "a1000034-a100-a100-a100-a10000000000", "AULA-F134", "103118", 2, true, "11111111-1111-1111-1111-111111111111", 15, "Gerencia de Proyectos Tecnológicos", 10 }
                });

            migrationBuilder.InsertData(
                table: "asignaturas",
                columns: new[] { "id_asignatura", "aula", "codigo", "creditos", "id_plan", "min_estudiantes", "nombre", "semestre" },
                values: new object[,]
                {
                    { "a2000001-a200-a200-a200-a20000000000", "AULA-F201", "104066", 4, "22222222-2222-2222-2222-222222222222", 15, "Matemáticas Básicas", 1 },
                    { "a2000002-a200-a200-a200-a20000000000", "AULA-F202", "100010", 2, "22222222-2222-2222-2222-222222222222", 15, "Fundamentos de Ingeniería", 1 },
                    { "a2000003-a200-a200-a200-a20000000000", "AULA-F203", "104029", 3, "22222222-2222-2222-2222-222222222222", 15, "Álgebra Lineal", 1 }
                });

            migrationBuilder.InsertData(
                table: "asignaturas",
                columns: new[] { "id_asignatura", "aula", "codigo", "creditos", "es_area_profesional", "id_plan", "min_estudiantes", "nombre", "semestre" },
                values: new object[] { "a2000004-a200-a200-a200-a20000000000", "AULA-F204", "103002", 3, true, "22222222-2222-2222-2222-222222222222", 15, "Lógica de Programación", 1 });

            migrationBuilder.InsertData(
                table: "asignaturas",
                columns: new[] { "id_asignatura", "aula", "codigo", "creditos", "id_plan", "min_estudiantes", "nombre", "semestre" },
                values: new object[,]
                {
                    { "a2000005-a200-a200-a200-a20000000000", "AULA-F205", "105038", 2, "22222222-2222-2222-2222-222222222222", 15, "Ética", 1 },
                    { "a2000006-a200-a200-a200-a20000000000", "AULA-F206", "151601", 1, "22222222-2222-2222-2222-222222222222", 15, "Inglés I", 1 }
                });

            migrationBuilder.InsertData(
                table: "asignaturas",
                columns: new[] { "id_asignatura", "aula", "codigo", "creditos", "es_area_profesional", "id_plan", "min_estudiantes", "nombre", "semestre" },
                values: new object[,]
                {
                    { "a2000007-a200-a200-a200-a20000000000", "AULA-F207", "103004", 3, true, "22222222-2222-2222-2222-222222222222", 15, "Teoría de Sistemas", 2 },
                    { "a2000008-a200-a200-a200-a20000000000", "AULA-F208", "103007", 3, true, "22222222-2222-2222-2222-222222222222", 15, "Técnicas de Programación", 2 },
                    { "a2000009-a200-a200-a200-a20000000000", "AULA-F209", "103008", 3, true, "22222222-2222-2222-2222-222222222222", 15, "Fundamentos de POO", 2 },
                    { "a2000010-a200-a200-a200-a20000000000", "AULA-F210", "104027", 4, true, "22222222-2222-2222-2222-222222222222", 15, "Matemáticas Discretas", 3 },
                    { "a2000011-a200-a200-a200-a20000000000", "AULA-F211", "103018", 3, true, "22222222-2222-2222-2222-222222222222", 15, "Programación Orientada a Objetos", 3 },
                    { "a2000012-a200-a200-a200-a20000000000", "AULA-F212", "103011", 3, true, "22222222-2222-2222-2222-222222222222", 15, "Estructura de Datos", 4 },
                    { "a2000013-a200-a200-a200-a20000000000", "AULA-F213", "103022", 3, true, "22222222-2222-2222-2222-222222222222", 15, "Ingeniería de Software I", 4 },
                    { "a2000014-a200-a200-a200-a20000000000", "AULA-F214", "109180", 3, true, "22222222-2222-2222-2222-222222222222", 15, "Bases de Datos I", 5 },
                    { "a2000015-a200-a200-a200-a20000000000", "AULA-F215", "109184", 3, true, "22222222-2222-2222-2222-222222222222", 15, "Electrónica Digital y Arq. Computadores", 6 },
                    { "a2000016-a200-a200-a200-a20000000000", "AULA-F216", "109182", 3, true, "22222222-2222-2222-2222-222222222222", 15, "Paradigmas de Lenguajes", 6 },
                    { "a2000017-a200-a200-a200-a20000000000", "AULA-F217", "103126", 3, true, "22222222-2222-2222-2222-222222222222", 15, "Redes LAN", 6 },
                    { "a2000018-a200-a200-a200-a20000000000", "AULA-F218", "109183", 3, true, "22222222-2222-2222-2222-222222222222", 15, "Programación Backend", 6 },
                    { "a2000019-a200-a200-a200-a20000000000", "AULA-F219", "103027", 3, true, "22222222-2222-2222-2222-222222222222", 15, "Sistemas Operativos", 6 },
                    { "a2000020-a200-a200-a200-a20000000000", "AULA-F220", "109104", 3, true, "22222222-2222-2222-2222-222222222222", 15, "Sistemas Embebidos", 7 },
                    { "a2000021-a200-a200-a200-a20000000000", "AULA-F221", "109186", 2, true, "22222222-2222-2222-2222-222222222222", 15, "Procesadores de Lenguajes", 7 },
                    { "a2000022-a200-a200-a200-a20000000000", "AULA-F222", "109185", 3, true, "22222222-2222-2222-2222-222222222222", 15, "Modelamiento y Simulación", 7 },
                    { "a2000023-a200-a200-a200-a20000000000", "AULA-F223", "109187", 3, true, "22222222-2222-2222-2222-222222222222", 15, "Programación Frontend", 7 },
                    { "a2000024-a200-a200-a200-a20000000000", "AULA-F224", "103093", 3, true, "22222222-2222-2222-2222-222222222222", 15, "Ingeniería de Software II", 7 },
                    { "a2000025-a200-a200-a200-a20000000000", "AULA-F225", "109188", 3, true, "22222222-2222-2222-2222-222222222222", 15, "Ciencia de los Datos", 8 },
                    { "a2000026-a200-a200-a200-a20000000000", "AULA-F226", "103021", 3, true, "22222222-2222-2222-2222-222222222222", 15, "Diseño de Algoritmos", 8 },
                    { "a2000027-a200-a200-a200-a20000000000", "AULA-F227", "109181", 2, true, "22222222-2222-2222-2222-222222222222", 15, "Bases de Datos II", 8 },
                    { "a2000028-a200-a200-a200-a20000000000", "AULA-F228", "103127", 3, true, "22222222-2222-2222-2222-222222222222", 15, "Énfasis Profesional", 9 },
                    { "a2000029-a200-a200-a200-a20000000000", "AULA-F229", "109189", 3, true, "22222222-2222-2222-2222-222222222222", 15, "Programación de Dispositivos Móviles", 9 },
                    { "a2000030-a200-a200-a200-a20000000000", "AULA-F230", "103135", 2, true, "22222222-2222-2222-2222-222222222222", 15, "Proyecto de Desarrollo de Software", 9 },
                    { "a2000031-a200-a200-a200-a20000000000", "AULA-F231", "103125", 3, true, "22222222-2222-2222-2222-222222222222", 15, "Sistemas de Información y Organizaciones", 10 },
                    { "a2000032-a200-a200-a200-a20000000000", "AULA-F232", "103117", 3, true, "22222222-2222-2222-2222-222222222222", 15, "Inteligencia Artificial", 10 },
                    { "a2000033-a200-a200-a200-a20000000000", "AULA-F233", "103118", 2, true, "22222222-2222-2222-2222-222222222222", 15, "Gerencia de Proyectos Tecnológicos", 12 },
                    { "a2000034-a200-a200-a200-a20000000000", "AULA-F234", "103136", 9, true, "22222222-2222-2222-2222-222222222222", 15, "Práctica Empresarial", 12 }
                });

            migrationBuilder.UpdateData(
                table: "planes_estudio",
                keyColumn: "id_plan",
                keyValue: "22222222-2222-2222-2222-222222222222",
                column: "nombre_plan",
                value: "Plan de Estudios 1030 Jornada Nocturna");

            migrationBuilder.InsertData(
                table: "planes_estudio",
                columns: new[] { "id_plan", "jornada", "nombre_plan" },
                values: new object[,]
                {
                    { "33333333-3333-3333-3333-333333333333", "Diurna", "Plan TAPSI Jornada Diurna" },
                    { "44444444-4444-4444-4444-444444444444", "Nocturna", "Plan TAPSI Jornada Nocturna" }
                });

            migrationBuilder.InsertData(
                table: "usuarios",
                columns: new[] { "id_usuario", "correo", "id_rol", "nombre_completo", "password_hash" },
                values: new object[] { "11111111-1111-1111-1111-111111111111", "admin@universidad.edu", 1, "Administrador Principal", "$2a$11$lQyp9rSox1qD4rA177buV.ZqN7e7pFRBz9k3WUahbbpjoBmPPWCQ6" });

            migrationBuilder.InsertData(
                table: "asignaturas",
                columns: new[] { "id_asignatura", "aula", "codigo", "creditos", "es_area_profesional", "es_fija_tapsi", "id_plan", "min_estudiantes", "nombre", "semestre" },
                values: new object[,]
                {
                    { "a3000001-a300-a300-a300-a30000000000", "AULA-F301", "104030", 3, true, true, "33333333-3333-3333-3333-333333333333", 15, "Cálculo Diferencial", 1 },
                    { "a3000002-a300-a300-a300-a30000000000", "AULA-F302", "103007", 3, true, true, "33333333-3333-3333-3333-333333333333", 15, "Técnicas de Programación", 1 },
                    { "a3000003-a300-a300-a300-a30000000000", "AULA-F303", "103018", 3, true, true, "33333333-3333-3333-3333-333333333333", 15, "Programación Orientada a Objetos", 1 },
                    { "a3000004-a300-a300-a300-a30000000000", "AULA-F304", "103004", 3, true, true, "33333333-3333-3333-3333-333333333333", 15, "Teoría de Sistemas", 1 },
                    { "a3000005-a300-a300-a300-a30000000000", "AULA-F305", "103027", 3, true, true, "33333333-3333-3333-3333-333333333333", 15, "Sistemas Operativos", 1 }
                });

            migrationBuilder.InsertData(
                table: "asignaturas",
                columns: new[] { "id_asignatura", "aula", "codigo", "creditos", "es_area_profesional", "es_opcional_tapsi_diurna", "id_plan", "min_estudiantes", "nombre", "semestre" },
                values: new object[,]
                {
                    { "a3000006-a300-a300-a300-a30000000000", "AULA-F306", "103093", 3, true, true, "33333333-3333-3333-3333-333333333333", 15, "Ingeniería de Software II", 1 },
                    { "a3000007-a300-a300-a300-a30000000000", "AULA-F307", "103126", 3, true, true, "33333333-3333-3333-3333-333333333333", 15, "Redes LAN", 1 },
                    { "a3000008-a300-a300-a300-a30000000000", "AULA-F308", "109183", 3, true, true, "33333333-3333-3333-3333-333333333333", 15, "Programación Backend", 1 }
                });

            migrationBuilder.InsertData(
                table: "asignaturas",
                columns: new[] { "id_asignatura", "aula", "codigo", "creditos", "es_area_profesional", "es_fija_tapsi", "id_plan", "min_estudiantes", "nombre", "semestre" },
                values: new object[,]
                {
                    { "a4000001-a400-a400-a400-a40000000000", "AULA-F401", "104030", 3, true, true, "44444444-4444-4444-4444-444444444444", 15, "Cálculo Diferencial", 1 },
                    { "a4000002-a400-a400-a400-a40000000000", "AULA-F402", "103007", 3, true, true, "44444444-4444-4444-4444-444444444444", 15, "Técnicas de Programación", 1 },
                    { "a4000003-a400-a400-a400-a40000000000", "AULA-F403", "103018", 3, true, true, "44444444-4444-4444-4444-444444444444", 15, "Programación Orientada a Objetos", 1 },
                    { "a4000004-a400-a400-a400-a40000000000", "AULA-F404", "103004", 3, true, true, "44444444-4444-4444-4444-444444444444", 15, "Teoría de Sistemas", 1 },
                    { "a4000005-a400-a400-a400-a40000000000", "AULA-F405", "103027", 3, true, true, "44444444-4444-4444-4444-444444444444", 15, "Sistemas Operativos", 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_asignaturas_codigo_id_plan",
                table: "asignaturas",
                columns: new[] { "codigo", "id_plan" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_bloqueos_franja_asignatura_id_asignatura_periodo_dia_hora_inicio_hora_fin",
                table: "bloqueos_franja_asignatura",
                columns: new[] { "id_asignatura", "periodo", "dia", "hora_inicio", "hora_fin" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bloqueos_franja_asignatura");

            migrationBuilder.DropIndex(
                name: "IX_asignaturas_codigo_id_plan",
                table: "asignaturas");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a1000001-a100-a100-a100-a10000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a1000002-a100-a100-a100-a10000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a1000003-a100-a100-a100-a10000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a1000004-a100-a100-a100-a10000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a1000005-a100-a100-a100-a10000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a1000006-a100-a100-a100-a10000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a1000007-a100-a100-a100-a10000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a1000008-a100-a100-a100-a10000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a1000009-a100-a100-a100-a10000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a1000010-a100-a100-a100-a10000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a1000011-a100-a100-a100-a10000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a1000012-a100-a100-a100-a10000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a1000013-a100-a100-a100-a10000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a1000014-a100-a100-a100-a10000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a1000015-a100-a100-a100-a10000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a1000016-a100-a100-a100-a10000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a1000017-a100-a100-a100-a10000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a1000018-a100-a100-a100-a10000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a1000019-a100-a100-a100-a10000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a1000020-a100-a100-a100-a10000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a1000021-a100-a100-a100-a10000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a1000022-a100-a100-a100-a10000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a1000023-a100-a100-a100-a10000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a1000024-a100-a100-a100-a10000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a1000025-a100-a100-a100-a10000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a1000026-a100-a100-a100-a10000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a1000027-a100-a100-a100-a10000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a1000028-a100-a100-a100-a10000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a1000029-a100-a100-a100-a10000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a1000030-a100-a100-a100-a10000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a1000031-a100-a100-a100-a10000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a1000032-a100-a100-a100-a10000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a1000033-a100-a100-a100-a10000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a1000034-a100-a100-a100-a10000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a2000001-a200-a200-a200-a20000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a2000002-a200-a200-a200-a20000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a2000003-a200-a200-a200-a20000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a2000004-a200-a200-a200-a20000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a2000005-a200-a200-a200-a20000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a2000006-a200-a200-a200-a20000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a2000007-a200-a200-a200-a20000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a2000008-a200-a200-a200-a20000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a2000009-a200-a200-a200-a20000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a2000010-a200-a200-a200-a20000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a2000011-a200-a200-a200-a20000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a2000012-a200-a200-a200-a20000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a2000013-a200-a200-a200-a20000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a2000014-a200-a200-a200-a20000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a2000015-a200-a200-a200-a20000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a2000016-a200-a200-a200-a20000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a2000017-a200-a200-a200-a20000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a2000018-a200-a200-a200-a20000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a2000019-a200-a200-a200-a20000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a2000020-a200-a200-a200-a20000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a2000021-a200-a200-a200-a20000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a2000022-a200-a200-a200-a20000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a2000023-a200-a200-a200-a20000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a2000024-a200-a200-a200-a20000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a2000025-a200-a200-a200-a20000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a2000026-a200-a200-a200-a20000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a2000027-a200-a200-a200-a20000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a2000028-a200-a200-a200-a20000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a2000029-a200-a200-a200-a20000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a2000030-a200-a200-a200-a20000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a2000031-a200-a200-a200-a20000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a2000032-a200-a200-a200-a20000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a2000033-a200-a200-a200-a20000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a2000034-a200-a200-a200-a20000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a3000001-a300-a300-a300-a30000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a3000002-a300-a300-a300-a30000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a3000003-a300-a300-a300-a30000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a3000004-a300-a300-a300-a30000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a3000005-a300-a300-a300-a30000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a3000006-a300-a300-a300-a30000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a3000007-a300-a300-a300-a30000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a3000008-a300-a300-a300-a30000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a4000001-a400-a400-a400-a40000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a4000002-a400-a400-a400-a40000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a4000003-a400-a400-a400-a40000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a4000004-a400-a400-a400-a40000000000");

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "id_asignatura",
                keyValue: "a4000005-a400-a400-a400-a40000000000");

            migrationBuilder.DeleteData(
                table: "usuarios",
                keyColumn: "id_usuario",
                keyValue: "11111111-1111-1111-1111-111111111111");

            migrationBuilder.DeleteData(
                table: "planes_estudio",
                keyColumn: "id_plan",
                keyValue: "33333333-3333-3333-3333-333333333333");

            migrationBuilder.DeleteData(
                table: "planes_estudio",
                keyColumn: "id_plan",
                keyValue: "44444444-4444-4444-4444-444444444444");

            migrationBuilder.DropColumn(
                name: "aula",
                table: "asignaturas");

            migrationBuilder.DropColumn(
                name: "es_area_profesional",
                table: "asignaturas");

            migrationBuilder.UpdateData(
                table: "planes_estudio",
                keyColumn: "id_plan",
                keyValue: "22222222-2222-2222-2222-222222222222",
                column: "nombre_plan",
                value: "Plan de Estudios Jornada Noche");

            migrationBuilder.CreateIndex(
                name: "IX_asignaturas_codigo",
                table: "asignaturas",
                column: "codigo",
                unique: true);
        }
    }
}
