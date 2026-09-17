using ManejoTareas.DTOs;
using ManejoTareas.Models;

namespace ManejoTareas.ViewModels;

public class TaskListViewModel
{
    public string UserName { get; set; } = "";
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public List<TareaDto> Tasks { get; set; } = new();
    // Alias para compatibilidad con posible uso de TaskItem/Tarea directamente
    public List<Tarea> TaskItems { get; set; } = new();
}
