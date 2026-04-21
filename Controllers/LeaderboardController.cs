using Microsoft.AspNetCore.Mvc;
using QazaqQuest.Services;
using QazaqQuest.ViewModels;

namespace QazaqQuest.Controllers;

public class LeaderboardController : Controller
{
    private readonly GameService _gameService;

    public LeaderboardController(GameService gameService)
    {
        _gameService = gameService;
    }

    public IActionResult Index()
    {
        var currentUser = _gameService.GetCurrentUser(HttpContext);
        var model = new LeaderboardViewModel
        {
            TopUsers = _gameService.GetLeaderboard().Take(15).ToList(),
            CurrentUser = currentUser,
            CurrentUserPosition = currentUser == null ? 0 : _gameService.GetUserRank(currentUser.Id)
        };

        return View(model);
    }
}
