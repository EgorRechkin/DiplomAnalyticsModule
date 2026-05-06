using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AnalyticsModuleDiplomMVC.Models;
using AnalyticsModuleDiplomMVC.Services;

namespace AnalyticsModuleDiplomMVC.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    private readonly DataService _dataService;
 
    public HomeController(ILogger<HomeController> logger, DataService dataService)
    {
        _logger = logger;
        _dataService = dataService;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}