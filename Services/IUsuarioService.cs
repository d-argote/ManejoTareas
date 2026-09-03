using ManejoTareas.DTOs;

namespace ManejoTareas.Services;

public interface IUsuarioService
{
    Task<List<UsuarioDto>> ObtenerTodosAsync();
    Task<UsuarioDto?> ObtenerPorIdAsync(int id);
    Task<UsuarioDto?> ObtenerPorEmailAsync(string email);
    Task<(bool Ok, string? Error, UsuarioDto? Usuario)> CrearAsync(CrearUsuarioDto dto, int? creadoPorId = null);
    Task<(bool Ok, string? Error)> ActualizarAsync(int id, EditarUsuarioDto dto);
    Task<(bool Ok, string? Error)> EliminarAsync(int id, int currentUserId);
    Task<List<RolDto>?> ObtenerRolesAsync();
    Task<bool> AsignarRolesAsync(int usuarioId, List<int> rolesIds, int? asignadoPor = null);
    Task<bool> VerificarPasswordAsync(string email, string password);
    Task ActualizarUltimoAccesoAsync(int userId);
}

public class RolDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public List<string> Permisos { get; set; } = new();
}
