using Microsoft.AspNetCore.Mvc;
using QazaqQuest.Services;
using QazaqQuest.ViewModels;

namespace QazaqQuest.Controllers;

public class DailyController : Controller
{
    private readonly GameService _gameService;
    private readonly SocialService _socialService;

    public DailyController(GameService gameService, SocialService socialService)
    {
        _gameService = gameService;
        _socialService = socialService;
    }

    public IActionResult Index()
    {
        var user = _gameService.GetCurrentUser(HttpContext);
        if (user == null)
        {
            TempData["Error"] = "Ежедневные задания доступны только зарегистрированным игрокам.";
            return RedirectToAction("Register", "Auth");
        }

        var challenges = _socialService.GetTodayChallenges();
        var progress = _socialService.GetDailyProgressMap(user.Id);

        return View(new DailyChallengeIndexViewModel
        {
            CurrentUser = user,
            Challenges = challenges,
            ProgressByChallengeId = progress
        });
    }

    [HttpPost]
    public IActionResult Claim(int id)
    {
        var userId = _gameService.GetCurrentUserId(HttpContext);
        if (userId == null)
            return RedirectToAction("Register", "Auth");

        var claimed = _socialService.ClaimDailyReward(userId.Value, id);
        TempData[claimed ? "Success" : "Error"] = claimed
            ? "Награда за ежедневное задание получена."
            : "Пока нельзя забрать награду за это задание.";

        return RedirectToAction(nameof(Index));
    }
}
