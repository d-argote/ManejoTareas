using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ManejoTareas.DTOs;
using ManejoTareas.Services;
using ManejoTareas.Attributes;
using ManejoTareas.Helpers;

namespace ManejoTareas.Controllers;

[Authorize]
public class TaskController : Controller
{
    private readonly ITareaService _tareaService;

    public TaskController(ITareaService tareaService)
    {
        _tareaService = tareaService;
    }

    [RequierePermiso(Permisos.TareasVer)]
    public async Task<IActionResult> Index()
    {
        var tareas = await _tareaService.ObtenerTodasAsync();
        return View(tareas);
    }

    [RequierePermiso(Permisos.TareasVer)]
    public async Task<IActionResult> Details(int id)
    {
        var tarea = await _tareaService.ObtenerPorIdAsync(id);
        if (tarea == null) return NotFound();
        return View(tarea);
    }

    [RequierePermiso(Permisos.TareasCrear)]
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [RequierePermiso(Permisos.TareasCrear)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CrearTareaDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        await _tareaService.CrearAsync(dto);
        return RedirectToAction(nameof(Index));
    }

    [RequierePermiso(Permisos.TareasEditar)]
    [HttpGet]
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

    [RequierePermiso(Permisos.TareasEditar)]
    [HttpPost]
    [ValidateAntiForgeryToken]
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

    [RequierePermiso(Permisos.TareasEliminar)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _tareaService.EliminarAsync(id);
        if (!ok) TempData["Error"] = "No tienes permiso para eliminar o la tarea no existe";
        return RedirectToAction(nameof(Index));
    }

    [RequierePermiso(Permisos.TareasCompletar)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleCompletada(int id)
    {
        await _tareaService.ToggleCompletadaAsync(id);
        return RedirectToAction(nameof(Index));
    }
}