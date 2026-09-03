using Microsoft.AspNetCore.Mvc;
using ManejoTareas.DTOs;
using ManejoTareas.Services;

namespace ManejoTareas.Controllers;

public class TaskController : Controller
{
    private readonly ITareaService _tareaService;

    public TaskController(ITareaService tareaService)
    {
        _tareaService = tareaService;
    }

    public async Task<IActionResult> Index()
    {
        var tareas = await _tareaService.ObtenerTodasAsync();
        return View(tareas);
    }

    public async Task<IActionResult> Details(int id)
    {
        var tarea = await _tareaService.ObtenerPorIdAsync(id);
        if (tarea == null) return NotFound();
        return View(tarea);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CrearTareaDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        await _tareaService.CrearAsync(dto);
        return RedirectToAction(nameof(Index));
    }

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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _tareaService.EliminarAsync(id);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleCompletada(int id)
    {
        await _tareaService.ToggleCompletadaAsync(id);
        return RedirectToAction(nameof(Index));
    }
}