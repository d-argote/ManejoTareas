using Microsoft.EntityFrameworkCore;
using ManejoTareas.Data;
using ManejoTareas.DTOs;
using ManejoTareas.Models;
using ManejoTareas.Helpers;

namespace ManejoTareas.Services;

public class UsuarioService : IUsuarioService
{
    private readonly AppDbContext _context;

    public UsuarioService(AppDbContext context) => _context = context;

    public async Task<List<UsuarioDto>> ObtenerTodosAsync()
    {
        var usuarios = await _context.Usuarios
            .Include(u => u.UsuarioRoles).ThenInclude(ur => ur.Rol).ThenInclude(r => r.RolPermisos).ThenInclude(rp => rp.Permiso)
            .OrderBy(u => u.Nombre)
            .ToListAsync();

        return usuarios.Select(MapToDto).ToList();
    }

    public async Task<UsuarioDto?> ObtenerPorIdAsync(int id)
    {
        var u = await _context.Usuarios
            .Include(x => x.UsuarioRoles).ThenInclude(ur => ur.Rol).ThenInclude(r => r.RolPermisos).ThenInclude(rp => rp.Permiso)
            .FirstOrDefaultAsync(x => x.Id == id);
        return u == null ? null : MapToDto(u);
    }

    public async Task<UsuarioDto?> ObtenerPorEmailAsync(string email)
    {
        var u = await _context.Usuarios
            .Include(x => x.UsuarioRoles).ThenInclude(ur => ur.Rol).ThenInclude(r => r.RolPermisos).ThenInclude(rp => rp.Permiso)
            .FirstOrDefaultAsync(x => x.Email.ToLower() == email.ToLower());
        return u == null ? null : MapToDto(u);
    }

    public async Task<(bool Ok, string? Error, UsuarioDto? Usuario)> CrearAsync(CrearUsuarioDto dto, int? creadoPorId = null)
    {
        if (await _context.Usuarios.AnyAsync(x => x.Email.ToLower() == dto.Email.ToLower()))
            return (false, "Ya existe un usuario con ese email", null);

        var usuario = new Usuario
        {
            Nombre = dto.Nombre,
            Email = dto.Email.ToLower().Trim(),
            PasswordHash = PasswordHasher.Hash(dto.Password),
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        if (dto.RolesIds.Any())
        {
            await AsignarRolesAsync(usuario.Id, dto.RolesIds, creadoPorId);
        }
        else
        {
            // Rol por defecto Lector (3)
            await AsignarRolesAsync(usuario.Id, new List<int> { 3 }, creadoPorId);
        }

        var creado = await ObtenerPorIdAsync(usuario.Id);
        return (true, null, creado);
    }

    public async Task<(bool Ok, string? Error)> ActualizarAsync(int id, EditarUsuarioDto dto)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.UsuarioRoles)
            .FirstOrDefaultAsync(u => u.Id == id);
        if (usuario == null) return (false, "Usuario no encontrado");

        if (await _context.Usuarios.AnyAsync(x => x.Id != id && x.Email.ToLower() == dto.Email.ToLower()))
            return (false, "Ya existe otro usuario con ese email");

