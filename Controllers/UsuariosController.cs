using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft .AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ManejoTareas.Attributes;
using ManejoTareas.DTOs;
using ManejoTareas.Helpers;
using ManejoTareas.Services;

namespace ManejoTareas.Controllers;

[Authorize]
public class UsuariosController : Controller
{
    private readonly IUsuarioService _usuarios;

    public UsuariosController(IUsuarioService usuarios) => _usuarios = usuarios;

    // GET /Usuarios -> requiere ver usuarios o ser admin
    [RequierePermiso(Permisos.UsuariosVer)]
    public async Task<IActionResult> Index()
    {
        var lista = await _usuarios.ObtenerTodosAsync();
        return View(lista);
    }

    [RequierePermiso(Permisos.UsuariosVer)]
    public async Task<IActionResult> Details(int id)
    {
        var u = await _usuarios.ObtenerPorIdAsync(id);
        if (u == null) return NotFound();
        return View(u);
    }

    [RequierePermiso(Permisos.UsuariosCrear)]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Roles = await _usuarios.ObtenerRolesAsync();
        return View(new CrearUsuarioDto());
    }

    [RequierePermiso(Permisos.UsuariosCrear)]
    [HttpPost]
    [ValidateAntiForgeryToken]
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

    [RequierePermiso(Permisos.UsuariosEditar)]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var u = await _usuarios.ObtenerPorIdAsync(id);
        if (u == null) return NotFound();

        ViewBag.Roles = await _usuarios.ObtenerRolesAsync();
        // Obtener rolesIds actuales via service (necesitamos consultar entidad completa)
        // Usaremos ObtenerPorId y mapear a editar DTO
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

    [RequierePermiso(Permisos.UsuariosEditar)]
    [HttpPost]
    [ValidateAntiForgeryToken]
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

    [RequierePermiso(Permisos.UsuariosEliminar)]
    [HttpPost]
    [ValidateAntiForgeryToken]
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
    [RequierePermiso(Permisos.UsuariosGestionarPermisos)]
    [HttpGet]
    public async Task<IActionResult> GestionarPermisos(int id)
    {
        var u = await _usuarios.ObtenerPorIdAsync(id);
        if (u == null) return NotFound();
        ViewBag.Usuario = u;
        ViewBag.Roles = await _usuarios.ObtenerRolesAsync();
        var rolesIds = await ObtenerRoleIdsDelUsuario(id);
        return View(new AsignarRolesDto { UsuarioId = id, RolesIds = rolesIds });
    }

    [RequierePermiso(Permisos.UsuariosGestionarPermisos)]
    [HttpPost]
    [ValidateAntiForgeryToken]
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
        // Consulta directa al contexto via servicio: obtenemos todos los roles y filtramos por usuario en View
        // Para simplificar, consultamos via reflection a DB? Pero tenemos el DTO con nombres no ids.
        // Hacemos consulta directa rapido usando un hack: leer desde service interno no expone ids, asi que consultamos de nuevo con contexto
        // En lugar de inyectar contexto aqui, usaremos el servicio ObtenerTodos y buscaremos, pero mejor consultar DB directamente
        // Por ahora usaremos un truco: buscar en _usuarios.ObtenerRoles y luego mapear con nombres
        // Necesitamos los ids, asi que agregamos metodo privado via HttpContext.RequestServices
        var db = HttpContext.RequestServices.GetRequiredService<Data.AppDbContext>();
        var ids = await db.UsuarioRoles.Where(ur => ur.UsuarioId == id).Select(ur => ur.RolId).ToListAsync();
        return ids;
    }
}
