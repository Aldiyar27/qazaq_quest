using Microsoft.AspNetCore.Mvc;
using QazaqQuest.Services;
using QazaqQuest.ViewModels;

namespace QazaqQuest.Controllers;

public class QuestController : Controller
{
    private readonly AppDataService _dataService;
    private readonly GameService _gameService;

    public QuestController(AppDataService dataService, GameService gameService)
    {
        _dataService = dataService;
        _gameService = gameService;
    }

    public IActionResult Index(string? city, string? difficulty, string? type, string? search)
    {
        var currentUser = _gameService.GetCurrentUser(HttpContext);
        var quests = _dataService.FilterQuests(city, difficulty, type, search)
            .Where(q => !q.IsHidden || (currentUser != null && _gameService.CanUserAccessQuest(currentUser, q)))
            .ToList();

        var model = new QuestListViewModel
        {
            City = city,
            Difficulty = difficulty,
            Type = type,
            Search = search,
            Quests = quests,
            AvailableCities = _dataService.GetCities(),
            AvailableDifficulties = _dataService.GetDifficulties(),
            AvailableTypes = _dataService.GetTypes()
        };

        ViewBag.CurrentLevel = currentUser?.Level ?? 0;
        return View(model);
    }

    public IActionResult Details(int id)
    {
        var quest = _dataService.GetQuestById(id);
        if (quest == null)
            return NotFound();

        var user = _gameService.GetCurrentUser(HttpContext);
        ViewBag.IsRegistered = IsRegisteredUser();
        ViewBag.CanAccessQuest = user != null && _gameService.CanUserAccessQuest(user, quest);
        ViewBag.CurrentLevel = user?.Level ?? 0;
        ViewBag.RouteUrl = BuildRouteUrl(quest);
        return View(quest);
    }

    public IActionResult Start(int id)
    {
        if (!IsRegisteredUser())
        {
            TempData["Error"] = "Чтобы участвовать в квесте, сначала зарегистрируйся или войди в аккаунт.";
            return RedirectToAction("Register", "Auth");
        }

        var quest = _dataService.GetQuestById(id);
        var user = _gameService.GetCurrentUser(HttpContext);
        if (quest == null || user == null)
            return NotFound();

        if (!_gameService.CanUserAccessQuest(user, quest))
        {
            TempData["Error"] = $"Этот маршрут откроется на уровне {quest.UnlockLevel}. Сейчас у тебя уровень {user.Level}.";
            return RedirectToAction(nameof(Details), new { id });
        }

        HttpContext.Session.SetInt32($"Quest_{id}_Step", 0);
        _gameService.StartQuest(user, quest);

        foreach (var point in quest.Points)
        {
            HttpContext.Session.Remove(GetLocationSessionKey(id, point.Id));
            HttpContext.Session.Remove(GetAnswerAttemptsSessionKey(id, point.Id));
        }

        TempData["Success"] = $"Квест «{quest.Title}» начат. Разреши геолокацию на первой точке.";
        return RedirectToAction(nameof(Play), new { id });
    }

    public IActionResult Play(int id, string? message = null)
    {
        if (!IsRegisteredUser())
        {
            TempData["Error"] = "Без регистрации прохождение квеста недоступно.";
            return RedirectToAction("Register", "Auth");
        }

        var quest = _dataService.GetQuestById(id);
        var userId = _gameService.GetCurrentUserId(HttpContext);
        if (quest == null || userId == null)
            return NotFound();

        var orderedPoints = quest.Points.OrderBy(p => p.Order).ToList();
        var progress = _gameService.GetUserQuestProgress(userId.Value, id);
        var currentStepIndex = progress?.CurrentStep ?? (HttpContext.Session.GetInt32($"Quest_{id}_Step") ?? 0);
        HttpContext.Session.SetInt32($"Quest_{id}_Step", currentStepIndex);

        if (currentStepIndex >= orderedPoints.Count)
        {
            return View(new QuestPlayViewModel
            {
                Quest = quest,
                IsCompleted = true,
                TotalSteps = orderedPoints.Count,
                CurrentStepIndex = orderedPoints.Count,
                Message = "Квест завершён. Опыт, монеты и достижения начислены в профиль."
            });
        }

        var currentPoint = orderedPoints[currentStepIndex];
        ViewBag.AnswerAttempts = HttpContext.Session.GetInt32(GetAnswerAttemptsSessionKey(id, currentPoint.Id)) ?? 0;
        ViewBag.TimeLimit = quest.IsTimed ? quest.TimeLimitMinutes : 0;
        ViewBag.ExperienceReward = quest.ExperienceReward;
        ViewBag.CoinsReward = quest.CoinsReward;

        return View(new QuestPlayViewModel
        {
            Quest = quest,
            CurrentPoint = currentPoint,
            CurrentStepIndex = currentStepIndex + 1,
            TotalSteps = orderedPoints.Count,
            Message = message,
            LocationVerified = string.Equals(
                HttpContext.Session.GetString(GetLocationSessionKey(id, currentPoint.Id)),
                "true",
                StringComparison.OrdinalIgnoreCase)
        });
    }

