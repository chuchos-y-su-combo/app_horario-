using ApplicationSchedule.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApplicationSchedule.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
    public DbSet<Asignatura> Asignaturas => Set<Asignatura>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Profesor> Profesores => Set<Profesor>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigurarAsignaturas(modelBuilder);
        ConfigurarRoles(modelBuilder);
        ConfigurarUsuarios(modelBuilder);
        ConfigurarProfesores(modelBuilder);
    }
    private static void ConfigurarAsignaturas(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Asignatura>(entity =>
        {
            entity.ToTable("Asignaturas");

            entity.HasKey(a => a.IdAsignatura);

            entity.Property(a => a.IdAsignatura)
                .HasColumnName("id_asignatura")
                .ValueGeneratedOnAdd();

            entity.Property(a => a.IdPlanEstudios)
                .HasColumnName("id_plan_estudios")
                .IsRequired();

            entity.Property(a => a.Codigo)
                .HasColumnName("codigo")
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(a => a.Nombre)
                .HasColumnName("nombre")
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(a => a.Creditos)
                .HasColumnName("creditos")
                .IsRequired();

            entity.Property(a => a.Semestre)
                .HasColumnName("semestre")
                .IsRequired();

            entity.Property(a => a.EsFijaTapsi)
                .HasColumnName("es_fija_tapsi")
                .HasDefaultValue(false)
                .IsRequired();

            entity.HasIndex(a => a.Codigo)
                .IsUnique();
        });
    }

    private static void ConfigurarRoles(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Rol>(entity =>
        {
            entity.ToTable("Roles");

            entity.HasKey(r => r.IdRol);

            entity.Property(r => r.IdRol)
                .HasColumnName("id_rol");

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
            entity.ToTable("Usuarios");

            entity.HasKey(u => u.IdUsuario);

            entity.Property(u => u.IdUsuario)
                .HasColumnName("id_usuario")
                .HasColumnType("char(36)")
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

    private static void ConfigurarProfesores(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Profesor>(entity =>
        {
            entity.ToTable("Profesores");

            entity.HasKey(p => p.IdProfesor);

            entity.Property(p => p.IdProfesor)
                .HasColumnName("id_profesor");

            entity.Property(p => p.Nombre)
                .HasColumnName("nombre")
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(p => p.Identificacion)
                .HasColumnName("identificacion")
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(p => p.TipoContrato)
                .HasColumnName("tipo_contrato")
                .HasMaxLength(50)
                .IsRequired();

            entity.HasIndex(p => p.Identificacion)
                .IsUnique(); 
        });
    }
}