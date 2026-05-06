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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigurarRoles(modelBuilder);
        ConfigurarUsuarios(modelBuilder);
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
}