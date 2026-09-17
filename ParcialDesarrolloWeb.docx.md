**Desarrollo web Utap**

*ASP.NET CORE MVC \+ IDENTITY*

1\.  PROPÓSITO Integrar MVC, Controllers, Actions, Routing, Razor, formularios GET/POST, Model Binding, ViewModels, Partial Views, Layout, Entity Framework Core, base de datos, Identity, autenticación, autorización y roles.

2\.  SITUACIÓN PROBLEMA Una empresa necesita una aplicación web para administrar tareas.

Los usuarios autenticados deben poder: \- Consultar tareas. – Registrar nuevas tareas. \- Consultar el detalle de una tarea. \- Marcar tareas como completadas. Solamente los usuarios con el rol “Admin” podrán eliminar tareas.

3\.  CONDICIONES Duración: 90 minutos. Puede utilizar:

\-   Visual Studio Code.

\-   Terminal integrada.

\-   Documentación oficial.

\-   Apuntes de clase.

\-   Proyecto base ManejoTareas.

No se permite copiar una solución de otro estudiante ni recibir código de otra persona.

4\.  CONTROLLER Y ROUTING  Crear/completar TasksController con: GET /tareas GET /tareas/crear POST /tareas/crear GET /tareas/detalle/{id}

Usar Attribute Routing.

Ejemplo:

\[Route(“tareas”)\] public class TasksController : Controller {

\[HttpGet(““)\] public IActionResult Index() { return View(); }

    \[HttpGet("crear")\]

    public IActionResult Create()

    {

        return View();

    }

    \[HttpGet("detalle/{id:int}")\]

    public IActionResult Details(int id)

    {

        return View();

    }

}

5\.  FORMULARIO RAZOR Crear Views/Tasks/Create.cshtml.

Debe permitir registrar título y descripción y utilizar: \- asp-for \- asp-validation-for \- asp-action \- method=“post”

El POST debe recibir TaskItem mediante Model Binding:

\[HttpPost(“crear”)\] \[ValidateAntiForgeryToken\]

public async Task Create(TaskItem task) { 

if (\!ModelState.IsValid) return View(task);

    \_context.Tasks.Add(task);

    await \_context.SaveChangesAsync();

    return RedirectToAction(nameof(Index));

}

6\. VIEWMODEL Crear/utilizar TaskListViewModel.

Ejemplo:

public class TaskListViewModel { 

public string UserName { get; set; } \= ““; 

public int TotalTasks { get; set; } 

public int CompletedTasks { get;set; } 

public List Tasks { get; set; } \= new(); 

}

Index.cshtml debe utilizar: @model ManejoTareas.ViewModels.TaskListViewModel

Debe mostrar usuario, total, completadas y lista de tareas.

7 PARTIAL VIEW Crear Views/Shared/\_TaskCard.cshtml.

Debe mostrar título, descripción y estado.

Desde Index:

@foreach (var task in Model.Tasks) { }

8\. IDENTITY Y AUTENTICACIÓN Proteger TasksController con:

\[Authorize\]

Demostrar: \- Usuario no autenticado: no puede acceder. – Usuario autenticado: puede acceder.

9\. AUTORIZACIÓN POR ROLES Delete debe estar disponible únicamente para Admin.

Ejemplo:

\[Authorize(Roles \= “Admin”)\] \[HttpGet(“eliminar/{id:int}”)\] 

El botón puede ocultarse para usuarios normales:

@if (User.IsInRole(“Admin”)) { Eliminar }

10\. ¿Qué función cumple el Controller?
RR: Basicamente es la parte logica del programa

11\. ¿Qué función cumple una View?


12\. ¿Qué hace \[Authorize(Roles \= “Admin”)\]?


13\. ¿Qué diferencia existe entre GET y POST?
RR: Uno optiene informacion o la recopila que es el get y el POST la publica 

14\. ¿Qué es Model Binding?


15\. ¿Para qué sirve una Partial View?

16\. ¿Qué función cumple \_ViewStart.cshtml?

17\. ¿Quién genera las tablas de Identity?

RETO ADICIONAL Si termina antes, implementar una acción para marcar una tarea como completada:

\[HttpGet(“completar/{id:int}”)\] public async Task Complete(int id) { var

task \= await \_context.Tasks.FindAsync(id);

    if (task \== null)

        return NotFound();

    task.Completed \= true;

    await \_context.SaveChangesAsync();

    return RedirectToAction(nameof(Index));

}

Al finalizar, comprobar: 

\[ \] Aplicación compila correctamente. 

\[ \] Base de datos actualizada. 

\[ \] Lista de tareas funcionando. 

\[ \] Formulario funcionando. 

\[ \] ViewModel funcionando.

\[ \] Partial View funcionando. 

\[ \] Identity funcionando. 

\[ \] \[Authorize\] funcionando. 

\[ \] Rol Admin funcionando. 

\[ \] Delete restringido a Admin.

Comandos:

dotnet build 

dotnet run


