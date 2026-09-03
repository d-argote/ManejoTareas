using System.ComponentModel.DataAnnotations;

namespace ManejoTareas.DTOs;

public class TareaDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El titulo es obligatorio")]
    [MaxLength(200, ErrorMessage = "El titulo no puede exceder 200 caracteres")]
    public string Titulo { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public bool Completada { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }
}

public class CrearTareaDto
{
    [Required(ErrorMessage = "El titulo es obligatorio")]
    [MaxLength(200, ErrorMessage = "El titulo no puede exceder 200 caracteres")]
    public string Titulo { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;
}

public class ActualizarTareaDto
{
    [Required(ErrorMessage = "El titulo es obligatorio")]
    [MaxLength(200, ErrorMessage = "El titulo no puede exceder 200 caracteres")]
    public string Titulo { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public bool Completada { get; set; }
}