    [HttpPost]
    public IActionResult VerifyLocation([FromBody] LocationCheckRequest request)
    {
        if (!IsRegisteredUser())
            return Json(new { success = false, message = "Сначала зарегистрируйся или войди в аккаунт." });

        var quest = _dataService.GetQuestById(request.Id);
        if (quest == null)
            return Json(new { success = false, message = "Квест не найден." });

        var currentStep = HttpContext.Session.GetInt32($"Quest_{request.Id}_Step") ?? 0;
        var currentPoint = quest.Points.OrderBy(p => p.Order).ElementAtOrDefault(currentStep);

        if (currentPoint == null || currentPoint.Id != request.PointId)
            return Json(new { success = false, message = "Сейчас активна другая точка маршрута." });

        var distance = _dataService.CalculateDistanceMeters(request.Latitude, request.Longitude, currentPoint.Latitude, currentPoint.Longitude);
        var withinRadius = distance <= currentPoint.RadiusMeters;

        if (withinRadius)
            HttpContext.Session.SetString(GetLocationSessionKey(request.Id, request.PointId), "true");
        else
            HttpContext.Session.Remove(GetLocationSessionKey(request.Id, request.PointId));

        return Json(new
        {
            success = true,
            withinRadius,
            distanceMeters = Math.Round(distance, 1),
            radiusMeters = currentPoint.RadiusMeters,
            pointName = currentPoint.Name,
            message = withinRadius
                ? $"Локация подтверждена. Ты находишься рядом с точкой «{currentPoint.Name}»."
                : $"Ты пока далеко от точки «{currentPoint.Name}». Подойди ближе и попробуй ещё раз."
        });
    }

    [HttpPost]
    public IActionResult CheckAnswer(int id, int pointId, string answer)
    {
        if (!IsRegisteredUser())
        {
            TempData["Error"] = "Для проверки задания нужно войти в аккаунт.";
            return RedirectToAction("Register", "Auth");
        }

        var quest = _dataService.GetQuestById(id);
        var userId = _gameService.GetCurrentUserId(HttpContext);
        if (quest == null || userId == null)
            return NotFound();

        var orderedPoints = quest.Points.OrderBy(p => p.Order).ToList();
        var currentStep = HttpContext.Session.GetInt32($"Quest_{id}_Step") ?? 0;
        var currentPoint = orderedPoints.ElementAtOrDefault(currentStep);

        if (currentPoint == null || currentPoint.Id != pointId)
            return RedirectToAction(nameof(Play), new { id, message = "Нельзя перескакивать между точками маршрута." });

        var isLocationVerified = string.Equals(HttpContext.Session.GetString(GetLocationSessionKey(id, pointId)), "true", StringComparison.OrdinalIgnoreCase);
        if (!isLocationVerified)
            return RedirectToAction(nameof(Play), new { id, message = "Сначала подтверди геолокацию на текущей точке." });

        if (Normalize(currentPoint.Answer) == Normalize(answer))
        {
            var nextStep = currentStep + 1;
            HttpContext.Session.SetInt32($"Quest_{id}_Step", nextStep);
            HttpContext.Session.Remove(GetLocationSessionKey(id, pointId));
            HttpContext.Session.Remove(GetAnswerAttemptsSessionKey(id, pointId));

            var completed = nextStep >= orderedPoints.Count;
            _gameService.AdvanceStep(userId.Value, quest, completed);

            if (completed)
                TempData["Success"] = $"Квест «{quest.Title}» завершён! Опыт, монеты и достижения начислены.";

            return RedirectToAction(nameof(Play), new { id, message = completed ? "Маршрут закрыт. Профиль и лидерборд уже обновлены." : "Верно! Проверка задания пройдена, следующая точка открыта." });
        }

        var attempts = (HttpContext.Session.GetInt32(GetAnswerAttemptsSessionKey(id, pointId)) ?? 0) + 1;
        HttpContext.Session.SetInt32(GetAnswerAttemptsSessionKey(id, pointId), attempts);
        _gameService.RecordAttempt(userId.Value, id);

        return RedirectToAction(nameof(Play), new { id, message = $"Ответ неверный. Попытка №{attempts}. Используй подсказку и попробуй снова." });
    }

    private bool IsRegisteredUser() =>
        !string.Equals(HttpContext.Session.GetString("UserRole") ?? "Guest", "Guest", StringComparison.OrdinalIgnoreCase);

    private static string Normalize(string value) => value.Trim().Replace("ё", "е", StringComparison.OrdinalIgnoreCase).ToLowerInvariant();
    private static string GetLocationSessionKey(int questId, int pointId) => $"Quest_{questId}_Point_{pointId}_Verified";
    private static string GetAnswerAttemptsSessionKey(int questId, int pointId) => $"Quest_{questId}_Point_{pointId}_Attempts";

    private static string BuildRouteUrl(QazaqQuest.Models.Quest quest)
    {
        var ordered = quest.Locations.OrderBy(x => x.Id).ToList();
        if (!ordered.Any()) return string.Empty;
        var points = string.Join('/', ordered.Select(x => $"{x.Latitude},{x.Longitude}"));
        return $"https://www.google.com/maps/dir/{points}";
    }
}
