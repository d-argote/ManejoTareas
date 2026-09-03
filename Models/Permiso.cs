using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManejoTareas.Models;

[Table("permisos")]
public class Permiso
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("clave")]
    public string Clave { get; set; } = string.Empty; // ej: tareas.crear, tareas.eliminar

    [MaxLength(200)]
    [Column("descripcion")]
    public string? Descripcion { get; set; }

    [MaxLength(50)]
    [Column("grupo")]
    public string? Grupo { get; set; } // ej: tareas, usuarios

    public ICollection<RolPermiso> RolPermisos { get; set; } = new List<RolPermiso>();
}
