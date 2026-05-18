using ApplicationSchedule.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApplicationSchedule.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<PlanEstudio> PlanesEstudio => Set<PlanEstudio>();
    public DbSet<Docente> Docentes => Set<Docente>();
    public DbSet<Asignatura> Asignaturas => Set<Asignatura>();
    public DbSet<DocenteHabilitado> DocentesHabilitados => Set<DocenteHabilitado>();
    public DbSet<Disponibilidad> Disponibilidades => Set<Disponibilidad>();   
    public DbSet<Asignacion> Asignaciones => Set<Asignacion>();
    public DbSet<BloqueoFranjaAsignatura> BloqueosFranjaAsignatura => Set<BloqueoFranjaAsignatura>();

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
                    NombrePlan = "Plan de Estudios Jornada Noche",
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

            entity.HasOne(a => a.PlanEstudio)
                .WithMany(p => p.Asignaturas)
                .HasForeignKey(a => a.IdPlan)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(a => a.Codigo)
                .IsUnique();
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