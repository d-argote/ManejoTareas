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

    public async Task<IActionResult> Index()
    {
        var tareas = await _tareaService.ObtenerTodasAsync();
        return View(tareas);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult About()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}