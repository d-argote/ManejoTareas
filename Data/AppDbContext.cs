using Microsoft.EntityFrameworkCore;
using ManejoTareas.Models;

namespace ManejoTareas.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Tarea> Tareas => Set<Tarea>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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
        });
    }
}