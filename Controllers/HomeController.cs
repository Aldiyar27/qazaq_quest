using Microsoft.AspNetCore.Mvc;
using QazaqQuest.Services;

namespace QazaqQuest.Controllers;

public class HomeController : Controller
{
    private readonly AppDataService _dataService;
    private readonly GameService _gameService;

    public HomeController(AppDataService dataService, GameService gameService)
    {
        _dataService = dataService;
        _gameService = gameService;
    }

    public IActionResult Index()
    {
        var currentUser = _gameService.GetCurrentUser(HttpContext);
        var allQuests = _dataService.GetQuests()
            .Where(q => !q.IsHidden || (currentUser != null && _gameService.CanUserAccessQuest(currentUser, q)))
            .ToList();

        ViewBag.QuestCount = allQuests.Count;
        ViewBag.CityCount = allQuests.Select(q => q.City).Distinct().Count();
        ViewBag.RoutePointCount = _dataService.GetTotalRoutePoints();
        ViewBag.UserName = HttpContext.Session.GetString("UserName") ?? "Гость";
        ViewBag.UserRole = HttpContext.Session.GetString("UserRole") ?? "Guest";
        ViewBag.CurrentLevel = currentUser?.Level ?? 0;
        ViewBag.FeaturedCities = allQuests.Select(q => q.City).Distinct().Take(10).ToList();

        return View(allQuests.Where(x => x.IsFeatured).Concat(allQuests.Where(x => !x.IsFeatured)).Distinct().Take(6).ToList());
    }

    public IActionResult Error()
    {
        ViewData["Title"] = "Ошибка";
        return View();
    }
}
