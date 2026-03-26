using Microsoft.AspNetCore.Mvc;
<<<<<<< HEAD

namespace QazaqQuest.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
=======
using QazaqQuest.Services;

namespace QazaqQuest.Controllers;

public class HomeController : Controller
{
    private readonly AppDataService _dataService;

    public HomeController(AppDataService dataService)
    {
        _dataService = dataService;
    }

    public IActionResult Index()
    {
        var allQuests = _dataService.GetQuests();

        ViewBag.QuestCount = allQuests.Count;
        ViewBag.CityCount = allQuests.Select(q => q.City).Distinct().Count();
        ViewBag.RoutePointCount = _dataService.GetTotalRoutePoints();
        ViewBag.UserName = HttpContext.Session.GetString("UserName") ?? "Гость";
        ViewBag.UserRole = HttpContext.Session.GetString("UserRole") ?? "Guest";
        ViewBag.FeaturedCities = allQuests.Select(q => q.City).Distinct().Take(10).ToList();

        return View(allQuests.Take(6).ToList());
    }


    public IActionResult Error()
    {
        ViewData["Title"] = "Ошибка";
        return View();
    }
}
>>>>>>> d34208a (feature 2.0)
