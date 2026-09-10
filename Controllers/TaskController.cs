using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ManejoTareas.DTOs;
using ManejoTareas.Services;
using ManejoTareas.Attributes;
using ManejoTareas.Helpers;

namespace ManejoTareas.Controllers;

[Authorize]
[Route("tareas")]
public class TaskController : Controller
{
    private readonly ITareaService _tareaService;

    public TaskController(ITareaService tareaService)
    {
        _tareaService = tareaService;
    }

    // GET /tareas
    // Alias: /Task , /Task/Index
    [HttpGet("")]
    [HttpGet("~/Task")]
    [HttpGet("~/Task/Index")]
    [RequierePermiso(Permisos.TareasVer)]
    public async Task<IActionResult> Index()
    {
        var tareas = await _tareaService.ObtenerTodasAsync();
        return View(tareas);
    }

    // GET /tareas/5
    // GET /tareas/detalle/5
    // Alias: /Task/Details/5
    [HttpGet("{id:int}")]
    [HttpGet("detalle/{id:int}")]
    [HttpGet("~/Task/Details/{id:int}")]
    [RequierePermiso(Permisos.TareasVer)]
    public async Task<IActionResult> Details(int id)
    {
        var tarea = await _tareaService.ObtenerPorIdAsync(id);
        if (tarea == null) return NotFound();
        return View(tarea);
    }

    // GET /tareas/crear
    // Alias: /Task/Create
    [HttpGet("crear")]
    [HttpGet("~/Task/Create")]
    [RequierePermiso(Permisos.TareasCrear)]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost("crear")]
    [HttpPost("~/Task/Create")]
    [ValidateAntiForgeryToken]
    [RequierePermiso(Permisos.TareasCrear)]
    public async Task<IActionResult> Create(CrearTareaDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        await _tareaService.CrearAsync(dto);
        return RedirectToAction(nameof(Index));
    }

    // GET /tareas/5/editar  y /tareas/editar/5
    // Alias: /Task/Edit/5
    [HttpGet("{id:int}/editar")]
    [HttpGet("editar/{id:int}")]
    [HttpGet("~/Task/Edit/{id:int}")]
    [RequierePermiso(Permisos.TareasEditar)]
    public async Task<IActionResult> Edit(int id)
    {
        var tarea = await _tareaService.ObtenerPorIdAsync(id);
        if (tarea == null) return NotFound();

        var dto = new ActualizarTareaDto
        {
            Titulo = tarea.Titulo,
            Descripcion = tarea.Descripcion,
            Completada = tarea.Completada
        };

        ViewBag.TareaId = id;
        return View(dto);
    }

    [HttpPost("{id:int}/editar")]
    [HttpPost("editar/{id:int}")]
    [HttpPost("~/Task/Edit/{id:int}")]
    [ValidateAntiForgeryToken]
    [RequierePermiso(Permisos.TareasEditar)]
    public async Task<IActionResult> Edit(int id, ActualizarTareaDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.TareaId = id;
            return View(dto);
        }

        var result = await _tareaService.ActualizarAsync(id, dto);
        if (result == null) return NotFound();

        return RedirectToAction(nameof(Index));
    }

    // POST /tareas/5/eliminar
    // Alias: /Task/Delete/5
    [HttpPost("{id:int}/eliminar")]
    [HttpPost("eliminar/{id:int}")]
    [HttpPost("~/Task/Delete/{id:int}")]
    [ValidateAntiForgeryToken]
    [RequierePermiso(Permisos.TareasEliminar)]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _tareaService.EliminarAsync(id);
        if (!ok) TempData["Error"] = "No tienes permiso para eliminar o la tarea no existe";
        return RedirectToAction(nameof(Index));
    }

    // POST /tareas/5/toggle
    // Alias: /Task/ToggleCompletada/5
    [HttpPost("{id:int}/toggle")]
    [HttpPost("toggle/{id:int}")]
    [HttpPost("~/Task/ToggleCompletada/{id:int}")]
    [ValidateAntiForgeryToken]
    [RequierePermiso(Permisos.TareasCompletar)]
    public async Task<IActionResult> ToggleCompletada(int id)
    {
        await _tareaService.ToggleCompletadaAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
