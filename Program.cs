using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using ManejoTareas.Data;
using ManejoTareas.Services;
using ManejoTareas.Middleware;
using ManejoTareas.Helpers;
using ManejoTareas.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// RLS Interceptor
builder.Services.AddSingleton<RlsConnectionInterceptor>();

builder.Services.AddDbContext<AppDbContext>((sp, options) =>
{
    var interceptor = sp.GetRequiredService<RlsConnectionInterceptor>();
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
           .AddInterceptors(interceptor);
});

// Auth
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccesoDenegado";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

builder.Services.AddAuthorization(options =>
{
    // Politicas por permiso
    foreach (var p in Permisos.Todos)
    {
        options.AddPolicy(p, policy => policy.RequireClaim("permiso", p));
    }
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Administrador"));
});

// Servicios
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITareaService, TareaService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

// Migraciones + seed admin
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();

        // Seed admin si no existe
        if (!await db.Usuarios.AnyAsync())
        {
            var adminPass = PasswordHasher.Hash("Admin123!");
            var admin = new Usuario
            {
                Nombre = "Administrador",
                Email = "admin@manejotareas.com",
                PasswordHash = adminPass,
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            };
            db.Usuarios.Add(admin);
            await db.SaveChangesAsync();
            // Asignar rol Administrador
            db.UsuarioRoles.Add(new UsuarioRol { UsuarioId = admin.Id, RolId = 1, FechaAsignacion = DateTime.UtcNow });
            await db.SaveChangesAsync();

            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Usuario admin creado: admin@manejotareas.com / Admin123!");
        }
        else if (!await db.UsuarioRoles.AnyAsync(ur => ur.RolId == 1))
        {
            // Si hay usuarios pero ninguno es admin, promueve al primero
            var first = await db.Usuarios.FirstAsync();
            db.UsuarioRoles.Add(new UsuarioRol { UsuarioId = first.Id, RolId = 1 });
            await db.SaveChangesAsync();
        }
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error al aplicar migraciones. Verifica que PostgreSQL esté corriendo en {ConnectionString}", builder.Configuration.GetConnectionString("DefaultConnection"));
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseRlsContext();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
