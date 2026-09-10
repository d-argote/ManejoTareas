using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ManejoTareas.DTOs;
using ManejoTareas.Models;
using ManejoTareas.Services;

namespace ManejoTareas.Controllers;

public class HomeController : Controller
{
    private readonly ITareaService _tareaService;

    public HomeController(ITareaService tareaService)
    {
        _tareaService = tareaService;
    }

    // Attribute Routing: ruta principal y alias
    // GET /          -> Index (root)
    // GET /Home      -> Index
    // GET /Home/Index-> Index
    // GET /inicio    -> alias amigable
    [HttpGet("/")]
    [HttpGet("/Home")]
    [HttpGet("/Home/Index")]
    [HttpGet("/inicio")]
    public async Task<IActionResult> Index()
    {
        if (User.Identity?.IsAuthenticated != true)
            return RedirectToAction("Login", "Auth");
        var tareas = await _tareaService.ObtenerTodasAsync();
        return View(tareas);
    }

    // GET /Home/Privacy
    // GET /privacidad  -> alias español
    // GET /Home/Privacidad
    [HttpGet("/Home/Privacy")]
    [HttpGet("/Home/Privacidad")]
    [HttpGet("/privacidad")]
    public IActionResult Privacy()
    {
        return View();
    }

    // GET /Home/About
    // GET /acerca
    [HttpGet("/Home/About")]
    [HttpGet("/Home/Acerca")]
    [HttpGet("/acerca")]
    public IActionResult About()
    {
        return View();
    }

    // GET /Home/Error
    // GET /error
    [HttpGet("/Home/Error")]
    [HttpGet("/error")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
