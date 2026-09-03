using System.ComponentModel.DataAnnotations.Schema;

namespace ManejoTareas.Models;

[Table("rol_permisos")]
public class RolPermiso
{
    [Column("rol_id")]
    public int RolId { get; set; }
    public Rol Rol { get; set; } = null!;

    [Column("permiso_id")]
    public int PermisoId { get; set; }
    public Permiso Permiso { get; set; } = null!;
}
