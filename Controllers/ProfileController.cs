using Microsoft.AspNetCore.Mvc;
using QazaqQuest.Models;
using QazaqQuest.Services;

namespace QazaqQuest.Controllers;

public class ProfileController : Controller
{
    private readonly AppDataService _dataService;

    public ProfileController(AppDataService dataService)
    {
        _dataService = dataService;
    }

    public IActionResult Index()
    {
        var allQuests = _dataService.GetQuests();
        var completedQuests = allQuests
            .Where(q => (HttpContext.Session.GetInt32($"Quest_{q.Id}_Step") ?? 0) >= q.Points.Count)
            .ToList();

        var model = new UserProfile
        {
            Name = HttpContext.Session.GetString("UserName") ?? "Гость",
            Email = HttpContext.Session.GetString("UserEmail") ?? "guest@qazaqquest.demo",
            Role = HttpContext.Session.GetString("UserRole") ?? "Guest",
            CompletedQuests = completedQuests.Count,
            TotalPoints = _dataService.GetTotalRewardPoints(completedQuests),
            Achievements = completedQuests.Sum(q => q.Rewards.Count)
        };

        ViewBag.CompletedQuestTitles = completedQuests.Select(q => q.Title).ToList();
        ViewBag.AvailableQuestCount = allQuests.Count;
        ViewBag.CityCount = allQuests.Select(q => q.City).Distinct().Count();

        return View(model);
    }
}
