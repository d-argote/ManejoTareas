using ManejoTareas.DTOs;

namespace ManejoTareas.Services;

public interface ITareaService
{
    Task<List<TareaDto>> ObtenerTodasAsync();
    Task<TareaDto?> ObtenerPorIdAsync(int id);
    Task<TareaDto> CrearAsync(CrearTareaDto dto);
    Task<TareaDto?> ActualizarAsync(int id, ActualizarTareaDto dto);
    Task<bool> EliminarAsync(int id);
    Task<TareaDto?> ToggleCompletadaAsync(int id);
}