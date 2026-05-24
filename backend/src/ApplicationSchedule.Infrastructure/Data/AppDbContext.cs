using ApplicationSchedule.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApplicationSchedule.Infrastructure.Data;

/// <summary>
/// Contexto de base de datos de la aplicación.
/// Centraliza las entidades del dominio y la configuración de esquema para EF Core.
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>
    /// Constructor de <see cref="AppDbContext"/> que recibe las opciones de EF Core.
    /// </summary>
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    /// <summary>Tabla de roles del sistema.</summary>
    public DbSet<Rol> Roles => Set<Rol>();
    /// <summary>Tabla de usuarios.</summary>
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    /// <summary>Tabla de planes de estudio.</summary>
    public DbSet<PlanEstudio> PlanesEstudio => Set<PlanEstudio>();
    /// <summary>Tabla de docentes (docentes/ profesores).</summary>
    public DbSet<Docente> Docentes => Set<Docente>();
    /// <summary>Tabla de asignaturas.</summary>
    public DbSet<Asignatura> Asignaturas => Set<Asignatura>();
    /// <summary>Tabla puente que relaciona docentes y asignaturas habilitadas.</summary>
    public DbSet<DocenteHabilitado> DocentesHabilitados => Set<DocenteHabilitado>();
    /// <summary>Tabla de disponibilidades horarias de docentes.</summary>
    public DbSet<Disponibilidad> Disponibilidades => Set<Disponibilidad>();   
    /// <summary>Tabla de asignaciones resultantes (horarios).</summary>
    public DbSet<Asignacion> Asignaciones => Set<Asignacion>();
    /// <summary>Tabla de bloqueos de franja por asignatura.</summary>
    public DbSet<BloqueoFranjaAsignatura> BloqueosFranjaAsignatura => Set<BloqueoFranjaAsignatura>();

    /// <summary>
    /// Punto de entrada para configurar el modelo y el esquema de la base de datos.
    /// Se delega la configuración por secciones a métodos privados para mantener orden.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigurarRoles(modelBuilder);
        ConfigurarUsuarios(modelBuilder);
        ConfigurarPlanesEstudio(modelBuilder);
        ConfigurarDocentes(modelBuilder);
        ConfigurarAsignaturas(modelBuilder);
        ConfigurarDocentesHabilitados(modelBuilder);
        ConfigurarDisponibilidad(modelBuilder);
        ConfigurarAsignaciones(modelBuilder);
        ConfigurarBloqueosFranjaAsignatura(modelBuilder);
    }


    /// <summary>
    /// Configura la relación muchos a muchos entre docentes y asignaturas.
    /// Esta tabla indica qué asignaturas puede dictar cada docente según su currículo.
    /// </summary>
    private static void ConfigurarDocentesHabilitados(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DocenteHabilitado>(entity =>
        {
            entity.ToTable("docentes_habilitados");

            entity.HasKey(dh => new { dh.IdDocente, dh.IdAsignatura });

            entity.Property(dh => dh.IdDocente)
                .HasColumnName("id_docente")
                .HasMaxLength(36)
                .IsRequired();

            entity.Property(dh => dh.IdAsignatura)
                .HasColumnName("id_asignatura")
                .HasMaxLength(36)
                .IsRequired();

            entity.Property(dh => dh.FechaHabilitacion)
                .HasColumnName("fecha_habilitacion")
                .IsRequired();

            entity.Property(dh => dh.Fuente)
                .HasColumnName("fuente")
                .HasMaxLength(50)
                .HasDefaultValue("Excel")
                .IsRequired();

            entity.HasOne(dh => dh.Docente)
                .WithMany(d => d.AsignaturasHabilitadas)
                .HasForeignKey(dh => dh.IdDocente)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(dh => dh.Asignatura)
                .WithMany(a => a.DocentesHabilitados)
                .HasForeignKey(dh => dh.IdAsignatura)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
    private static void ConfigurarRoles(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Rol>(entity =>
        {
            entity.ToTable("roles");

            entity.HasKey(r => r.IdRol);

            entity.Property(r => r.IdRol)
                .HasColumnName("id_rol")
                .ValueGeneratedOnAdd();

            entity.Property(r => r.NombreRol)
                .HasColumnName("nombre_rol")
                .HasMaxLength(50)
                .IsRequired();

            entity.HasIndex(r => r.NombreRol)
                .IsUnique();

            entity.HasData(
                new Rol { IdRol = 1, NombreRol = "Administrador" },
                new Rol { IdRol = 2, NombreRol = "Coordinador" }
            );
        });
    }

    private static void ConfigurarUsuarios(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("usuarios");

            entity.HasKey(u => u.IdUsuario);

            entity.Property(u => u.IdUsuario)
                .HasColumnName("id_usuario")
                .HasMaxLength(36)
                .IsRequired();

            entity.Property(u => u.IdRol)
                .HasColumnName("id_rol")
                .IsRequired();

            entity.Property(u => u.Correo)
                .HasColumnName("correo")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(u => u.PasswordHash)
                .HasColumnName("password_hash")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(u => u.NombreCompleto)
                .HasColumnName("nombre_completo")
                .HasMaxLength(150)
                .IsRequired();

            entity.HasIndex(u => u.Correo)
                .IsUnique();

            entity.HasOne(u => u.Rol)
                .WithMany(r => r.Usuarios)
                .HasForeignKey(u => u.IdRol)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasData(
                new Usuario
                {
                    IdUsuario = "11111111-1111-1111-1111-111111111111",
                    IdRol = 1,
                    Correo = "admin@universidad.edu",
                    PasswordHash = "$2a$11$lQyp9rSox1qD4rA177buV.ZqN7e7pFRBz9k3WUahbbpjoBmPPWCQ6",
                    NombreCompleto = "Administrador Principal"
                }
            );
        });
    }

    private static void ConfigurarPlanesEstudio(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PlanEstudio>(entity =>
        {
            entity.ToTable("planes_estudio");

            entity.HasKey(p => p.IdPlan);

            entity.Property(p => p.IdPlan)
                .HasColumnName("id_plan")
                .HasMaxLength(36)
                .IsRequired();

            entity.Property(p => p.NombrePlan)
                .HasColumnName("nombre_plan")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(p => p.Jornada)
                .HasColumnName("jornada")
                .HasMaxLength(20)
                .IsRequired();

            entity.HasData(
                new PlanEstudio
                {
                    IdPlan = "11111111-1111-1111-1111-111111111111",
                    NombrePlan = "Plan de Estudios 1020 Jornada Diurna",
                    Jornada = "Diurna"
                },
                new PlanEstudio
                {
                    IdPlan = "22222222-2222-2222-2222-222222222222",
                    NombrePlan = "Plan de Estudios 1030 Jornada Nocturna",
                    Jornada = "Nocturna"
                },
                new PlanEstudio
                {
                    IdPlan = "33333333-3333-3333-3333-333333333333",
                    NombrePlan = "Plan TAPSI Jornada Diurna",
                    Jornada = "Diurna"
                },
                new PlanEstudio
                {
                    IdPlan = "44444444-4444-4444-4444-444444444444",
                    NombrePlan = "Plan TAPSI Jornada Nocturna",
                    Jornada = "Nocturna"
                }
            );
        });
    }

    private static void ConfigurarDocentes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Docente>(entity =>
        {
            entity.ToTable("docentes");

            entity.HasKey(d => d.IdDocente);

            entity.Property(d => d.IdDocente)
                .HasColumnName("id_docente")
                .HasMaxLength(36)
                .IsRequired();

            entity.Property(d => d.Identificacion)
                .HasColumnName("identificacion")
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(d => d.Nombre)
                .HasColumnName("nombre")
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(d => d.TipoContrato)
                .HasColumnName("tipo_contrato")
                .HasMaxLength(2)
                .IsRequired();

            entity.Property(d => d.MaxAsignaturas)
                .HasColumnName("max_asignaturas")
                .IsRequired();

            entity.HasIndex(d => d.Identificacion)
                .IsUnique();
        });
    }

    private static void ConfigurarAsignaturas(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Asignatura>(entity =>
        {
            entity.ToTable("asignaturas");

            entity.HasKey(a => a.IdAsignatura);

            entity.Property(a => a.IdAsignatura)
                .HasColumnName("id_asignatura")
                .HasMaxLength(36)
                .IsRequired();

            entity.Property(a => a.IdPlan)
                .HasColumnName("id_plan")
                .HasMaxLength(36)
                .IsRequired();

            entity.Property(a => a.Codigo)
                .HasColumnName("codigo")
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(a => a.Nombre)
                .HasColumnName("nombre")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(a => a.Creditos)
                .HasColumnName("creditos")
                .IsRequired();

            entity.Property(a => a.Semestre)
                .HasColumnName("semestre")
                .IsRequired();

            entity.Property(a => a.MinEstudiantes)
                .HasColumnName("min_estudiantes")
                .HasDefaultValue(15)
                .IsRequired();

            entity.Property(a => a.EsFijaTapsi)
                .HasColumnName("es_fija_tapsi")
                .HasDefaultValue(false)
                .IsRequired();

            entity.Property(a => a.EsOpcionalTapsiDiurna)
                .HasColumnName("es_opcional_tapsi_diurna")
                .HasDefaultValue(false)
                .IsRequired();

            entity.Property(a => a.EsAreaProfesional)
                .HasColumnName("es_area_profesional")
                .HasDefaultValue(false)
                .IsRequired();

            entity.HasOne(a => a.PlanEstudio)
                .WithMany(p => p.Asignaturas)
                .HasForeignKey(a => a.IdPlan)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(a => new { a.Codigo, a.IdPlan })
                .IsUnique();

            // ── SEED: Plan Ingeniería Diurna 1020 ──────────────────────────────
            entity.HasData(
                // Semestre 1
                new Asignatura { IdAsignatura = "a1000001-a100-a100-a100-a10000000000", IdPlan = "11111111-1111-1111-1111-111111111111", Codigo = "104066", Nombre = "Matemáticas Básicas", Creditos = 4, Semestre = 1, MinEstudiantes = 15, EsAreaProfesional = false },
                new Asignatura { IdAsignatura = "a1000002-a100-a100-a100-a10000000000", IdPlan = "11111111-1111-1111-1111-111111111111", Codigo = "100010", Nombre = "Fundamentos de Ingeniería", Creditos = 2, Semestre = 1, MinEstudiantes = 15, EsAreaProfesional = false },
                new Asignatura { IdAsignatura = "a1000003-a100-a100-a100-a10000000000", IdPlan = "11111111-1111-1111-1111-111111111111", Codigo = "103004", Nombre = "Teoría de Sistemas", Creditos = 3, Semestre = 1, MinEstudiantes = 15, EsAreaProfesional = true },
                new Asignatura { IdAsignatura = "a1000004-a100-a100-a100-a10000000000", IdPlan = "11111111-1111-1111-1111-111111111111", Codigo = "103002", Nombre = "Lógica de Programación", Creditos = 3, Semestre = 1, MinEstudiantes = 15, EsAreaProfesional = true },
                new Asignatura { IdAsignatura = "a1000005-a100-a100-a100-a10000000000", IdPlan = "11111111-1111-1111-1111-111111111111", Codigo = "100059", Nombre = "Competencias Comunicativas", Creditos = 3, Semestre = 1, MinEstudiantes = 15, EsAreaProfesional = false },
                new Asignatura { IdAsignatura = "a1000006-a100-a100-a100-a10000000000", IdPlan = "11111111-1111-1111-1111-111111111111", Codigo = "105038", Nombre = "Ética", Creditos = 2, Semestre = 1, MinEstudiantes = 15, EsAreaProfesional = false },
                new Asignatura { IdAsignatura = "a1000007-a100-a100-a100-a10000000000", IdPlan = "11111111-1111-1111-1111-111111111111", Codigo = "151601", Nombre = "Inglés I", Creditos = 1, Semestre = 1, MinEstudiantes = 15, EsAreaProfesional = false },
                // Semestre 2
                new Asignatura { IdAsignatura = "a1000008-a100-a100-a100-a10000000000", IdPlan = "11111111-1111-1111-1111-111111111111", Codigo = "103007", Nombre = "Técnicas de Programación", Creditos = 3, Semestre = 2, MinEstudiantes = 15, EsAreaProfesional = true },
                new Asignatura { IdAsignatura = "a1000009-a100-a100-a100-a10000000000", IdPlan = "11111111-1111-1111-1111-111111111111", Codigo = "103008", Nombre = "Fundamentos de POO", Creditos = 3, Semestre = 2, MinEstudiantes = 15, EsAreaProfesional = true },
                // Semestre 3
                new Asignatura { IdAsignatura = "a1000010-a100-a100-a100-a10000000000", IdPlan = "11111111-1111-1111-1111-111111111111", Codigo = "104027", Nombre = "Matemáticas Discretas", Creditos = 4, Semestre = 3, MinEstudiantes = 15, EsAreaProfesional = true },
                new Asignatura { IdAsignatura = "a1000011-a100-a100-a100-a10000000000", IdPlan = "11111111-1111-1111-1111-111111111111", Codigo = "103011", Nombre = "Estructura de Datos", Creditos = 3, Semestre = 3, MinEstudiantes = 15, EsAreaProfesional = true },
                new Asignatura { IdAsignatura = "a1000012-a100-a100-a100-a10000000000", IdPlan = "11111111-1111-1111-1111-111111111111", Codigo = "103018", Nombre = "Programación Orientada a Objetos", Creditos = 3, Semestre = 3, MinEstudiantes = 15, EsAreaProfesional = true },
                // Semestre 4
                new Asignatura { IdAsignatura = "a1000013-a100-a100-a100-a10000000000", IdPlan = "11111111-1111-1111-1111-111111111111", Codigo = "103027", Nombre = "Sistemas Operativos", Creditos = 3, Semestre = 4, MinEstudiantes = 15, EsAreaProfesional = true },
                new Asignatura { IdAsignatura = "a1000014-a100-a100-a100-a10000000000", IdPlan = "11111111-1111-1111-1111-111111111111", Codigo = "109180", Nombre = "Bases de Datos I", Creditos = 3, Semestre = 4, MinEstudiantes = 15, EsAreaProfesional = true },
                new Asignatura { IdAsignatura = "a1000015-a100-a100-a100-a10000000000", IdPlan = "11111111-1111-1111-1111-111111111111", Codigo = "103022", Nombre = "Ingeniería de Software I", Creditos = 3, Semestre = 4, MinEstudiantes = 15, EsAreaProfesional = true },
                // Semestre 5
                new Asignatura { IdAsignatura = "a1000016-a100-a100-a100-a10000000000", IdPlan = "11111111-1111-1111-1111-111111111111", Codigo = "109182", Nombre = "Paradigmas de Lenguajes", Creditos = 3, Semestre = 5, MinEstudiantes = 15, EsAreaProfesional = true },
                new Asignatura { IdAsignatura = "a1000017-a100-a100-a100-a10000000000", IdPlan = "11111111-1111-1111-1111-111111111111", Codigo = "103126", Nombre = "Redes LAN", Creditos = 3, Semestre = 5, MinEstudiantes = 15, EsAreaProfesional = true },
                new Asignatura { IdAsignatura = "a1000018-a100-a100-a100-a10000000000", IdPlan = "11111111-1111-1111-1111-111111111111", Codigo = "103093", Nombre = "Ingeniería de Software II", Creditos = 3, Semestre = 5, MinEstudiantes = 15, EsAreaProfesional = true },
                new Asignatura { IdAsignatura = "a1000019-a100-a100-a100-a10000000000", IdPlan = "11111111-1111-1111-1111-111111111111", Codigo = "109183", Nombre = "Programación Backend", Creditos = 3, Semestre = 5, MinEstudiantes = 15, EsAreaProfesional = true },
                // Semestre 6
                new Asignatura { IdAsignatura = "a1000020-a100-a100-a100-a10000000000", IdPlan = "11111111-1111-1111-1111-111111111111", Codigo = "109184", Nombre = "Electrónica Digital y Arq. Computadores", Creditos = 3, Semestre = 6, MinEstudiantes = 15, EsAreaProfesional = true },
                new Asignatura { IdAsignatura = "a1000021-a100-a100-a100-a10000000000", IdPlan = "11111111-1111-1111-1111-111111111111", Codigo = "109185", Nombre = "Modelamiento y Simulación", Creditos = 3, Semestre = 6, MinEstudiantes = 15, EsAreaProfesional = true },
                new Asignatura { IdAsignatura = "a1000022-a100-a100-a100-a10000000000", IdPlan = "11111111-1111-1111-1111-111111111111", Codigo = "109186", Nombre = "Procesadores de Lenguajes", Creditos = 2, Semestre = 6, MinEstudiantes = 15, EsAreaProfesional = true },
                new Asignatura { IdAsignatura = "a1000023-a100-a100-a100-a10000000000", IdPlan = "11111111-1111-1111-1111-111111111111", Codigo = "109187", Nombre = "Programación Frontend", Creditos = 3, Semestre = 6, MinEstudiantes = 15, EsAreaProfesional = true },
                new Asignatura { IdAsignatura = "a1000024-a100-a100-a100-a10000000000", IdPlan = "11111111-1111-1111-1111-111111111111", Codigo = "103021", Nombre = "Diseño de Algoritmos", Creditos = 3, Semestre = 6, MinEstudiantes = 15, EsAreaProfesional = true },
                // Semestre 7
                new Asignatura { IdAsignatura = "a1000025-a100-a100-a100-a10000000000", IdPlan = "11111111-1111-1111-1111-111111111111", Codigo = "109104", Nombre = "Sistemas Embebidos", Creditos = 3, Semestre = 7, MinEstudiantes = 15, EsAreaProfesional = true },
                new Asignatura { IdAsignatura = "a1000026-a100-a100-a100-a10000000000", IdPlan = "11111111-1111-1111-1111-111111111111", Codigo = "103127", Nombre = "Énfasis Profesional", Creditos = 3, Semestre = 7, MinEstudiantes = 15, EsAreaProfesional = true },
                new Asignatura { IdAsignatura = "a1000027-a100-a100-a100-a10000000000", IdPlan = "11111111-1111-1111-1111-111111111111", Codigo = "109188", Nombre = "Ciencia de los Datos", Creditos = 3, Semestre = 7, MinEstudiantes = 15, EsAreaProfesional = true },
                new Asignatura { IdAsignatura = "a1000028-a100-a100-a100-a10000000000", IdPlan = "11111111-1111-1111-1111-111111111111", Codigo = "109181", Nombre = "Bases de Datos II", Creditos = 2, Semestre = 7, MinEstudiantes = 15, EsAreaProfesional = true },
                // Semestre 8
                new Asignatura { IdAsignatura = "a1000029-a100-a100-a100-a10000000000", IdPlan = "11111111-1111-1111-1111-111111111111", Codigo = "103125", Nombre = "Sistemas de Información y Organizaciones", Creditos = 3, Semestre = 8, MinEstudiantes = 15, EsAreaProfesional = true },
                new Asignatura { IdAsignatura = "a1000030-a100-a100-a100-a10000000000", IdPlan = "11111111-1111-1111-1111-111111111111", Codigo = "103117", Nombre = "Inteligencia Artificial", Creditos = 3, Semestre = 8, MinEstudiantes = 15, EsAreaProfesional = true },
                new Asignatura { IdAsignatura = "a1000031-a100-a100-a100-a10000000000", IdPlan = "11111111-1111-1111-1111-111111111111", Codigo = "109189", Nombre = "Programación de Dispositivos Móviles", Creditos = 3, Semestre = 8, MinEstudiantes = 15, EsAreaProfesional = true },
                new Asignatura { IdAsignatura = "a1000032-a100-a100-a100-a10000000000", IdPlan = "11111111-1111-1111-1111-111111111111", Codigo = "103135", Nombre = "Proyecto de Desarrollo de Software", Creditos = 2, Semestre = 8, MinEstudiantes = 15, EsAreaProfesional = true },
                // Semestre 10
                new Asignatura { IdAsignatura = "a1000033-a100-a100-a100-a10000000000", IdPlan = "11111111-1111-1111-1111-111111111111", Codigo = "103136", Nombre = "Práctica Empresarial", Creditos = 9, Semestre = 10, MinEstudiantes = 15, EsAreaProfesional = true },
                new Asignatura { IdAsignatura = "a1000034-a100-a100-a100-a10000000000", IdPlan = "11111111-1111-1111-1111-111111111111", Codigo = "103118", Nombre = "Gerencia de Proyectos Tecnológicos", Creditos = 2, Semestre = 10, MinEstudiantes = 15, EsAreaProfesional = true },

                // ── SEED: Plan Ingeniería Nocturna 1030 ───────────────────────────
                // Semestre 1
                new Asignatura { IdAsignatura = "a2000001-a200-a200-a200-a20000000000", IdPlan = "22222222-2222-2222-2222-222222222222", Codigo = "104066", Nombre = "Matemáticas Básicas", Creditos = 4, Semestre = 1, MinEstudiantes = 15, EsAreaProfesional = false },
                new Asignatura { IdAsignatura = "a2000002-a200-a200-a200-a20000000000", IdPlan = "22222222-2222-2222-2222-222222222222", Codigo = "100010", Nombre = "Fundamentos de Ingeniería", Creditos = 2, Semestre = 1, MinEstudiantes = 15, EsAreaProfesional = false },
                new Asignatura { IdAsignatura = "a2000003-a200-a200-a200-a20000000000", IdPlan = "22222222-2222-2222-2222-222222222222", Codigo = "104029", Nombre = "Álgebra Lineal", Creditos = 3, Semestre = 1, MinEstudiantes = 15, EsAreaProfesional = false },
                new Asignatura { IdAsignatura = "a2000004-a200-a200-a200-a20000000000", IdPlan = "22222222-2222-2222-2222-222222222222", Codigo = "103002", Nombre = "Lógica de Programación", Creditos = 3, Semestre = 1, MinEstudiantes = 15, EsAreaProfesional = true },
                new Asignatura { IdAsignatura = "a2000005-a200-a200-a200-a20000000000", IdPlan = "22222222-2222-2222-2222-222222222222", Codigo = "105038", Nombre = "Ética", Creditos = 2, Semestre = 1, MinEstudiantes = 15, EsAreaProfesional = false },
                new Asignatura { IdAsignatura = "a2000006-a200-a200-a200-a20000000000", IdPlan = "22222222-2222-2222-2222-222222222222", Codigo = "151601", Nombre = "Inglés I", Creditos = 1, Semestre = 1, MinEstudiantes = 15, EsAreaProfesional = false },
                // Semestre 2
                new Asignatura { IdAsignatura = "a2000007-a200-a200-a200-a20000000000", IdPlan = "22222222-2222-2222-2222-222222222222", Codigo = "103004", Nombre = "Teoría de Sistemas", Creditos = 3, Semestre = 2, MinEstudiantes = 15, EsAreaProfesional = true },
                new Asignatura { IdAsignatura = "a2000008-a200-a200-a200-a20000000000", IdPlan = "22222222-2222-2222-2222-222222222222", Codigo = "103007", Nombre = "Técnicas de Programación", Creditos = 3, Semestre = 2, MinEstudiantes = 15, EsAreaProfesional = true },
                new Asignatura { IdAsignatura = "a2000009-a200-a200-a200-a20000000000", IdPlan = "22222222-2222-2222-2222-222222222222", Codigo = "103008", Nombre = "Fundamentos de POO", Creditos = 3, Semestre = 2, MinEstudiantes = 15, EsAreaProfesional = true },
                // Semestre 3
                new Asignatura { IdAsignatura = "a2000010-a200-a200-a200-a20000000000", IdPlan = "22222222-2222-2222-2222-222222222222", Codigo = "104027", Nombre = "Matemáticas Discretas", Creditos = 4, Semestre = 3, MinEstudiantes = 15, EsAreaProfesional = true },
                new Asignatura { IdAsignatura = "a2000011-a200-a200-a200-a20000000000", IdPlan = "22222222-2222-2222-2222-222222222222", Codigo = "103018", Nombre = "Programación Orientada a Objetos", Creditos = 3, Semestre = 3, MinEstudiantes = 15, EsAreaProfesional = true },
                // Semestre 4
                new Asignatura { IdAsignatura = "a2000012-a200-a200-a200-a20000000000", IdPlan = "22222222-2222-2222-2222-222222222222", Codigo = "103011", Nombre = "Estructura de Datos", Creditos = 3, Semestre = 4, MinEstudiantes = 15, EsAreaProfesional = true },
                new Asignatura { IdAsignatura = "a2000013-a200-a200-a200-a20000000000", IdPlan = "22222222-2222-2222-2222-222222222222", Codigo = "103022", Nombre = "Ingeniería de Software I", Creditos = 3, Semestre = 4, MinEstudiantes = 15, EsAreaProfesional = true },
                // Semestre 5
                new Asignatura { IdAsignatura = "a2000014-a200-a200-a200-a20000000000", IdPlan = "22222222-2222-2222-2222-222222222222", Codigo = "109180", Nombre = "Bases de Datos I", Creditos = 3, Semestre = 5, MinEstudiantes = 15, EsAreaProfesional = true },
                // Semestre 6
                new Asignatura { IdAsignatura = "a2000015-a200-a200-a200-a20000000000", IdPlan = "22222222-2222-2222-2222-222222222222", Codigo = "109184", Nombre = "Electrónica Digital y Arq. Computadores", Creditos = 3, Semestre = 6, MinEstudiantes = 15, EsAreaProfesional = true },
                new Asignatura { IdAsignatura = "a2000016-a200-a200-a200-a20000000000", IdPlan = "22222222-2222-2222-2222-222222222222", Codigo = "109182", Nombre = "Paradigmas de Lenguajes", Creditos = 3, Semestre = 6, MinEstudiantes = 15, EsAreaProfesional = true },
                new Asignatura { IdAsignatura = "a2000017-a200-a200-a200-a20000000000", IdPlan = "22222222-2222-2222-2222-222222222222", Codigo = "103126", Nombre = "Redes LAN", Creditos = 3, Semestre = 6, MinEstudiantes = 15, EsAreaProfesional = true },
                new Asignatura { IdAsignatura = "a2000018-a200-a200-a200-a20000000000", IdPlan = "22222222-2222-2222-2222-222222222222", Codigo = "109183", Nombre = "Programación Backend", Creditos = 3, Semestre = 6, MinEstudiantes = 15, EsAreaProfesional = true },
                new Asignatura { IdAsignatura = "a2000019-a200-a200-a200-a20000000000", IdPlan = "22222222-2222-2222-2222-222222222222", Codigo = "103027", Nombre = "Sistemas Operativos", Creditos = 3, Semestre = 6, MinEstudiantes = 15, EsAreaProfesional = true },
                // Semestre 7
                new Asignatura { IdAsignatura = "a2000020-a200-a200-a200-a20000000000", IdPlan = "22222222-2222-2222-2222-222222222222", Codigo = "109104", Nombre = "Sistemas Embebidos", Creditos = 3, Semestre = 7, MinEstudiantes = 15, EsAreaProfesional = true },
                new Asignatura { IdAsignatura = "a2000021-a200-a200-a200-a20000000000", IdPlan = "22222222-2222-2222-2222-222222222222", Codigo = "109186", Nombre = "Procesadores de Lenguajes", Creditos = 2, Semestre = 7, MinEstudiantes = 15, EsAreaProfesional = true },
                new Asignatura { IdAsignatura = "a2000022-a200-a200-a200-a20000000000", IdPlan = "22222222-2222-2222-2222-222222222222", Codigo = "109185", Nombre = "Modelamiento y Simulación", Creditos = 3, Semestre = 7, MinEstudiantes = 15, EsAreaProfesional = true },
                new Asignatura { IdAsignatura = "a2000023-a200-a200-a200-a20000000000", IdPlan = "22222222-2222-2222-2222-222222222222", Codigo = "109187", Nombre = "Programación Frontend", Creditos = 3, Semestre = 7, MinEstudiantes = 15, EsAreaProfesional = true },
                new Asignatura { IdAsignatura = "a2000024-a200-a200-a200-a20000000000", IdPlan = "22222222-2222-2222-2222-222222222222", Codigo = "103093", Nombre = "Ingeniería de Software II", Creditos = 3, Semestre = 7, MinEstudiantes = 15, EsAreaProfesional = true },
                // Semestre 8
                new Asignatura { IdAsignatura = "a2000025-a200-a200-a200-a20000000000", IdPlan = "22222222-2222-2222-2222-222222222222", Codigo = "109188", Nombre = "Ciencia de los Datos", Creditos = 3, Semestre = 8, MinEstudiantes = 15, EsAreaProfesional = true },
                new Asignatura { IdAsignatura = "a2000026-a200-a200-a200-a20000000000", IdPlan = "22222222-2222-2222-2222-222222222222", Codigo = "103021", Nombre = "Diseño de Algoritmos", Creditos = 3, Semestre = 8, MinEstudiantes = 15, EsAreaProfesional = true },
                new Asignatura { IdAsignatura = "a2000027-a200-a200-a200-a20000000000", IdPlan = "22222222-2222-2222-2222-222222222222", Codigo = "109181", Nombre = "Bases de Datos II", Creditos = 2, Semestre = 8, MinEstudiantes = 15, EsAreaProfesional = true },
                // Semestre 9
                new Asignatura { IdAsignatura = "a2000028-a200-a200-a200-a20000000000", IdPlan = "22222222-2222-2222-2222-222222222222", Codigo = "103127", Nombre = "Énfasis Profesional", Creditos = 3, Semestre = 9, MinEstudiantes = 15, EsAreaProfesional = true },
                new Asignatura { IdAsignatura = "a2000029-a200-a200-a200-a20000000000", IdPlan = "22222222-2222-2222-2222-222222222222", Codigo = "109189", Nombre = "Programación de Dispositivos Móviles", Creditos = 3, Semestre = 9, MinEstudiantes = 15, EsAreaProfesional = true },
                new Asignatura { IdAsignatura = "a2000030-a200-a200-a200-a20000000000", IdPlan = "22222222-2222-2222-2222-222222222222", Codigo = "103135", Nombre = "Proyecto de Desarrollo de Software", Creditos = 2, Semestre = 9, MinEstudiantes = 15, EsAreaProfesional = true },
                // Semestre 10
                new Asignatura { IdAsignatura = "a2000031-a200-a200-a200-a20000000000", IdPlan = "22222222-2222-2222-2222-222222222222", Codigo = "103125", Nombre = "Sistemas de Información y Organizaciones", Creditos = 3, Semestre = 10, MinEstudiantes = 15, EsAreaProfesional = true },
                new Asignatura { IdAsignatura = "a2000032-a200-a200-a200-a20000000000", IdPlan = "22222222-2222-2222-2222-222222222222", Codigo = "103117", Nombre = "Inteligencia Artificial", Creditos = 3, Semestre = 10, MinEstudiantes = 15, EsAreaProfesional = true },
                // Semestre 12
                new Asignatura { IdAsignatura = "a2000033-a200-a200-a200-a20000000000", IdPlan = "22222222-2222-2222-2222-222222222222", Codigo = "103118", Nombre = "Gerencia de Proyectos Tecnológicos", Creditos = 2, Semestre = 12, MinEstudiantes = 15, EsAreaProfesional = true },
                new Asignatura { IdAsignatura = "a2000034-a200-a200-a200-a20000000000", IdPlan = "22222222-2222-2222-2222-222222222222", Codigo = "103136", Nombre = "Práctica Empresarial", Creditos = 9, Semestre = 12, MinEstudiantes = 15, EsAreaProfesional = true },

                // ── SEED: Plan TAPSI Diurna ────────────────────────────────────────
                // Fijas
                new Asignatura { IdAsignatura = "a3000001-a300-a300-a300-a30000000000", IdPlan = "33333333-3333-3333-3333-333333333333", Codigo = "104030", Nombre = "Cálculo Diferencial", Creditos = 3, Semestre = 1, MinEstudiantes = 15, EsAreaProfesional = true, EsFijaTapsi = true },
                new Asignatura { IdAsignatura = "a3000002-a300-a300-a300-a30000000000", IdPlan = "33333333-3333-3333-3333-333333333333", Codigo = "103007", Nombre = "Técnicas de Programación", Creditos = 3, Semestre = 1, MinEstudiantes = 15, EsAreaProfesional = true, EsFijaTapsi = true },
                new Asignatura { IdAsignatura = "a3000003-a300-a300-a300-a30000000000", IdPlan = "33333333-3333-3333-3333-333333333333", Codigo = "103018", Nombre = "Programación Orientada a Objetos", Creditos = 3, Semestre = 1, MinEstudiantes = 15, EsAreaProfesional = true, EsFijaTapsi = true },
                new Asignatura { IdAsignatura = "a3000004-a300-a300-a300-a30000000000", IdPlan = "33333333-3333-3333-3333-333333333333", Codigo = "103004", Nombre = "Teoría de Sistemas", Creditos = 3, Semestre = 1, MinEstudiantes = 15, EsAreaProfesional = true, EsFijaTapsi = true },
                new Asignatura { IdAsignatura = "a3000005-a300-a300-a300-a30000000000", IdPlan = "33333333-3333-3333-3333-333333333333", Codigo = "103027", Nombre = "Sistemas Operativos", Creditos = 3, Semestre = 1, MinEstudiantes = 15, EsAreaProfesional = true, EsFijaTapsi = true },
                // Opcionales TAPSI diurna
                new Asignatura { IdAsignatura = "a3000006-a300-a300-a300-a30000000000", IdPlan = "33333333-3333-3333-3333-333333333333", Codigo = "103093", Nombre = "Ingeniería de Software II", Creditos = 3, Semestre = 1, MinEstudiantes = 15, EsAreaProfesional = true, EsOpcionalTapsiDiurna = true },
                new Asignatura { IdAsignatura = "a3000007-a300-a300-a300-a30000000000", IdPlan = "33333333-3333-3333-3333-333333333333", Codigo = "103126", Nombre = "Redes LAN", Creditos = 3, Semestre = 1, MinEstudiantes = 15, EsAreaProfesional = true, EsOpcionalTapsiDiurna = true },
                new Asignatura { IdAsignatura = "a3000008-a300-a300-a300-a30000000000", IdPlan = "33333333-3333-3333-3333-333333333333", Codigo = "109183", Nombre = "Programación Backend", Creditos = 3, Semestre = 1, MinEstudiantes = 15, EsAreaProfesional = true, EsOpcionalTapsiDiurna = true },

                // ── SEED: Plan TAPSI Nocturna ──────────────────────────────────────
                new Asignatura { IdAsignatura = "a4000001-a400-a400-a400-a40000000000", IdPlan = "44444444-4444-4444-4444-444444444444", Codigo = "104030", Nombre = "Cálculo Diferencial", Creditos = 3, Semestre = 1, MinEstudiantes = 15, EsAreaProfesional = true, EsFijaTapsi = true },
                new Asignatura { IdAsignatura = "a4000002-a400-a400-a400-a40000000000", IdPlan = "44444444-4444-4444-4444-444444444444", Codigo = "103007", Nombre = "Técnicas de Programación", Creditos = 3, Semestre = 1, MinEstudiantes = 15, EsAreaProfesional = true, EsFijaTapsi = true },
                new Asignatura { IdAsignatura = "a4000003-a400-a400-a400-a40000000000", IdPlan = "44444444-4444-4444-4444-444444444444", Codigo = "103018", Nombre = "Programación Orientada a Objetos", Creditos = 3, Semestre = 1, MinEstudiantes = 15, EsAreaProfesional = true, EsFijaTapsi = true },
                new Asignatura { IdAsignatura = "a4000004-a400-a400-a400-a40000000000", IdPlan = "44444444-4444-4444-4444-444444444444", Codigo = "103004", Nombre = "Teoría de Sistemas", Creditos = 3, Semestre = 1, MinEstudiantes = 15, EsAreaProfesional = true, EsFijaTapsi = true },
                new Asignatura { IdAsignatura = "a4000005-a400-a400-a400-a40000000000", IdPlan = "44444444-4444-4444-4444-444444444444", Codigo = "103027", Nombre = "Sistemas Operativos", Creditos = 3, Semestre = 1, MinEstudiantes = 15, EsAreaProfesional = true, EsFijaTapsi = true }
            );
        });
    }

    private static void ConfigurarDisponibilidad(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Disponibilidad>(entity =>
        {
            entity.ToTable("disponibilidad");

            entity.HasKey(d => d.IdDisponibilidad);

            entity.Property(d => d.IdDisponibilidad)
                .HasColumnName("id_disponibilidad")
                .HasMaxLength(36)
                .IsRequired();

            entity.Property(d => d.IdDocente)
                .HasColumnName("id_docente")
                .HasMaxLength(36)
                .IsRequired();

            entity.Property(d => d.DiaSemana)
                .HasColumnName("dia_semana")
                .IsRequired();

            entity.Property(d => d.HoraInicio)
                .HasColumnName("hora_inicio")
                .HasMaxLength(5)
                .IsRequired();

            entity.Property(d => d.HoraFin)
                .HasColumnName("hora_fin")
                .HasMaxLength(5)
                .IsRequired();

            entity.HasOne(d => d.Docente)
                .WithMany(docente => docente.Disponibilidades)
                .HasForeignKey(d => d.IdDocente)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigurarAsignaciones(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Asignacion>(entity =>
        {
            entity.ToTable("asignaciones");

            entity.HasKey(a => a.IdAsignacion);

            entity.Property(a => a.IdAsignacion)
                .HasColumnName("id_asignacion")
                .HasMaxLength(36)
                .IsRequired();

            entity.Property(a => a.IdDocente)
                .HasColumnName("id_docente")
                .HasMaxLength(36)
                .IsRequired();

            entity.Property(a => a.IdAsignatura)
                .HasColumnName("id_asignatura")
                .HasMaxLength(36)
                .IsRequired();

            entity.Property(a => a.Dia)
                .HasColumnName("dia")
                .IsRequired();

            entity.Property(a => a.HoraInicio)
                .HasColumnName("hora_inicio")
                .HasMaxLength(5)
                .IsRequired();

            entity.Property(a => a.HoraFin)
                .HasColumnName("hora_fin")
                .HasMaxLength(5)
                .IsRequired();

            entity.Property(a => a.Periodo)
                .HasColumnName("periodo")
                .HasMaxLength(10)
                .IsRequired();

            entity.Property(a => a.Estado)
                .HasColumnName("estado")
                .HasMaxLength(20)
                .HasDefaultValue("Propuesta")
                .IsRequired();

            entity.Property(a => a.Escenario)
                .HasColumnName("escenario")
                .HasMaxLength(30)
                .HasDefaultValue("ING_DIURNA")
                .IsRequired();

            entity.HasOne(a => a.Docente)
                .WithMany(d => d.Asignaciones)
                .HasForeignKey(a => a.IdDocente)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.Asignatura)
                .WithMany(a => a.Asignaciones)
                .HasForeignKey(a => a.IdAsignatura)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
    private static void ConfigurarBloqueosFranjaAsignatura(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BloqueoFranjaAsignatura>(entity =>
        {
            entity.ToTable("bloqueos_franja_asignatura");

            entity.HasKey(b => b.IdBloqueo);

            entity.Property(b => b.IdBloqueo)
                .HasColumnName("id_bloqueo")
                .HasMaxLength(36)
                .IsRequired();

            entity.Property(b => b.IdAsignatura)
                .HasColumnName("id_asignatura")
                .HasMaxLength(36)
                .IsRequired();

            entity.Property(b => b.Periodo)
                .HasColumnName("periodo")
                .HasMaxLength(10)
                .IsRequired();

            entity.Property(b => b.Dia)
                .HasColumnName("dia")
                .IsRequired();

            entity.Property(b => b.HoraInicio)
                .HasColumnName("hora_inicio")
                .HasMaxLength(5)
                .IsRequired();

            entity.Property(b => b.HoraFin)
                .HasColumnName("hora_fin")
                .HasMaxLength(5)
                .IsRequired();

            entity.Property(b => b.Motivo)
                .HasColumnName("motivo")
                .HasMaxLength(250);

            entity.Property(b => b.FechaCreacionUtc)
                .HasColumnName("fecha_creacion_utc")
                .IsRequired();

            entity.HasOne(b => b.Asignatura)
                .WithMany(a => a.BloqueosFranja)
                .HasForeignKey(b => b.IdAsignatura)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(b => new
            {
                b.IdAsignatura,
                b.Periodo,
                b.Dia,
                b.HoraInicio,
                b.HoraFin
            }).IsUnique();
        });
    }
}