using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QazaqQuest.Data;
using QazaqQuest.Models;
using QazaqQuest.Services;
using QazaqQuest.ViewModels;

namespace QazaqQuest.Controllers;

public class AdminController : Controller
{
    private readonly AppDataService _dataService;
    private readonly AppDbContext _dbContext;

    public AdminController(AppDataService dataService, AppDbContext dbContext)
    {
        _dataService = dataService;
        _dbContext = dbContext;
    }

    public IActionResult Index()
    {
        if (!IsAdmin())
        {
            TempData["Error"] = "Доступ в админ-панель только для Admin.";
            return RedirectToAction("Index", "Home");
        }

        var quests = _dataService.GetQuests();
        ViewBag.TotalCities = quests.Select(q => q.City).Distinct().Count();
        ViewBag.FreeCount = quests.Count(q => q.Price == 0);
        ViewBag.PaidCount = quests.Count(q => q.Price > 0);
        ViewBag.TotalPoints = quests.Sum(q => q.Points.Count);
        ViewBag.TotalLocations = quests.Sum(q => q.Locations.Count);
        return View(quests);
    }

    public IActionResult CreateQuest()
    {
        if (!IsAdmin()) return RedirectToAction("Index", "Home");
        return View("QuestForm", BuildFormModel());
    }

    public IActionResult EditQuest(int id)
    {
        if (!IsAdmin()) return RedirectToAction("Index", "Home");
        var quest = _dataService.GetQuestById(id);
        if (quest == null) return NotFound();
        return View("QuestForm", BuildFormModel(quest));
    }

