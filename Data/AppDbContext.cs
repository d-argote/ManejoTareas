using Microsoft.EntityFrameworkCore;
using ManejoTareas.Models;
using ManejoTareas.Helpers;

namespace ManejoTareas.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Tarea> Tareas => Set<Tarea>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Permiso> Permisos => Set<Permiso>();
    public DbSet<UsuarioRol> UsuarioRoles => Set<UsuarioRol>();
    public DbSet<RolPermiso> RolPermisos => Set<RolPermiso>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Tareas
        modelBuilder.Entity<Tarea>(entity =>
        {
            entity.ToTable("tareas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.Titulo).HasColumnName("titulo").IsRequired().HasMaxLength(200);
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.Completada).HasColumnName("completada").HasDefaultValue(false);
            entity.Property(e => e.FechaCreacion).HasColumnName("fecha_creacion").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.FechaActualizacion).HasColumnName("fecha_actualizacion");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");
            entity.HasOne(e => e.Usuario).WithMany().HasForeignKey(e => e.UsuarioId).OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(e => e.UsuarioId);
        });

        // Usuarios
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("usuarios");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.Nombre).HasColumnName("nombre").IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).HasColumnName("email").IsRequired().HasMaxLength(150);
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash").IsRequired();
            entity.Property(e => e.Activo).HasColumnName("activo").HasDefaultValue(true);
            entity.Property(e => e.FechaCreacion).HasColumnName("fecha_creacion").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.FechaActualizacion).HasColumnName("fecha_actualizacion");
            entity.Property(e => e.UltimoAcceso).HasColumnName("ultimo_acceso");
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Roles
        modelBuilder.Entity<Rol>(entity =>
        {
            entity.ToTable("roles");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.Nombre).HasColumnName("nombre").IsRequired().HasMaxLength(50);
            entity.Property(e => e.Descripcion).HasColumnName("descripcion").HasMaxLength(200);
            entity.HasIndex(e => e.Nombre).IsUnique();
        });

        // Permisos
        modelBuilder.Entity<Permiso>(entity =>
        {
            entity.ToTable("permisos");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.Clave).HasColumnName("clave").IsRequired().HasMaxLength(100);
            entity.Property(e => e.Descripcion).HasColumnName("descripcion").HasMaxLength(200);
            entity.Property(e => e.Grupo).HasColumnName("grupo").HasMaxLength(50);
            entity.HasIndex(e => e.Clave).IsUnique();
        });

        // UsuarioRol (muchos a muchos)
        modelBuilder.Entity<UsuarioRol>(entity =>
        {
            entity.ToTable("usuario_roles");
            entity.HasKey(e => new { e.UsuarioId, e.RolId });
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");
            entity.Property(e => e.RolId).HasColumnName("rol_id");
            entity.Property(e => e.FechaAsignacion).HasColumnName("fecha_asignacion").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.AsignadoPor).HasColumnName("asignado_por");
            entity.HasOne(e => e.Usuario).WithMany(u => u.UsuarioRoles).HasForeignKey(e => e.UsuarioId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Rol).WithMany(r => r.UsuarioRoles).HasForeignKey(e => e.RolId).OnDelete(DeleteBehavior.Cascade);
        });

        // RolPermiso
        modelBuilder.Entity<RolPermiso>(entity =>
        {
            entity.ToTable("rol_permisos");
            entity.HasKey(e => new { e.RolId, e.PermisoId });
            entity.Property(e => e.RolId).HasColumnName("rol_id");
            entity.Property(e => e.PermisoId).HasColumnName("permiso_id");
            entity.HasOne(e => e.Rol).WithMany(r => r.RolPermisos).HasForeignKey(e => e.RolId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Permiso).WithMany(p => p.RolPermisos).HasForeignKey(e => e.PermisoId).OnDelete(DeleteBehavior.Cascade);
        });

        // Seed de Roles
        modelBuilder.Entity<Rol>().HasData(
            new Rol { Id = 1, Nombre = "Administrador", Descripcion = "Acceso total, gestiona usuarios y permisos" },
            new Rol { Id = 2, Nombre = "Editor", Descripcion = "Puede crear y editar tareas, no eliminar" },
            new Rol { Id = 3, Nombre = "Lector", Descripcion = "Solo puede ver tareas" }
        );

        // Seed de Permisos (usar fully qualified para evitar conflicto con DbSet Permisos)
        modelBuilder.Entity<Permiso>().HasData(
            new Permiso { Id = 1, Clave = global::ManejoTareas.Helpers.Permisos.TareasVer, Descripcion = "Ver tareas", Grupo = "tareas" },
            new Permiso { Id = 2, Clave = global::ManejoTareas.Helpers.Permisos.TareasCrear, Descripcion = "Crear tareas", Grupo = "tareas" },
            new Permiso { Id = 3, Clave = global::ManejoTareas.Helpers.Permisos.TareasEditar, Descripcion = "Editar tareas", Grupo = "tareas" },
            new Permiso { Id = 4, Clave = global::ManejoTareas.Helpers.Permisos.TareasEliminar, Descripcion = "Eliminar tareas", Grupo = "tareas" },
            new Permiso { Id = 5, Clave = global::ManejoTareas.Helpers.Permisos.TareasCompletar, Descripcion = "Marcar tareas como completadas", Grupo = "tareas" },
            new Permiso { Id = 6, Clave = global::ManejoTareas.Helpers.Permisos.UsuariosVer, Descripcion = "Ver usuarios", Grupo = "usuarios" },
            new Permiso { Id = 7, Clave = global::ManejoTareas.Helpers.Permisos.UsuariosCrear, Descripcion = "Crear usuarios", Grupo = "usuarios" },
            new Permiso { Id = 8, Clave = global::ManejoTareas.Helpers.Permisos.UsuariosEditar, Descripcion = "Editar usuarios", Grupo = "usuarios" },
            new Permiso { Id = 9, Clave = global::ManejoTareas.Helpers.Permisos.UsuariosEliminar, Descripcion = "Eliminar usuarios", Grupo = "usuarios" },
            new Permiso { Id = 10, Clave = global::ManejoTareas.Helpers.Permisos.UsuariosGestionarPermisos, Descripcion = "Gestionar permisos/roles", Grupo = "usuarios" }
        );

        // Seed RolPermisos
        // Admin: todos
        modelBuilder.Entity<RolPermiso>().HasData(
            new RolPermiso { RolId = 1, PermisoId = 1 },
            new RolPermiso { RolId = 1, PermisoId = 2 },
            new RolPermiso { RolId = 1, PermisoId = 3 },
            new RolPermiso { RolId = 1, PermisoId = 4 },
            new RolPermiso { RolId = 1, PermisoId = 5 },
            new RolPermiso { RolId = 1, PermisoId = 6 },
            new RolPermiso { RolId = 1, PermisoId = 7 },
            new RolPermiso { RolId = 1, PermisoId = 8 },
            new RolPermiso { RolId = 1, PermisoId = 9 },
            new RolPermiso { RolId = 1, PermisoId = 10 },
            // Editor: ver, crear, editar, completar
            new RolPermiso { RolId = 2, PermisoId = 1 },
            new RolPermiso { RolId = 2, PermisoId = 2 },
            new RolPermiso { RolId = 2, PermisoId = 3 },
            new RolPermiso { RolId = 2, PermisoId = 5 },
            // Lector: solo ver
            new RolPermiso { RolId = 3, PermisoId = 1 }
        );

        // Nota: Usuario admin se crea via migracion SQL con password hasheado dinámicamente
        // o via seed en Program.cs al iniciar. No se hace HasData aqui porque el hash cambia.
    }

    /// <summary>
    /// Ejecuta SET LOCAL para RLS. Llamar al inicio de cada request autenticado.
    /// </summary>
    public async Task SetRlsContextAsync(int? userId, bool isAdmin, CancellationToken ct = default)
    {
        var idStr = userId?.ToString() ?? "";
        var adminStr = isAdmin ? "1" : "0";
        // Usamos set_config con is_local=false para que persista en la conexion pooled durante el request
        try
        {
            await Database.ExecuteSqlRawAsync("SELECT set_config('app.current_user_id', {0}, false)", new[] { idStr }, ct);
            await Database.ExecuteSqlRawAsync("SELECT set_config('app.is_admin', {0}, false)", new[] { adminStr }, ct);
        }
        catch
        {
            // Si la DB no esta disponible, no bloqueamos el request
        }
    }
}
