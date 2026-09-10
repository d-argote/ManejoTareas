using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ManejoTareas.Attributes;
using ManejoTareas.DTOs;
using ManejoTareas.Helpers;
using ManejoTareas.Services;

namespace ManejoTareas.Controllers;

[Authorize]
[Route("usuarios")]
public class UsuariosController : Controller
{
    private readonly IUsuarioService _usuarios;

    public UsuariosController(IUsuarioService usuarios) => _usuarios = usuarios;

    // GET /usuarios  (también responde a /Usuarios por case-insensitive)
    // Alias: /Usuarios/Index
    [HttpGet("")]
    [HttpGet("~/Usuarios/Index")]
    [RequierePermiso(Permisos.UsuariosVer)]
    public async Task<IActionResult> Index()
    {
        var lista = await _usuarios.ObtenerTodosAsync();
        return View(lista);
    }

    // GET /usuarios/5  o /usuarios/detalle/5
    // Alias: /Usuarios/Details/5
    [HttpGet("{id:int}")]
    [HttpGet("detalle/{id:int}")]
    [HttpGet("~/Usuarios/Details/{id:int}")]
    [RequierePermiso(Permisos.UsuariosVer)]
    public async Task<IActionResult> Details(int id)
    {
        var u = await _usuarios.ObtenerPorIdAsync(id);
        if (u == null) return NotFound();
        return View(u);
    }

    // GET /usuarios/crear
    // Alias: /Usuarios/Create
    [HttpGet("crear")]
    [HttpGet("~/Usuarios/Create")]
    [RequierePermiso(Permisos.UsuariosCrear)]
    public async Task<IActionResult> Create()
    {
        ViewBag.Roles = await _usuarios.ObtenerRolesAsync();
        return View(new CrearUsuarioDto());
    }

    [HttpPost("crear")]
    [HttpPost("~/Usuarios/Create")]
    [ValidateAntiForgeryToken]
    [RequierePermiso(Permisos.UsuariosCrear)]
    public async Task<IActionResult> Create(CrearUsuarioDto dto)
    {
        ViewBag.Roles = await _usuarios.ObtenerRolesAsync();
        if (!ModelState.IsValid) return View(dto);

        var currentId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var cid) ? cid : (int?)null;
        var (ok, error, _) = await _usuarios.CrearAsync(dto, currentId);
        if (!ok)
        {
            ModelState.AddModelError(string.Empty, error ?? "Error al crear");
            return View(dto);
        }
        TempData["Success"] = "Usuario creado correctamente";
        return RedirectToAction(nameof(Index));
    }

    // GET /usuarios/5/editar
    // Alias: /Usuarios/Edit/5
    [HttpGet("{id:int}/editar")]
    [HttpGet("editar/{id:int}")]
    [HttpGet("~/Usuarios/Edit/{id:int}")]
    [RequierePermiso(Permisos.UsuariosEditar)]
    public async Task<IActionResult> Edit(int id)
    {
        var u = await _usuarios.ObtenerPorIdAsync(id);
        if (u == null) return NotFound();

        ViewBag.Roles = await _usuarios.ObtenerRolesAsync();
        var dto = new EditarUsuarioDto
        {
            Nombre = u.Nombre,
            Email = u.Email,
            Activo = u.Activo,
            RolesIds = await ObtenerRoleIdsDelUsuario(id)
        };
        ViewBag.UsuarioId = id;
        return View(dto);
    }

    [HttpPost("{id:int}/editar")]
    [HttpPost("editar/{id:int}")]
    [HttpPost("~/Usuarios/Edit/{id:int}")]
    [ValidateAntiForgeryToken]
    [RequierePermiso(Permisos.UsuariosEditar)]
    public async Task<IActionResult> Edit(int id, EditarUsuarioDto dto)
    {
        ViewBag.Roles = await _usuarios.ObtenerRolesAsync();
        ViewBag.UsuarioId = id;
        if (!ModelState.IsValid) return View(dto);

        var (ok, error) = await _usuarios.ActualizarAsync(id, dto);
        if (!ok)
        {
            ModelState.AddModelError(string.Empty, error ?? "Error al actualizar");
            return View(dto);
        }
        TempData["Success"] = "Usuario actualizado";
        return RedirectToAction(nameof(Index));
    }

    // POST /usuarios/5/eliminar
    // Alias: /Usuarios/Delete/5
    [HttpPost("{id:int}/eliminar")]
    [HttpPost("eliminar/{id:int}")]
    [HttpPost("~/Usuarios/Delete/{id:int}")]
    [ValidateAntiForgeryToken]
    [RequierePermiso(Permisos.UsuariosEliminar)]
    public async Task<IActionResult> Delete(int id)
    {
        var currentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var (ok, error) = await _usuarios.EliminarAsync(id, currentId);
        if (!ok)
        {
            TempData["Error"] = error;
        }
        else
        {
            TempData["Success"] = "Usuario eliminado";
        }
        return RedirectToAction(nameof(Index));
    }

    // Gestion de roles/permisos
    // GET /usuarios/5/permisos
    // Alias: /Usuarios/GestionarPermisos/5
    [HttpGet("{id:int}/permisos")]
    [HttpGet("permisos/{id:int}")]
    [HttpGet("~/Usuarios/GestionarPermisos/{id:int}")]
    [RequierePermiso(Permisos.UsuariosGestionarPermisos)]
    public async Task<IActionResult> GestionarPermisos(int id)
    {
        var u = await _usuarios.ObtenerPorIdAsync(id);
        if (u == null) return NotFound();
        ViewBag.Usuario = u;
        ViewBag.Roles = await _usuarios.ObtenerRolesAsync();
        var rolesIds = await ObtenerRoleIdsDelUsuario(id);
        return View(new AsignarRolesDto { UsuarioId = id, RolesIds = rolesIds });
    }

    [HttpPost("{id:int}/permisos")]
    [HttpPost("permisos/{id:int}")]
    [HttpPost("~/Usuarios/GestionarPermisos")]
    [HttpPost("~/Usuarios/GestionarPermisos/{id:int}")]
    [ValidateAntiForgeryToken]
    [RequierePermiso(Permisos.UsuariosGestionarPermisos)]
    public async Task<IActionResult> GestionarPermisos(AsignarRolesDto dto)
    {
        var currentId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var cid) ? cid : (int?)null;
        var ok = await _usuarios.AsignarRolesAsync(dto.UsuarioId, dto.RolesIds, currentId);
        if (!ok)
        {
            TempData["Error"] = "No se pudieron asignar roles";
        }
        else
        {
            TempData["Success"] = "Permisos actualizados";
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<int>> ObtenerRoleIdsDelUsuario(int id)
    {
        var db = HttpContext.RequestServices.GetRequiredService<Data.AppDbContext>();
        var ids = await db.UsuarioRoles.Where(ur => ur.UsuarioId == id).Select(ur => ur.RolId).ToListAsync();
        return ids;
    }
}
