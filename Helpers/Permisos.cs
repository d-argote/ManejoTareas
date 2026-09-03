namespace ManejoTareas.Helpers;

public static class Permisos
{
    // Tareas
    public const string TareasVer = "tareas.ver";
    public const string TareasCrear = "tareas.crear";
    public const string TareasEditar = "tareas.editar";
    public const string TareasEliminar = "tareas.eliminar";
    public const string TareasCompletar = "tareas.completar";

    // Usuarios
    public const string UsuariosVer = "usuarios.ver";
    public const string UsuariosCrear = "usuarios.crear";
    public const string UsuariosEditar = "usuarios.editar";
    public const string UsuariosEliminar = "usuarios.eliminar";
    public const string UsuariosGestionarPermisos = "usuarios.gestionar_permisos";

    public static readonly string[] Todos = new[]
    {
        TareasVer, TareasCrear, TareasEditar, TareasEliminar, TareasCompletar,
        UsuariosVer, UsuariosCrear, UsuariosEditar, UsuariosEliminar, UsuariosGestionarPermisos
    };

    public static readonly Dictionary<string, string> Descripcion = new()
    {
        [TareasVer] = "Ver tareas",
        [TareasCrear] = "Crear tareas",
        [TareasEditar] = "Editar tareas",
        [TareasEliminar] = "Eliminar tareas",
        [TareasCompletar] = "Marcar tareas como completadas",
        [UsuariosVer] = "Ver usuarios",
        [UsuariosCrear] = "Crear usuarios",
        [UsuariosEditar] = "Editar usuarios",
        [UsuariosEliminar] = "Eliminar usuarios",
        [UsuariosGestionarPermisos] = "Gestionar permisos/roles (solo admin)"
    };

    public static readonly Dictionary<string, string> Grupos = new()
    {
        [TareasVer] = "tareas",
        [TareasCrear] = "tareas",
        [TareasEditar] = "tareas",
        [TareasEliminar] = "tareas",
        [TareasCompletar] = "tareas",
        [UsuariosVer] = "usuarios",
        [UsuariosCrear] = "usuarios",
        [UsuariosEditar] = "usuarios",
        [UsuariosEliminar] = "usuarios",
        [UsuariosGestionarPermisos] = "usuarios"
    };
}