    [HttpPost]
    public IActionResult SaveQuest(AdminQuestFormViewModel model)
    {
        if (!IsAdmin()) return RedirectToAction("Index", "Home");

        model.Points ??= new();
        model.Locations ??= new();
        model.Rewards ??= new();

        model.Points = model.Points.Where(x => !string.IsNullOrWhiteSpace(x.Name) || !string.IsNullOrWhiteSpace(x.Task)).ToList();
        model.Locations = model.Locations.Where(x => !string.IsNullOrWhiteSpace(x.Name)).ToList();
        model.Rewards = model.Rewards.Where(x => !string.IsNullOrWhiteSpace(x.Title)).ToList();

        if (string.IsNullOrWhiteSpace(model.Quest.Title) || string.IsNullOrWhiteSpace(model.Quest.Description))
        {
            TempData["Error"] = "У квеста должны быть минимум название и описание.";
            return View("QuestForm", model);
        }

        Quest quest;
        if (model.Quest.Id == 0)
        {
            quest = model.Quest;
            quest.Points = new();
            quest.Locations = new();
            quest.Rewards = new();
            _dbContext.Quests.Add(quest);
        }
        else
        {
            quest = _dbContext.Quests
                .Include(q => q.Points)
                .Include(q => q.Locations)
                .Include(q => q.Rewards)
                .FirstOrDefault(q => q.Id == model.Quest.Id) ?? throw new InvalidOperationException("Квест не найден");

            quest.Title = model.Quest.Title;
            quest.Description = model.Quest.Description;
            quest.City = model.Quest.City;
            quest.Difficulty = model.Quest.Difficulty;
            quest.Type = model.Quest.Type;
            quest.Price = model.Quest.Price;
            quest.Duration = model.Quest.Duration;
            quest.RouteLength = model.Quest.RouteLength;
            quest.Category = model.Quest.Category;
            quest.Audience = model.Quest.Audience;
            quest.ImageUrl = model.Quest.ImageUrl;
            quest.CoverStyle = model.Quest.CoverStyle;
            quest.Icon = model.Quest.Icon;
            quest.Language = model.Quest.Language;
            quest.Partner = model.Quest.Partner;
            quest.Bonus = model.Quest.Bonus;

            _dbContext.QuestPoints.RemoveRange(quest.Points);
            _dbContext.QuestLocations.RemoveRange(quest.Locations);
            _dbContext.Rewards.RemoveRange(quest.Rewards);
            quest.Points = new();
            quest.Locations = new();
            quest.Rewards = new();
        }

        foreach (var location in model.Locations)
        {
            quest.Locations.Add(new QuestLocation
            {
                Name = location.Name,
                Address = location.Address,
                Latitude = location.Latitude,
                Longitude = location.Longitude,
                RadiusMeters = location.RadiusMeters <= 0 ? 150 : location.RadiusMeters
            });
        }

        foreach (var point in model.Points.OrderBy(x => x.Order))
        {
            QuestLocation? linkedLocation = null;
            if (point.QuestLocationId.HasValue && point.QuestLocationId.Value > 0 && point.QuestLocationId.Value <= quest.Locations.Count)
                linkedLocation = quest.Locations[point.QuestLocationId.Value - 1];

            quest.Points.Add(new QuestPoint
            {
                Name = point.Name,
                Task = point.Task,
                Answer = point.Answer,
                TaskType = point.TaskType,
                Hint = point.Hint,
                Order = point.Order,
                Location = linkedLocation,
                Latitude = linkedLocation?.Latitude ?? 0,
                Longitude = linkedLocation?.Longitude ?? 0,
                RadiusMeters = linkedLocation?.RadiusMeters ?? 0,
                Options = point.Options
            });
        }

        foreach (var reward in model.Rewards)
        {
            quest.Rewards.Add(new Reward
            {
                Title = reward.Title,
                Description = reward.Description,
                Points = reward.Points
            });
        }

        _dbContext.SaveChanges();
        TempData["Success"] = model.Quest.Id == 0 ? "Квест добавлен в базу данных." : "Квест обновлён.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult DeleteQuest(int id)
    {
        if (!IsAdmin()) return RedirectToAction("Index", "Home");
        var quest = _dbContext.Quests.FirstOrDefault(q => q.Id == id);
        if (quest == null) return NotFound();
        _dbContext.Quests.Remove(quest);
        _dbContext.SaveChanges();
        TempData["Success"] = "Квест удалён.";
        return RedirectToAction(nameof(Index));
    }

    private AdminQuestFormViewModel BuildFormModel(Quest? quest = null)
    {
        quest ??= new Quest
        {
            City = "Астана",
            Difficulty = "Лёгкий",
            Type = "Бесплатный",
            Language = "RU / KZ / EN",
            CoverStyle = "linear-gradient(135deg, #114b5f 0%, #1a936f 100%)",
            Icon = "🧭"
        };

        var locations = quest.Locations.Any()
            ? quest.Locations.ToList()
            : new List<QuestLocation> { new(), new(), new(), new() };

        var points = quest.Points.Any()
            ? quest.Points.OrderBy(p => p.Order).Select(p => new QuestPoint
            {
                Id = p.Id,
                Name = p.Name,
                Task = p.Task,
                Answer = p.Answer,
                TaskType = p.TaskType,
                Hint = p.Hint,
                Order = p.Order,
                QuestLocationId = p.Location == null ? null : quest.Locations.OrderBy(l => l.Id).ToList().FindIndex(l => l.Id == p.Location.Id) + 1,
                Options = p.Options
            }).ToList()
            : new List<QuestPoint>
            {
                new() { Order = 1, TaskType = "Текстовый вопрос" },
                new() { Order = 2, TaskType = "Выбор варианта" },
                new() { Order = 3, TaskType = "Фото-задание" },
                new() { Order = 4, TaskType = "Текстовый вопрос" }
            };

        var rewards = quest.Rewards.Any() ? quest.Rewards.ToList() : new List<Reward> { new(), new() };

        return new AdminQuestFormViewModel
        {
            Quest = quest,
            Points = points,
            Locations = locations,
            Rewards = rewards
        };
    }

    private bool IsAdmin() => string.Equals(HttpContext.Session.GetString("UserRole"), "Admin", StringComparison.OrdinalIgnoreCase);
}
