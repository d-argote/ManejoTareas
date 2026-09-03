using Microsoft.EntityFrameworkCore;
using ManejoTareas.Data;
using ManejoTareas.DTOs;
using ManejoTareas.Models;
using ManejoTareas.Services;

namespace ManejoTareas.Services;

public class TareaService : ITareaService
{
    private readonly AppDbContext _context;

    public TareaService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<TareaDto>> ObtenerTodasAsync()
    {
        return await _context.Tareas
            .OrderByDescending(t => t.FechaCreacion)
            .Select(t => new TareaDto
            {
                Id = t.Id,
                Titulo = t.Titulo,
                Descripcion = t.Descripcion,
                Completada = t.Completada,
                FechaCreacion = t.FechaCreacion,
                FechaActualizacion = t.FechaActualizacion
            })
            .ToListAsync();
    }

    public async Task<TareaDto?> ObtenerPorIdAsync(int id)
    {
        var tarea = await _context.Tareas.FindAsync(id);
        if (tarea == null) return null;

        return new TareaDto
        {
            Id = tarea.Id,
            Titulo = tarea.Titulo,
            Descripcion = tarea.Descripcion,
            Completada = tarea.Completada,
            FechaCreacion = tarea.FechaCreacion,
            FechaActualizacion = tarea.FechaActualizacion
        };
    }

public async Task<TareaDto> CrearAsync(CrearTareaDto dto)
        {
            var tarea = new Tarea
            {
                Titulo = dto.Titulo,
                Descripcion = dto.Descripcion,
                Completada = false,
                FechaCreacion = DateTime.UtcNow
            };

        _context.Tareas.Add(tarea);
        await _context.SaveChangesAsync();

        return new TareaDto
        {
            Id = tarea.Id,
            Titulo = tarea.Titulo,
            Descripcion = tarea.Descripcion,
            Completada = tarea.Completada,
            FechaCreacion = tarea.FechaCreacion,
            FechaActualizacion = tarea.FechaActualizacion
        };
    }

    public async Task<TareaDto?> ActualizarAsync(int id, ActualizarTareaDto dto)
    {
        var tarea = await _context.Tareas.FindAsync(id);
        if (tarea == null) return null;

        tarea.Titulo = dto.Titulo;
        tarea.Descripcion = dto.Descripcion;
        tarea.Completada = dto.Completada;
        tarea.FechaActualizacion = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new TareaDto
        {
            Id = tarea.Id,
            Titulo = tarea.Titulo,
            Descripcion = tarea.Descripcion,
            Completada = tarea.Completada,
            FechaCreacion = tarea.FechaCreacion,
            FechaActualizacion = tarea.FechaActualizacion
        };
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var tarea = await _context.Tareas.FindAsync(id);
        if (tarea == null) return false;

        _context.Tareas.Remove(tarea);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<TareaDto?> ToggleCompletadaAsync(int id)
    {
        var tarea = await _context.Tareas.FindAsync(id);
        if (tarea == null) return null;

        tarea.Completada = !tarea.Completada;
        tarea.FechaActualizacion = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new TareaDto
        {
            Id = tarea.Id,
            Titulo = tarea.Titulo,
            Descripcion = tarea.Descripcion,
            Completada = tarea.Completada,
            FechaCreacion = tarea.FechaCreacion,
            FechaActualizacion = tarea.FechaActualizacion
        };
    }
}