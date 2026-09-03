using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ManejoTareas.Helpers;

namespace ManejoTareas.Attributes;

/// <summary>
/// Atributo para proteger acciones por permiso. Verifica que el usuario tenga el permiso indicado
/// o sea Administrador. Si no esta autenticado -> 401 Challenge, si no tiene permiso -> 403 Forbidden.
/// </summary>
public class RequierePermisoAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly string _permiso;
    private readonly bool _permitirAdminSiempre;

    public RequierePermisoAttribute(string permiso, bool permitirAdminSiempre = true)
    {
        _permiso = permiso;
        _permitirAdminSiempre = permitirAdminSiempre;
    }

    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (user.Identity?.IsAuthenticated != true)
        {
            context.Result = new ChallengeResult();
            return Task.CompletedTask;
        }

        // Admin siempre pasa si esta permitido
        if (_permitirAdminSiempre && (user.IsInRole("Administrador") || user.HasClaim("permiso", Permisos.UsuariosGestionarPermisos)))
        {
            // Si pide permiso admin, solo admin; si pide cualquier otro, admin tambien pasa
            if (_permiso == Permisos.UsuariosGestionarPermisos || user.IsInRole("Administrador"))
                return Task.CompletedTask;
            // Admin con claim gestiona permisos pasa para todo
            if (user.HasClaim("permiso", Permisos.UsuariosGestionarPermisos))
                return Task.CompletedTask;
        }

        if (user.HasClaim("permiso", _permiso) || user.IsInRole("Administrador"))
            return Task.CompletedTask;

        // Verificar todos los claims de tipo permiso (p.ej. tareas.ver)
        var tiene = user.Claims.Any(c => c.Type == "permiso" && c.Value == _permiso);
        if (tiene)
            return Task.CompletedTask;

        context.Result = new ForbidResult();
        return Task.CompletedTask;
    }
}
