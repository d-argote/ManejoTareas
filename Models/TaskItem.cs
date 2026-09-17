using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManejoTareas.Models;

/// <summary>
/// Alias para compatibilidad con el enunciado del parcial.
/// El parcial pide TaskItem, el proyecto usa Tarea. TaskItem hereda de Tarea para que ambos nombres funcionen.
/// </summary>
[Table("tareas")]
public class TaskItem : Tarea
{
}

// Extensión para que _context.Tasks funcione (alias de Tareas) ver AppDbContext
