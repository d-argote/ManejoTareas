using System.ComponentModel.DataAnnotations;

namespace ManejoTareas.DTOs;

public class UsuarioDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? UltimoAcceso { get; set; }
    public List<string> Roles { get; set; } = new();
    public List<string> Permisos { get; set; } = new();
}

public class CrearUsuarioDto
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "Email invalido")]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contrasena es obligatoria")]
    [MinLength(6, ErrorMessage = "Minimo 6 caracteres")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Compare(nameof(Password), ErrorMessage = "Las contrasenas no coinciden")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public List<int> RolesIds { get; set; } = new();
}

public class EditarUsuarioDto
{
    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;

    // Password opcional al editar
    [MinLength(6, ErrorMessage = "Minimo 6 caracteres")]
    public string? NuevoPassword { get; set; }

    public List<int> RolesIds { get; set; } = new();
}

public class LoginDto
{
    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contrasena es obligatoria")]
    public string Password { get; set; } = string.Empty;

    public bool Recordarme { get; set; }
}

public class RegistroDto
{
    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class AsignarRolesDto
{
    public int UsuarioId { get; set; }
    public List<int> RolesIds { get; set; } = new();
}
