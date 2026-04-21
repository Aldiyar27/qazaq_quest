using Microsoft.AspNetCore.Mvc;
using QazaqQuest.Services;
using QazaqQuest.ViewModels;

namespace QazaqQuest.Controllers;

public class SocialController : Controller
{
    private readonly GameService _gameService;
    private readonly SocialService _socialService;

    public SocialController(GameService gameService, SocialService socialService)
    {
        _gameService = gameService;
        _socialService = socialService;
    }

    public IActionResult Index(Guid? friendId)
    {
        var user = _gameService.GetCurrentUser(HttpContext);
        if (user == null)
        {
            TempData["Error"] = "Раздел друзей и чат доступны только после входа.";
            return RedirectToAction("Register", "Auth");
        }

        var selectedFriend = friendId.HasValue ? _socialService.GetUser(friendId.Value) : null;
        var messages = friendId.HasValue && selectedFriend != null && _socialService.AreFriends(user.Id, friendId.Value)
            ? _socialService.GetConversation(user.Id, friendId.Value)
            : new List<QazaqQuest.Models.ChatMessage>();

        return View(new SocialHubViewModel
        {
            CurrentUser = user,
            SuggestedUsers = _socialService.GetSuggestedUsers(user.Id),
            Friends = _socialService.GetFriends(user.Id),
            IncomingRequests = _socialService.GetIncomingRequests(user.Id),
            OutgoingRequests = _socialService.GetOutgoingRequests(user.Id),
            SelectedFriendId = friendId,
            SelectedFriend = selectedFriend,
            Messages = messages
        });
    }

    [HttpPost]
    public IActionResult AddFriend(Guid userId)
    {
        var currentUserId = _gameService.GetCurrentUserId(HttpContext);
        if (currentUserId == null)
            return RedirectToAction("Register", "Auth");

        var sent = _socialService.SendFriendRequest(currentUserId.Value, userId);
        TempData[sent ? "Success" : "Error"] = sent
            ? "Запрос в друзья отправлен."
            : "Не удалось отправить запрос. Возможно, он уже существует.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult Accept(int id)
    {
        var currentUserId = _gameService.GetCurrentUserId(HttpContext);
        if (currentUserId == null)
            return RedirectToAction("Register", "Auth");

        var accepted = _socialService.AcceptFriendRequest(id, currentUserId.Value);
        TempData[accepted ? "Success" : "Error"] = accepted
            ? "Пользователь добавлен в друзья."
            : "Не удалось принять заявку.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult Decline(int id)
    {
        var currentUserId = _gameService.GetCurrentUserId(HttpContext);
        if (currentUserId == null)
            return RedirectToAction("Register", "Auth");

        _socialService.DeclineFriendRequest(id, currentUserId.Value);
        TempData["Success"] = "Заявка отклонена.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult SendMessage(Guid recipientId, string messageText)
    {
        var currentUserId = _gameService.GetCurrentUserId(HttpContext);
        if (currentUserId == null)
            return RedirectToAction("Register", "Auth");

        var sentMessage = _socialService.SendMessage(currentUserId.Value, recipientId, messageText);
        TempData[sentMessage ? "Success" : "Error"] = sentMessage
            ? "Сообщение отправлено."
            : "Не удалось отправить сообщение. Чат доступен только друзьям.";

        return RedirectToAction(nameof(Index), new { friendId = recipientId });
    }
}
