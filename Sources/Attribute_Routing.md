# Attribute Routing - ManejoTareas

> Implementación de **Attribute Routing** para URLs limpias y amigables en el proyecto ManejoTareas (ASP.NET Core MVC 9.0).

---

## 1. Objetivo

Reemplazar/ complementar el enrutamiento convencional (`MapControllerRoute` con patrón `{controller}/{action}/{id?}`) por **enrutamiento por atributos** (`[Route]`, `[HttpGet]`, `[HttpPost]`) que permite:

- URLs semánticas y en español (`/tareas`, `/usuarios`, `/auth/login`, `/privacidad`, `/acerca`).
- Control granular por acción (verbos HTTP explícitos).
- Compatibilidad con URLs anteriores mediante **alias** (`~/Task/...`, `~/Usuarios/...`, `~/Auth/...`).
- Mejor SEO y legibilidad.

Ver conceptos oficiales: https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/routing#attribute-routing

---

## 2. Cambios en `Program.cs`

### 2.1 Autenticación (rutas actualizadas)

```csharp
// Program.cs:24-34
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/auth/login";                // antes: /Auth/Login
        options.LogoutPath = "/auth/logout";              // antes: /Auth/Logout
        options.AccessDeniedPath = "/auth/acceso-denegado"; // antes: /Auth/AccesoDenegado
        ...
    });
```

> Las rutas de cookie deben coincidir con las nuevas rutas por atributo. El sistema es case-insensitive, pero se usa minúsculas como canónico.

### 2.2 Habilitar Attribute Routing

```csharp
// Program.cs:112-114
app.UseAuthentication();
app.UseRlsContext();
app.UseAuthorization();

// Attribute Routing + Conventional Routing (fallback)
app.MapControllers(); // Habilita Attribute Routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
```

- `AddControllersWithViews()` ya registra el servicio necesario.
- `MapControllers()` es **obligatorio** para que los atributos `[Route]`, `[HttpGet]`, `[HttpPost]` tengan efecto.
- Se mantiene `MapControllerRoute` como **fallback** para controladores no migrados o para que `/Home/Index` siga funcionando si faltara un alias.

> **Orden importa:** `MapControllers()` antes que `MapControllerRoute`.

---

## 3. HomeController - Rutas Públicas

**Archivo:** `Controllers/HomeController.cs:8`

Sin ruta a nivel de controlador, cada acción define rutas absolutas (con `/` inicial):

| Acción | Verbo | Rutas Attribute | Descripción |
|--------|-------|----------------|-------------|
| `Index` | `GET` | `/` , `/Home` , `/Home/Index` , `/inicio` | Página principal. Requiere auth, redirige a `/auth/login` si no autenticado. |
| `Privacy` | `GET` | `/Home/Privacy` , `/Home/Privacidad` , `/privacidad` | Alias español |
| `About` | `GET` | `/Home/About` , `/Home/Acerca` , `/acerca` | Alias español |
| `Error` | `GET` | `/Home/Error` , `/error` | Con `[ResponseCache]` |

**Ejemplo:**

```csharp
[HttpGet("/")]
[HttpGet("/Home")]
[HttpGet("/Home/Index")]
[HttpGet("/inicio")]
public async Task<IActionResult> Index() { ... }

[HttpGet("/Home/Privacy")]
[HttpGet("/privacidad")]
public IActionResult Privacy() { ... }
```

> Uso de `"/"` al inicio hace la ruta **absoluta** (ignora prefijo de controlador). Si se usara `"Home/Privacy"` sin `/`, también sería absoluta cuando no hay `[Route]` en el controlador, pero con `/` es más explícito.

---

## 4. AuthController - Autenticación

**Archivo:** `Controllers/AuthController.cs:10`

```csharp
[Route("auth")]
public class AuthController : Controller { ... }
```

Prefijo `auth` (minúsculas) → todas las acciones heredan `/auth/...`.

