using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using ManejoTareas.Data;
using ManejoTareas.Helpers;

namespace ManejoTareas.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;

    public AuthService(AppDbContext context) => _context = context;

    public async Task<(bool Ok, string? Error, ClaimsPrincipal? Principal)> LoginAsync(string email, string password, bool recordar)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.UsuarioRoles).ThenInclude(ur => ur.Rol).ThenInclude(r => r.RolPermisos).ThenInclude(rp => rp.Permiso)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

        if (usuario == null)
            return (false, "Usuario no encontrado", null);
        if (!usuario.Activo)
            return (false, "Usuario desactivado. Contacta al administrador", null);
        if (!PasswordHasher.Verify(password, usuario.PasswordHash))
            return (false, "Contrasena incorrecta", null);

        usuario.UltimoAcceso = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var principal = await CrearPrincipalAsync(usuario.Id);
        return (true, null, principal);
    }

    public async Task<ClaimsPrincipal> CrearPrincipalAsync(int userId)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.UsuarioRoles).ThenInclude(ur => ur.Rol).ThenInclude(r => r.RolPermisos).ThenInclude(rp => rp.Permiso)
            .FirstAsync(u => u.Id == userId);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Nombre),
            new Claim(ClaimTypes.Email, usuario.Email),
        };

        var permisosSet = new HashSet<string>();
        foreach (var ur in usuario.UsuarioRoles)
        {
            if (ur.Rol != null)
            {
                claims.Add(new Claim(ClaimTypes.Role, ur.Rol.Nombre));
                foreach (var rp in ur.Rol.RolPermisos)
                {
                    if (rp.Permiso != null)
                        permisosSet.Add(rp.Permiso.Clave);
                }
            }
        }

        foreach (var p in permisosSet)
            claims.Add(new Claim("permiso", p));

        var identity = new ClaimsIdentity(claims, "Cookies");
        return new ClaimsPrincipal(identity);
    }
}
