# Respuestas Parcial - Desarrollo Web UTAP (ManejoTareas)
> Explicaciones paso a paso para principiantes - Proyecto `ManejoTareas` ASP.NET Core MVC + EF Core + Cookies

---

### 10. ¿Qué función cumple el Controller? (La "C" de MVC)
Imagina el Controller como el **mesero** del restaurante. Tú (navegador) haces un pedido (URL), el mesero lo lleva a la cocina (lógica/BD) y te trae el plato (HTML).

**Flujo exacto en ManejoTareas:**
1. Escribes `GET /tareas` en el navegador.
2. `Program.cs:113` `MapControllers()` busca qué Controller tiene `[Route("tareas")]` -> `Controllers/TasksController.cs:13` `public class TasksController`.
3. Busca la Action con `[HttpGet("")]` -> `Index()` `Controllers/TasksController.cs:22`.
4. La Action verifica `[Authorize]` (¿estás logueado?), consulta BD `await _context.Tareas.ToListAsync()`, arma el `TaskListViewModel` con `UserName`/`TotalTasks` y hace `return View(vm)`.
5. MVC busca `Views/Tasks/Index.cshtml` y la renderiza.

**REGLA:** El Controller NO tiene HTML, solo **decide** qué hacer. La lógica pesada va en `Services/TareaService.cs`, la vista solo muestra.

---

### 11. ¿Qué función cumple una View? (La "V" de MVC)
Es la **plantilla HTML + Razor** que ve el usuario. Recibe un `@model` del Controller y lo dibuja. No debe consultar BD.

**Partes:**
- `Views/Tasks/Index.cshtml:1` `@model ManejoTareas.ViewModels.TaskListViewModel` -> le dice "voy a recibir este objeto".
- Usa `@Model.UserName`, `@foreach(var task in Model.Tasks)` para pintar.
- Usa Tag Helpers: `asp-for`, `asp-action` que generan `name`/`href` automáticamente.
- Usa `Layout = "_Layout"` (`Views/_ViewStart.cshtml:2`) para no repetir navbar/footer.

**Ejemplo diferencia:**
```csharp
// Controller (C#) -> decide
var vm = new TaskListViewModel{ UserName="Ana", TotalTasks=5};
return View(vm);

// View (Razor) -> dibuja
<h1>Hola @Model.UserName</h1> // -> <h1>Hola Ana</h1>
```

---

### 12. ¿Qué hace `[Authorize(Roles = "Admin")]`? (Autorización)
Es un **guardia en la puerta** de la Action.

**Dos pasos separados:**
1. **Autenticación** `[Authorize]` -> ¿tienes pulsera (cookie `.AspNetCore.Cookies`)? Si no -> 401 Challenge -> redirige a `Program.cs:27` `LoginPath="/auth/login"`.
2. **Autorización** `Roles="Admin"` -> ¿tu pulsera dice `Role=Admin`? Si no -> 403 Forbidden -> `Program.cs:29` `AccessDeniedPath="/auth/acceso-denegado"`.

**Cómo se crea la pulsera:**
En `Services/AuthService.cs:40` al hacer login se crea:
```csharp
new Claim(ClaimTypes.Role, "Administrador");
new Claim(ClaimTypes.Role, "Admin"); // alias para que ambos valgan
```
Entonces `User.IsInRole("Admin")` es true.

**En el proyecto:**
- `Controllers/TasksController.cs:11` `[Authorize]` en la clase protege TODO `/tareas`.
- `Controllers/TasksController.cs:144` `[Authorize(Roles="Admin")]` solo en `DeleteGet("eliminar/{id}")`. En la vista `Views/Shared/_TaskCard.cshtml:13` se oculta el botón: `@if(User.IsInRole("Admin")){ <button>Eliminar</button> }`.

---

