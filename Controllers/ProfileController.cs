using Microsoft.AspNetCore.Mvc;
using QazaqQuest.Models;
using QazaqQuest.Services;

namespace QazaqQuest.Controllers;

public class ProfileController : Controller
{
    private readonly AppDataService _dataService;
    private readonly GameService _gameService;
    private readonly SocialService _socialService;

    public ProfileController(AppDataService dataService, GameService gameService, SocialService socialService)
    {
        _dataService = dataService;
        _gameService = gameService;
        _socialService = socialService;
    }

    public IActionResult Index()
    {
        var allQuests = _dataService.GetQuests();
        var currentUser = _gameService.GetCurrentUser(HttpContext);

        if (currentUser == null)
        {
            var guest = new UserProfile
            {
                Name = HttpContext.Session.GetString("UserName") ?? "Гость",
                Email = HttpContext.Session.GetString("UserEmail") ?? "guest@qazaqquest.demo",
                Role = HttpContext.Session.GetString("UserRole") ?? "Guest"
            };
            return View(guest);
        }

        var completedProgresses = currentUser.QuestProgresses.Where(x => x.IsCompleted).ToList();
        var completedQuests = allQuests.Where(q => completedProgresses.Any(p => p.QuestId == q.Id)).ToList();
        var isAdmin = string.Equals(currentUser.Role, "Admin", StringComparison.OrdinalIgnoreCase);
        var model = new UserProfile
        {
            Name = currentUser.Name,
            Email = currentUser.Email,
            Role = currentUser.Role,
            AvatarUrl = currentUser.AvatarUrl,
            CompletedQuests = completedQuests.Count,
            StartedQuests = currentUser.QuestProgresses.Count,
            TotalPoints = _dataService.GetTotalRewardPoints(completedQuests),
            Achievements = currentUser.Achievements.Count,
            ExperiencePoints = currentUser.ExperiencePoints,
            Coins = currentUser.Coins,
            Level = currentUser.Level,
            RankPosition = isAdmin ? 0 : _gameService.GetUserRank(currentUser.Id)
        };

        ViewBag.RankHidden = isAdmin;
        ViewBag.CompletedQuestTitles = completedQuests.Select(q => q.Title).ToList();
        ViewBag.AchievementsList = currentUser.Achievements.OrderByDescending(x => x.UnlockedAtUtc).ToList();
        ViewBag.InProgress = currentUser.QuestProgresses.Where(x => !x.IsCompleted)
            .Select(x => new { Quest = allQuests.FirstOrDefault(q => q.Id == x.QuestId)?.Title ?? "Маршрут", x.CurrentStep, x.TotalSteps })
            .ToList();
        ViewBag.AvailableQuestCount = allQuests.Count(q => !q.IsHidden || _gameService.CanUserAccessQuest(currentUser, q));
        ViewBag.CityCount = allQuests.Select(q => q.City).Distinct().Count();
        ViewBag.FriendsCount = _socialService.GetFriendsCount(currentUser.Id);
        ViewBag.TodayDailyCompleted = _socialService.GetDailyProgressMap(currentUser.Id).Values.Count(x => x.IsCompleted);
        ViewBag.ReviewsCount = currentUser.QuestReviews.Count;

        return View(model);
    }
}
