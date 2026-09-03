using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using ManejoTareas.DTOs;
using ManejoTareas.Services;

namespace ManejoTareas.Controllers;

public class AuthController : Controller
{
    private readonly IAuthService _auth;
    private readonly IUsuarioService _usuarios;

    public AuthController(IAuthService auth, IUsuarioService usuarios)
    {
        _auth = auth;
        _usuarios = usuarios;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");
        ViewBag.ReturnUrl = returnUrl;
        return View(new LoginDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDto dto, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(dto);

        var (ok, error, principal) = await _auth.LoginAsync(dto.Email, dto.Password, dto.Recordarme);
        if (!ok || principal == null)
        {
            ModelState.AddModelError(string.Empty, error ?? "Credenciales invalidas");
            return View(dto);
        }

        var props = new AuthenticationProperties
        {
            IsPersistent = dto.Recordarme,
            ExpiresUtc = dto.Recordarme ? DateTimeOffset.UtcNow.AddDays(7) : DateTimeOffset.UtcNow.AddHours(8)
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Registro()
    {
        // Registro publico opcional: si ya hay usuarios, solo admin puede crear. Para bootstrap permitimos registro si no hay usuarios.
        return View(new RegistroDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Registro(RegistroDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        // Si ya existen usuarios, exigir que registro solo sea via admin (bloquear auto-registro)
        var todos = await _usuarios.ObtenerTodosAsync();
        if (todos.Count > 0 && !User.IsInRole("Administrador"))
        {
            ModelState.AddModelError(string.Empty, "El registro esta deshabilitado. Contacta al administrador.");
            return View(dto);
        }

        var crearDto = new CrearUsuarioDto
        {
            Nombre = dto.Nombre,
            Email = dto.Email,
            Password = dto.Password,
            ConfirmPassword = dto.ConfirmPassword,
            RolesIds = new List<int> { 3 } // Lector por defecto, admin cambiara
        };

        // Si es el primer usuario, darle admin
        if (todos.Count == 0)
            crearDto.RolesIds = new List<int> { 1 };

        var (ok, error, usuario) = await _usuarios.CrearAsync(crearDto);
        if (!ok)
        {
            ModelState.AddModelError(string.Empty, error ?? "Error al crear usuario");
            return View(dto);
        }

        // Auto-login
        var principal = await _auth.CrearPrincipalAsync(usuario!.Id);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult AccesoDenegado()
    {
        return View();
    }
}
