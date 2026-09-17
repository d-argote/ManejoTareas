using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ManejoTareas.Data;
using ManejoTareas.Models;
using ManejoTareas.ViewModels;
using ManejoTareas.DTOs;

namespace ManejoTareas.Controllers;

[Authorize]
[Route("tareas")]
public class TasksController : Controller
{
    private readonly AppDbContext _context;

    public TasksController(AppDbContext context)
    {
        _context = context;
    }

    // GET /tareas
    [HttpGet("")]
    [HttpGet("~/Tasks")]
    [HttpGet("~/Tasks/Index")]
    public async Task<IActionResult> Index()
    {
        var tareas = await _context.Tareas
            .OrderByDescending(t => t.FechaCreacion)
            .ToListAsync();

        // Construir DTOs para la Partial View
        var dtos = tareas.Select(t => new TareaDto
        {
            Id = t.Id,
            Titulo = t.Titulo,
            Descripcion = t.Descripcion,
            Completada = t.Completada,
            FechaCreacion = t.FechaCreacion,
            FechaActualizacion = t.FechaActualizacion
        }).ToList();

        var vm = new TaskListViewModel
        {
            UserName = User.Identity?.Name ?? "Usuario",
            TotalTasks = tareas.Count,
            CompletedTasks = tareas.Count(t => t.Completada),
            Tasks = dtos
        };

        return View(vm);
    }

    // GET /tareas/crear
    [HttpGet("crear")]
    [HttpGet("~/Tasks/Create")]
    public IActionResult Create()
    {
        return View(new Tarea());
    }

    // POST /tareas/crear - versión del parcial con TaskItem
    [HttpPost("crear")]
    [HttpPost("~/Tasks/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TaskItem task)
    {
        if (!ModelState.IsValid) return View(task);

        // Asignar usuario actual si existe
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out var uid))
            task.UsuarioId = uid;

        task.FechaCreacion = DateTime.UtcNow;
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // Sobrecarga para compatibilidad con Tarea directa (el form también puede postear Tarea)
    [HttpPost("crear-tarea")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTarea(Tarea tarea)
    {
        var taskItem = new TaskItem
        {
            Titulo = tarea.Titulo,
            Descripcion = tarea.Descripcion,
            Completada = tarea.Completada,
            FechaCreacion = tarea.FechaCreacion,
            UsuarioId = tarea.UsuarioId
        };
        return await Create(taskItem);
    }

    // GET /tareas/detalle/{id}
    [HttpGet("detalle/{id:int}")]
    [HttpGet("~/Tasks/Details/{id:int}")]
    [HttpGet("~/Tasks/Detalle/{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var task = await _context.Tareas.FindAsync(id);
        if (task == null) return NotFound();
        return View(task);
    }

    // Alias para compatibilidad con /tareas/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> DetailsById(int id)
    {
        return await Details(id);
    }

    // GET /tareas/completar/{id} - RETO ADICIONAL
    [HttpGet("completar/{id:int}")]
    [HttpGet("~/Tasks/Complete/{id:int}")]
    [HttpGet("~/Tasks/Completar/{id:int}")]
    public async Task<IActionResult> Complete(int id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null)
            return NotFound();
        task.Completada = true;
        task.FechaActualizacion = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // POST toggle para compatibilidad con UI actual
    [HttpPost("toggle/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleCompletada(int id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null) return NotFound();
        task.Completada = !task.Completada;
        task.FechaActualizacion = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // Delete: solo Admin - GET según parcial
    [Authorize(Roles = "Admin")]
    [HttpGet("eliminar/{id:int}")]
    [HttpGet("~/Tasks/Delete/{id:int}")]
    [HttpGet("~/Tasks/Eliminar/{id:int}")]
    public async Task<IActionResult> DeleteGet(int id)
    {
        // Verificación adicional para Administrador alias
        if (!User.IsInRole("Admin") && !User.IsInRole("Administrador"))
            return Forbid();

        var task = await _context.Tasks.FindAsync(id);
        if (task == null) return NotFound();
        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Tarea eliminada";
        return RedirectToAction(nameof(Index));
    }

    // POST eliminar - también restringido a Admin
    [Authorize(Roles = "Admin,Administrador")]
    [HttpPost("eliminar/{id:int}")]
    [HttpPost("~/Tasks/Delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        if (!User.IsInRole("Admin") && !User.IsInRole("Administrador"))
        {
            TempData["Error"] = "No tienes permiso para eliminar";
            return RedirectToAction(nameof(Index));
        }
        var task = await _context.Tasks.FindAsync(id);
        if (task == null) return NotFound();
        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // Alias POST sin anti forgery para pruebas? No, mantenemos validación
}