| Acción | Verbo | Rutas | URL canónica | Alias convencional |
|--------|-------|-------|--------------|--------------------|
| `Login` (GET) | `GET` | `login` , `~/Auth/Login` | `GET /auth/login` | `GET /Auth/Login` |
| `Login` (POST) | `POST` | `login` , `~/Auth/Login` | `POST /auth/login` | `POST /Auth/Login` |
| `Registro` (GET) | `GET` | `registro` , `~/Auth/Registro` | `GET /auth/registro` | `GET /Auth/Registro` |
| `Registro` (POST) | `POST` | `registro` , `~/Auth/Registro` | `POST /auth/registro` | `POST /Auth/Registro` |
| `Logout` | `POST` | `logout` , `~/Auth/Logout` | `POST /auth/logout` | `POST /Auth/Logout` |
| `AccesoDenegado` | `GET` | `acceso-denegado` , `~/Auth/AccesoDenegado` | `GET /auth/acceso-denegado` | `GET /Auth/AccesoDenegado` |

**Notas:**

- `Registro` mantiene compatibilidad con `/Auth/Registro` (Mayúscula R) aunque el canónico es `/auth/registro` (case-insensitive).
- `acceso-denegado` usa guion para SEO vs `AccesoDenegado` camelCase. Ambos soportados mediante dos atributos.
- `Logout` es solo `POST` con `ValidateAntiForgeryToken`.

**Ejemplo:**

```csharp
[HttpGet("login")]
[HttpGet("~/Auth/Login")]
public IActionResult Login(string? returnUrl = null) { ... }

[HttpGet("acceso-denegado")]
[HttpGet("~/Auth/AccesoDenegado")]
public IActionResult AccesoDenegado() { ... }
```

El `~` en `"~/Auth/Login"` indica ruta absoluta, ignorando el prefijo `auth`.

---

## 5. TaskController - Tareas

**Archivo:** `Controllers/TaskController.cs:11`

```csharp
[Authorize]
[Route("tareas")]
public class TaskController : Controller { ... }
```

Prefijo en español `tareas` → URLs amigables. Se mantienen alias `~/Task/...` para compatibilidad con vistas existentes (`asp-controller="Task"` generará ahora `/tareas` automáticamente).

| Acción | Verbo | Permiso | Rutas Attribute | URL canónica |
|--------|-------|---------|----------------|--------------|
| `Index` | `GET` | `tareas.ver` | `""` , `~/Task` , `~/Task/Index` | `GET /tareas` |
| `Details` | `GET` | `tareas.ver` | `{id:int}` , `detalle/{id:int}` , `~/Task/Details/{id:int}` | `GET /tareas/5` <br> `GET /tareas/detalle/5` |
| `Create` (GET) | `GET` | `tareas.crear` | `crear` , `~/Task/Create` | `GET /tareas/crear` |
| `Create` (POST) | `POST` | `tareas.crear` | `crear` , `~/Task/Create` | `POST /tareas/crear` |
| `Edit` (GET) | `GET` | `tareas.editar` | `{id:int}/editar` , `editar/{id:int}` , `~/Task/Edit/{id:int}` | `GET /tareas/5/editar` <br> `GET /tareas/editar/5` |
| `Edit` (POST) | `POST` | `tareas.editar` | `{id:int}/editar` , `editar/{id:int}` , `~/Task/Edit/{id:int}` | `POST /tareas/5/editar` |
| `Delete` | `POST` | `tareas.eliminar` | `{id:int}/eliminar` , `eliminar/{id:int}` , `~/Task/Delete/{id:int}` | `POST /tareas/5/eliminar` |
| `ToggleCompletada` | `POST` | `tareas.completar` | `{id:int}/toggle` , `toggle/{id:int}` , `~/Task/ToggleCompletada/{id:int}` | `POST /tareas/5/toggle` |

**Puntos clave:**

- `{id:int}` constraint asegura que solo números matcheen.
- Doble patrón para editar (`{id}/editar` y `editar/{id}`) cubre ambos estilos REST.
- Alias `~/Task/...` preserva URLs viejas tras migración.
- `RedirectToAction(nameof(Index))` ahora genera `/tareas` por el atributo en `Index`.

**Ejemplo:**

```csharp
[HttpGet("")]
[HttpGet("~/Task")]
[RequierePermiso(Permisos.TareasVer)]
public async Task<IActionResult> Index() { ... }

[HttpGet("{id:int}")]
[HttpGet("detalle/{id:int}")]
public async Task<IActionResult> Details(int id) { ... }

[HttpPost("{id:int}/eliminar")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Delete(int id) { ... }
```

---

## 6. UsuariosController - Administración de Usuarios

**Archivo:** `Controllers/UsuariosController.cs:11`

