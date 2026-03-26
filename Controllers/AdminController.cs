using Microsoft.AspNetCore.Mvc;
using QazaqQuest.Services;

namespace QazaqQuest.Controllers;

public class AdminController : Controller
{
    private readonly AppDataService _dataService;

    public AdminController(AppDataService dataService)
    {
        _dataService = dataService;
    }

    public IActionResult Index()
    {
        var role = HttpContext.Session.GetString("UserRole") ?? "Guest";
        if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "Доступ в админ-панель только для Admin.";
            return RedirectToAction("Index", "Home");
        }

        var quests = _dataService.GetQuests();

        ViewBag.TotalCities = quests.Select(q => q.City).Distinct().Count();
        ViewBag.FreeCount = quests.Count(q => q.Price == 0);
        ViewBag.PaidCount = quests.Count(q => q.Price > 0);
        ViewBag.TotalPoints = quests.Sum(q => q.Points.Count);

        return View(quests);
    }
}
