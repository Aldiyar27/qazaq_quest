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
        var visibleCurrentUser = currentUser != null && !string.Equals(currentUser.Role, "Admin", StringComparison.OrdinalIgnoreCase)
            ? currentUser
            : null;

        var model = new LeaderboardViewModel
        {
            TopUsers = _gameService.GetLeaderboard().Take(15).ToList(),
            CurrentUser = visibleCurrentUser,
            CurrentUserPosition = visibleCurrentUser == null ? 0 : _gameService.GetUserRank(visibleCurrentUser.Id),
            IsCurrentUserHiddenFromLeaderboard = currentUser != null && string.Equals(currentUser.Role, "Admin", StringComparison.OrdinalIgnoreCase)
        };

        return View(model);
    }
}
