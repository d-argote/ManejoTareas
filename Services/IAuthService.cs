using System.Security.Claims;
using ManejoTareas.DTOs;

namespace ManejoTareas.Services;

public interface IAuthService
{
    Task<(bool Ok, string? Error, ClaimsPrincipal? Principal)> LoginAsync(string email, string password, bool recordar);
    Task<ClaimsPrincipal> CrearPrincipalAsync(int userId);
}
