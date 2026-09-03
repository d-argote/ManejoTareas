using System.Security.Claims;
using ManejoTareas.Helpers;

namespace ManejoTareas.Middleware;

public class RlsMiddleware
{
    private readonly RequestDelegate _next;

    public RlsMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        // Propaga el usuario actual al UserContext para el interceptor RLS
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var idStr = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(idStr, out var uid))
            {
                UserContext.CurrentUserId = uid;
                UserContext.IsAdmin = context.User.IsInRole("Administrador") || 
                                      context.User.HasClaim("permiso", Permisos.UsuariosGestionarPermisos);
            }
        }
        else
        {
            UserContext.Clear();
        }

        try
        {
            await _next(context);
        }
        finally
        {
            // Limpiar para no filtrar entre requests en el mismo hilo
            UserContext.Clear();
        }
    }
}

public static class RlsMiddlewareExtensions
{
    public static IApplicationBuilder UseRlsContext(this IApplicationBuilder app) => app.UseMiddleware<RlsMiddleware>();
}