        usuario.Nombre = dto.Nombre;
        usuario.Email = dto.Email.ToLower().Trim();
        usuario.Activo = dto.Activo;
        usuario.FechaActualizacion = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(dto.NuevoPassword))
        {
            usuario.PasswordHash = PasswordHasher.Hash(dto.NuevoPassword!);
        }

        await _context.SaveChangesAsync();

        if (dto.RolesIds != null)
        {
            await AsignarRolesAsync(id, dto.RolesIds);
        }

        return (true, null);
    }

    public async Task<(bool Ok, string? Error)> EliminarAsync(int id, int currentUserId)
    {
        if (id == currentUserId) return (false, "No puedes eliminarte a ti mismo");

        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario == null) return (false, "Usuario no encontrado");

        // No permitir eliminar al ultimo admin
        var esAdmin = await _context.UsuarioRoles.AnyAsync(ur => ur.UsuarioId == id && ur.Rol.Nombre == "Administrador");
        // Need include: but we used navigation via query on UsuarioRoles joining Roles
        // Alternative count admins
        if (esAdmin)
        {
            var totalAdmins = await _context.UsuarioRoles
                .Include(ur => ur.Rol)
                .CountAsync(ur => ur.Rol.Nombre == "Administrador");
            // totalAdmins es conteo de filas usuario_roles con admin; si solo 1 fila y es este usuario, no dejar
            var adminUsers = await _context.UsuarioRoles
                .Include(ur => ur.Rol)
                .Where(ur => ur.Rol.Nombre == "Administrador")
                .Select(ur => ur.UsuarioId)
                .Distinct()
                .CountAsync();
            if (adminUsers <= 1)
                return (false, "No se puede eliminar al ultimo administrador");
        }

        _context.Usuarios.Remove(usuario);
        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<List<RolDto>?> ObtenerRolesAsync()
    {
        var roles = await _context.Roles
            .Include(r => r.RolPermisos).ThenInclude(rp => rp.Permiso)
            .OrderBy(r => r.Id)
            .ToListAsync();
        return roles.Select(r => new RolDto
        {
            Id = r.Id,
            Nombre = r.Nombre,
            Descripcion = r.Descripcion,
            Permisos = r.RolPermisos.Select(rp => rp.Permiso.Clave).ToList()
        }).ToList();
    }

    public async Task<bool> AsignarRolesAsync(int usuarioId, List<int> rolesIds, int? asignadoPor = null)
    {
        var usuario = await _context.Usuarios.Include(u => u.UsuarioRoles).FirstOrDefaultAsync(u => u.Id == usuarioId);
        if (usuario == null) return false;

        // Eliminar roles actuales no incluidos
        var actuales = usuario.UsuarioRoles.ToList();
        foreach (var ur in actuales)
        {
            if (!rolesIds.Contains(ur.RolId))
                _context.UsuarioRoles.Remove(ur);
        }

        // Agregar nuevos
        foreach (var rolId in rolesIds.Distinct())
        {
            if (!actuales.Any(x => x.RolId == rolId))
            {
                // Verificar que rol existe
                if (!await _context.Roles.AnyAsync(r => r.Id == rolId)) continue;
                _context.UsuarioRoles.Add(new UsuarioRol
                {
                    UsuarioId = usuarioId,
                    RolId = rolId,
                    FechaAsignacion = DateTime.UtcNow,
                    AsignadoPor = asignadoPor
                });
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> VerificarPasswordAsync(string email, string password)
    {
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && u.Activo);
        if (usuario == null) return false;
        return PasswordHasher.Verify(password, usuario.PasswordHash);
    }

    public async Task ActualizarUltimoAccesoAsync(int userId)
    {
        var u = await _context.Usuarios.FindAsync(userId);
        if (u != null)
        {
            u.UltimoAcceso = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    private static UsuarioDto MapToDto(Usuario u)
    {
        var roles = u.UsuarioRoles.Select(ur => ur.Rol?.Nombre ?? "").Where(n => !string.IsNullOrEmpty(n)).ToList();
        var permisos = u.UsuarioRoles
            .SelectMany(ur => ur.Rol?.RolPermisos ?? Enumerable.Empty<RolPermiso>())
            .Select(rp => rp.Permiso?.Clave ?? "")
            .Where(c => !string.IsNullOrEmpty(c))
            .Distinct()
            .ToList();

        return new UsuarioDto
        {
            Id = u.Id,
            Nombre = u.Nombre,
            Email = u.Email,
            Activo = u.Activo,
            FechaCreacion = u.FechaCreacion,
            UltimoAcceso = u.UltimoAcceso,
            Roles = roles,
            Permisos = permisos
        };
    }
}