### 13. ¿Qué diferencia existe entre GET y POST? 
| Característica | GET | POST |
|---|---|---|
| **Para qué** | **Leer/Pedir** datos (seguro) | **Enviar/Crear/Modificar** datos |
| **Idempotente** | Sí | No |
| **Dónde van datos** | URL: `/tareas/detalle/1` | Body: `Titulo=Tarea1&...` |
| **Cache** | Sí | No |
| **Cambia BD** | Nunca | Sí |
| **Ejemplo** | `GET /tareas`, `GET /tareas/crear` | `POST /tareas/crear`, `POST /auth/login` |
| **Validación** | No lleva `ValidateAntiForgeryToken` | Sí lleva `ValidateAntiForgeryToken` |

**Anécdota:** Si usas `GET` para borrar (`<a href="/tareas/eliminar/1">`), un bot borraría todo. Por eso borrar debe ser `POST`.

---

### 14. ¿Qué es Model Binding? 
Es el sistema que **traduce texto HTTP a objetos C#** sin que escribas `Request.Form["Titulo"]` a mano.

**Paso a paso con `POST /tareas/crear`:**
1. View tiene: `Views/Tasks/Create.cshtml:16` `<input asp-for="Titulo" />` -> genera `<input name="Titulo" />`
2. Usuario llena "Comprar pan" -> envía `Titulo=Comprar+pan&...`
3. ASP.NET ve `public async Task<IActionResult> Create(TaskItem task)` `Controllers/TasksController.cs:66` y crea `new TaskItem{ Titulo="Comprar pan" }`.
4. Valida `[Required][MaxLength(200)]` en `Models/Tarea.cs:13`. Si falla, `ModelState.IsValid==false` y hace `return View(task)`.

**Fuentes:** `Route` (`detalle/{id}`), `Query` (`?page=1`), `Form` (`Titulo`), `Body JSON`.

---

### 15. ¿Para qué sirve una Partial View? 
Es un **pedazo de HTML** reutilizable (componente). Evita copiar/pegar.

**Ejemplo:**
```html
// Views/Shared/_TaskCard.cshtml:1
@model TareaDto
<div class="card"><h5>@Model.Titulo</h5><p>@Model.Descripcion</p></div>

// Views/Tasks/Index.cshtml:34
@foreach(var task in Model.Tasks){
  <partial name="_TaskCard" model="task" />
}
```

---

### 16. ¿Qué función cumple `_ViewStart.cshtml`? 
Se ejecuta **antes de CADA View** automáticamente. Define `Layout = "_Layout"` (`Views/_ViewStart.cshtml:2`). Evita repetir `@{Layout="_Layout";}` en cada vista.

---

### 17. ¿Quién genera las tablas de Identity? 
**Entity Framework Core Migrations** (`AppDbContext` + `db.Database.Migrate()` en `Program.cs:60`). Al ejecutar `dotnet ef migrations add` y `Migrate()` al arrancar, EF crea tablas según `OnModelCreating` (`Data/AppDbContext.cs:18`).

En este proyecto se usa **Cookie Authentication** custom (`AddAuthentication().AddCookie()` `Program.cs:24`) con tablas `usuarios`, `roles`, `permisos`, `usuario_roles` (`Migrations/20260903003952_UsuariosYRLS.cs`). El admin se crea en `Program.cs:63`.

---

### Checklist Parcial Verificado
- [x] `dotnet build` OK
- [x] `GET /tareas`, `GET /tareas/crear`, `POST /tareas/crear`, `GET /tareas/detalle/{id}` con Attribute Routing
- [x] `Views/Tasks/Create.cshtml` con `asp-for`/`asp-validation-for`/`asp-action`/`method="post"`
- [x] `TaskListViewModel` en `ViewModels/TaskListViewModel.cs:6`
- [x] `_TaskCard.cshtml` + `foreach`
- [x] `[Authorize]` en `TasksController`
- [x] `[Authorize(Roles="Admin")]` en Delete + botón `IsInRole`
- [x] `GET completar/{id}` reto `Controllers/TasksController.cs:116`
