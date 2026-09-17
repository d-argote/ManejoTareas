using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ManejoTareas.DTOs;
using ManejoTareas.Services;
using ManejoTareas.Attributes;
using ManejoTareas.Helpers;

namespace ManejoTareas.Controllers;

[Authorize]
[Route("Task")]
public class TaskController : Controller
{
    private readonly ITareaService _tareaService;

    public TaskController(ITareaService tareaService)
    {
        _tareaService = tareaService;
    }

    // GET /Task
    [HttpGet("")]
    [HttpGet("Index")]
    [RequierePermiso(Permisos.TareasVer)]
    public async Task<IActionResult> Index()
    {
        var tareas = await _tareaService.ObtenerTodasAsync();
        return View(tareas);
    }

    // GET /Task/Details/5
    [HttpGet("Details/{id:int}")]
    [HttpGet("{id:int}")]
    [RequierePermiso(Permisos.TareasVer)]
    public async Task<IActionResult> Details(int id)
    {
        var tarea = await _tareaService.ObtenerPorIdAsync(id);
        if (tarea == null) return NotFound();
        return View(tarea);
    }

    // GET /Task/Create
    [HttpGet("Create")]
    [HttpGet("crear")]
    [RequierePermiso(Permisos.TareasCrear)]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost("Create")]
    [HttpPost("crear")]
    [ValidateAntiForgeryToken]
    [RequierePermiso(Permisos.TareasCrear)]
    public async Task<IActionResult> Create(CrearTareaDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        await _tareaService.CrearAsync(dto);
        return RedirectToAction(nameof(Index));
    }

    // GET /Task/Edit/5
    [HttpGet("Edit/{id:int}")]
    [HttpGet("{id:int}/editar")]
    [HttpGet("editar/{id:int}")]
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

    [HttpPost("Edit/{id:int}")]
    [HttpPost("{id:int}/editar")]
    [HttpPost("editar/{id:int}")]
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

    // POST /Task/Delete/5
    [HttpPost("Delete/{id:int}")]
    [HttpPost("{id:int}/eliminar")]
    [HttpPost("eliminar/{id:int}")]
    [ValidateAntiForgeryToken]
    [RequierePermiso(Permisos.TareasEliminar)]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _tareaService.EliminarAsync(id);
        if (!ok) TempData["Error"] = "No tienes permiso para eliminar o la tarea no existe";
        return RedirectToAction(nameof(Index));
    }

    // POST /Task/ToggleCompletada/5
    [HttpPost("ToggleCompletada/{id:int}")]
    [HttpPost("{id:int}/toggle")]
    [HttpPost("toggle/{id:int}")]
    [ValidateAntiForgeryToken]
    [RequierePermiso(Permisos.TareasCompletar)]
    public async Task<IActionResult> ToggleCompletada(int id)
    {
        await _tareaService.ToggleCompletadaAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