```csharp
[Authorize]
[Route("usuarios")]
public class UsuariosController : Controller { ... }
```

| Acción | Verbo | Permiso | Rutas | URL canónica |
|--------|-------|---------|-------|--------------|
| `Index` | `GET` | `usuarios.ver` | `""` , `~/Usuarios` , `~/Usuarios/Index` | `GET /usuarios` |
| `Details` | `GET` | `usuarios.ver` | `{id:int}` , `detalle/{id:int}` , `~/Usuarios/Details/{id:int}` | `GET /usuarios/5` |
| `Create` (GET) | `GET` | `usuarios.crear` | `crear` , `~/Usuarios/Create` | `GET /usuarios/crear` |
| `Create` (POST) | `POST` | `usuarios.crear` | `crear` , `~/Usuarios/Create` | `POST /usuarios/crear` |
| `Edit` (GET) | `GET` | `usuarios.editar` | `{id:int}/editar` , `editar/{id:int}` , `~/Usuarios/Edit/{id:int}` | `GET /usuarios/5/editar` |
| `Edit` (POST) | `POST` | `usuarios.editar` | `{id:int}/editar` , `editar/{id:int}` , `~/Usuarios/Edit/{id:int}` | `POST /usuarios/5/editar` |
| `Delete` | `POST` | `usuarios.eliminar` | `{id:int}/eliminar` , `eliminar/{id:int}` , `~/Usuarios/Delete/{id:int}` | `POST /usuarios/5/eliminar` |
| `GestionarPermisos` (GET) | `GET` | `usuarios.gestionar_permisos` | `{id:int}/permisos` , `permisos/{id:int}` , `~/Usuarios/GestionarPermisos/{id:int}` | `GET /usuarios/5/permisos` |
| `GestionarPermisos` (POST) | `POST` | `usuarios.gestionar_permisos` | `{id:int}/permisos` , `permisos/{id:int}` , `~/Usuarios/GestionarPermisos` , `~/Usuarios/GestionarPermisos/{id:int}` | `POST /usuarios/5/permisos` |

**Corrección:** Se eliminó el typo `using Microsoft .AspNetCore.Identity;` (espacio) → `using Microsoft.EntityFrameworkCore;` etc. El using no se usaba.

**Ejemplo:**

```csharp
[HttpGet("")]
[HttpGet("~/Usuarios")]
public async Task<IActionResult> Index() { ... }

[HttpGet("{id:int}/permisos")]
[HttpGet("~/Usuarios/GestionarPermisos/{id:int}")]
public async Task<IActionResult> GestionarPermisos(int id) { ... }
```

---

## 7. Comparativa Conventional vs Attribute

| Aspecto | Conventional (`MapControllerRoute`) | Attribute Routing (actual) |
|---------|-------------------------------------|----------------------------|
| Definición | Centralizada en `Program.cs` con patrón `{controller}/{action}/{id?}` | Descentralizada, en cada controlador/acción con `[Route]` |
| URL ejemplo Tareas | `/Task/Edit/5` | `/tareas/5/editar` (canónico) + alias `/Task/Edit/5` compatible |
| URL ejemplo Auth | `/Auth/AccesoDenegado` | `/auth/acceso-denegado` + alias `/Auth/AccesoDenegado` |
| Control | Poco granular | Por verbo HTTP (`[HttpGet]`, `[HttpPost]`), constraints (`{id:int}`) |
| Legibilidad | Inglés, camelCase | Español, kebab-case (`acceso-denegado`) |
| Mantenibilidad | Cambio global afecta todo | Cambio por controlador |

**Se mantiene `MapControllerRoute` como fallback** para que cualquier ruta no cubierta por atributos aún funcione (por ejemplo, si se añade un nuevo controlador sin atributos). Los atributos tienen precedencia sobre la ruta convencional.

---

## 8. Generación de URLs en Vistas

Las vistas **no necesitan cambios** porque usan Tag Helpers:

```html
<a asp-controller="Task" asp-action="Index">Tareas</a>
<a asp-action="Details" asp-route-id="@tarea.Id">Ver</a>
<form asp-action="Delete" asp-route-id="@tarea.Id" method="post">
```

Con Attribute Routing:

