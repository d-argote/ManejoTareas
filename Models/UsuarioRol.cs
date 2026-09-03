using System.ComponentModel.DataAnnotations.Schema;

namespace ManejoTareas.Models;

[Table("usuario_roles")]
public class UsuarioRol
{
    [Column("usuario_id")]
    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    [Column("rol_id")]
    public int RolId { get; set; }
    public Rol Rol { get; set; } = null!;

    [Column("fecha_asignacion")]
    public DateTime FechaAsignacion { get; set; } = DateTime.UtcNow;

    [Column("asignado_por")]
    public int? AsignadoPor { get; set; }
}