- `asp-controller="Task" asp-action="Index"` → genera `/tareas` (según `[HttpGet("")]` en `TaskController.Index`).
- `asp-action="Details" asp-route-id="5"` → `/tareas/5` (según `[HttpGet("{id:int}")]`).
- `asp-action="Edit" asp-route-id="5"` → `/tareas/5/editar`.
- `asp-controller="Auth" asp-action="Login"` → `/auth/login`.

Si se necesita URL absoluta, usar `Url.Action("Details","Task", new { id=5 })` → `/tareas/5`.

---

## 9. Buenas Prácticas Aplicadas

1.  **Prefijos coherentes:** `tareas`, `usuarios`, `auth` en minúsculas, sin mayúsculas.
2.  **Verbos HTTP explícitos:** `[HttpGet]` vs `[HttpPost]` en lugar de `[Route]` genérico.
3.  **Constraints:** `{id:int}` evita colisiones (`/tareas/crear` no se interpreta como id).
4.  **Alias con `~/`:** Mantienen compatibilidad hacia atrás (`~/Task/...`).
5.  **Rutas absolutas con `/`:** En `HomeController` para root `/`.
6.  **Kebab-case para SEO:** `acceso-denegado` en lugar de `AccesoDenegado`.
7.  **Mantener convenciones:** `MapControllers()` + `MapControllerRoute` híbrido.

---

## 10. Verificación

### 10.1 Compilación

```bash
dotnet build
# 0 Warnings, 0 Errors
```

### 10.2 Probar rutas

```bash
dotnet run --urls "http://localhost:5293"
```

| URL a probar | Esperado | Controlador |
|--------------|----------|-------------|
| `http://localhost:5293/` | 200 - Home Index (si auth → redirect `/auth/login`) | Home |
| `http://localhost:5293/inicio` | 200 - alias Home | Home |
| `http://localhost:5293/privacidad` | 200 - Privacy | Home |
| `http://localhost:5293/acerca` | 200 - About | Home |
| `http://localhost:5293/auth/login` | 200 - Login | Auth |
| `http://localhost:5293/Auth/Login` | 200 - alias (case-insensitive) | Auth |
| `http://localhost:5293/auth/registro` | 200 - Registro | Auth |
| `http://localhost:5293/auth/acceso-denegado` | 200 - Acceso denegado | Auth |
| `http://localhost:5293/tareas` | 302 → `/auth/login` si no auth, 200 si auth | Task |
| `http://localhost:5293/Task` | 302/200 - alias Task | Task |
| `http://localhost:5293/tareas/crear` | 200 - Create (requiere permiso) | Task |
| `http://localhost:5293/tareas/1` | 200/404 - Details | Task |
| `http://localhost:5293/tareas/1/editar` | 200 - Edit | Task |
| `http://localhost:5293/usuarios` | 200 - Usuarios Index | Usuarios |
| `http://localhost:5293/usuarios/1/permisos` | 200 - Gestionar permisos | Usuarios |

### 10.3 Herramienta de rutas

Agregar en desarrollo para listar rutas:

```csharp
app.MapGet("/debug/routes", (IEnumerable<EndpointDataSource> sources) =>
{
    var endpoints = sources.SelectMany(s => s.Endpoints).OfType<RouteEndpoint>();
    return Results.Json(endpoints.Select(e => new {
        RoutePattern = e.RoutePattern.RawText,
        DisplayName = e.DisplayName
    }));
});
```

---

## 11. Referencias

- ASP.NET Core Routing - Attribute Routing: https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/routing
- Routing to controller actions: https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/routing
- Constraints: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/routing#route-constraint-reference

---

## 12. Checklist de Implementación

- [x] `Program.cs:12` - `AddControllersWithViews()` (ya existía)
- [x] `Program.cs:24-34` - Actualizado `LoginPath`, `LogoutPath`, `AccessDeniedPath` a `/auth/...`
- [x] `Program.cs:112-114` - Agregado `app.MapControllers()` antes de `MapControllerRoute`
- [x] `Controllers/HomeController.cs:8` - Attribute Routing con alias español
- [x] `Controllers/AuthController.cs:10` - `[Route("auth")]` con verbos y alias `~/Auth/...`
- [x] `Controllers/TaskController.cs:11` - `[Route("tareas")]` con alias `~/Task/...`
- [x] `Controllers/UsuariosController.cs:11` - `[Route("usuarios")]` con alias `~/Usuarios/...`
- [x] Documentación `Sources/Attribute_Routing.md` creada
- [x] `dotnet build` verificado sin errores
