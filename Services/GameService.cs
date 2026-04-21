using Microsoft.EntityFrameworkCore;
using QazaqQuest.Data;
using QazaqQuest.Models;

namespace QazaqQuest.Services;

public class GameService
{
    private readonly AppDbContext _dbContext;

    public GameService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Guid? GetCurrentUserId(HttpContext httpContext)
    {
        var email = httpContext.Session.GetString("UserEmail");
        if (string.IsNullOrWhiteSpace(email)) return null;
        return _dbContext.Users.Where(x => x.Email == email).Select(x => (Guid?)x.Id).FirstOrDefault();
    }

    public AppUser? GetCurrentUser(HttpContext httpContext)
    {
        var email = httpContext.Session.GetString("UserEmail");
        if (string.IsNullOrWhiteSpace(email)) return null;
        return _dbContext.Users
            .Include(x => x.QuestProgresses).ThenInclude(x => x.Quest)
            .Include(x => x.Achievements)
            .FirstOrDefault(x => x.Email == email);
    }

    public bool CanUserAccessQuest(AppUser? user, Quest quest)
    {
        if (user == null) return false;
        if (quest.IsHidden)
        {
            var hiddenUnlocked = _dbContext.UserQuestProgresses.Any(x => x.UserId == user.Id && x.QuestId == quest.Id && x.IsHiddenUnlocked);
            if (!hiddenUnlocked) return false;
        }
        return user.Level >= quest.UnlockLevel;
    }

    public UserQuestProgress StartQuest(AppUser user, Quest quest)
    {
        var progress = _dbContext.UserQuestProgresses.FirstOrDefault(x => x.UserId == user.Id && x.QuestId == quest.Id);
        if (progress == null)
        {
            progress = new UserQuestProgress
            {
                UserId = user.Id,
                QuestId = quest.Id,
                CurrentStep = 0,
                TotalSteps = quest.Points.Count,
                StartedAtUtc = DateTime.UtcNow,
                LastPlayedAtUtc = DateTime.UtcNow
            };
            _dbContext.UserQuestProgresses.Add(progress);
        }
        else
        {
            progress.TotalSteps = quest.Points.Count;
            progress.LastPlayedAtUtc = DateTime.UtcNow;
        }

        _dbContext.SaveChanges();
        return progress;
    }

    public UserQuestProgress? GetUserQuestProgress(Guid userId, int questId)
    {
        return _dbContext.UserQuestProgresses.FirstOrDefault(x => x.UserId == userId && x.QuestId == questId);
    }

    public void RecordAttempt(Guid userId, int questId)
    {
        var progress = _dbContext.UserQuestProgresses.FirstOrDefault(x => x.UserId == userId && x.QuestId == questId);
        if (progress == null) return;
        progress.AttemptsCount += 1;
        progress.LastPlayedAtUtc = DateTime.UtcNow;
        _dbContext.SaveChanges();
    }

    public void AdvanceStep(Guid userId, Quest quest, bool completed)
    {
        var progress = _dbContext.UserQuestProgresses.FirstOrDefault(x => x.UserId == userId && x.QuestId == quest.Id);
        if (progress == null) return;

        if (!completed)
        {
            progress.CurrentStep += 1;
            progress.LastPlayedAtUtc = DateTime.UtcNow;
        }
        else
        {
            progress.CurrentStep = quest.Points.Count;
            progress.IsCompleted = true;
            progress.CompletedAtUtc = DateTime.UtcNow;
            progress.LastPlayedAtUtc = DateTime.UtcNow;
            if (!progress.IsRewardClaimed)
            {
                var user = _dbContext.Users.Include(x => x.Achievements).First(x => x.Id == userId);
                user.ExperiencePoints += quest.ExperienceReward;
                user.Coins += quest.CoinsReward;
                user.Level = CalculateLevel(user.ExperiencePoints);
                progress.IsRewardClaimed = true;
                UnlockAchievements(user, quest);
                UnlockHiddenQuests(user);
            }
        }

        _dbContext.SaveChanges();
    }

    public List<AppUser> GetLeaderboard() => _dbContext.Users
        .Include(x => x.QuestProgresses)
        .Include(x => x.Achievements)
        .Where(a => a.Role.ToLower() != "admin")
        .OrderByDescending(x => x.ExperiencePoints)
        .ThenByDescending(x => x.Coins)
        .ThenByDescending(x => x.QuestProgresses.Count(q => q.IsCompleted))
        .ToList();

    public int GetUserRank(Guid userId)
    {
        var ranked = GetLeaderboard();
        var idx = ranked.FindIndex(x => x.Id == userId);
        return idx >= 0 ? idx + 1 : 0;
    }

    private static int CalculateLevel(int xp) => Math.Max(1, (xp / 200) + 1);

    private void UnlockAchievements(AppUser user, Quest quest)
    {
        TryAddAchievement(user, $"quest-{quest.Id}", $"Маршрут завершён: {quest.Title}", "Игрок полностью закрыл квест и забрал награды.", 25);

        var completedCount = _dbContext.UserQuestProgresses.Count(x => x.UserId == user.Id && x.IsCompleted);
        if (completedCount >= 1)
            TryAddAchievement(user, "first-quest", "Первый финиш", "Завершён первый квест в системе.", 20);
        if (completedCount >= 3)
            TryAddAchievement(user, "three-quests", "Охотник за маршрутами", "Закрыто минимум три квеста.", 40);
        if (completedCount >= 5)
            TryAddAchievement(user, "five-quests", "Легенда города", "Закрыто минимум пять квестов.", 60);

        if (quest.IsTimed)
            TryAddAchievement(user, "timed-runner", "На время", "Пройден квест с ограничением по времени.", 30);
        if (quest.IsCoop)
            TryAddAchievement(user, "team-player", "Командный игрок", "Завершён кооперативный маршрут.", 30);
    }

    private void UnlockHiddenQuests(AppUser user)
    {
        var completedCount = _dbContext.UserQuestProgresses.Count(x => x.UserId == user.Id && x.IsCompleted);
        if (completedCount < 2) return;

        var hiddenQuests = _dbContext.Quests.Where(x => x.IsHidden).ToList();
        foreach (var quest in hiddenQuests)
        {
            var progress = _dbContext.UserQuestProgresses.FirstOrDefault(x => x.UserId == user.Id && x.QuestId == quest.Id);
            if (progress == null)
            {
                progress = new UserQuestProgress { UserId = user.Id, QuestId = quest.Id, TotalSteps = quest.Points.Count };
                _dbContext.UserQuestProgresses.Add(progress);
            }
            progress.IsHiddenUnlocked = true;
        }
        TryAddAchievement(user, "hidden-unlock", "Тайные маршруты", "Открыт доступ к скрытым квестам.", 35);
    }

    private void TryAddAchievement(AppUser user, string code, string title, string description, int rewardPoints)
    {
        if (_dbContext.UserAchievements.Any(x => x.UserId == user.Id && x.Code == code)) return;
        _dbContext.UserAchievements.Add(new UserAchievement
        {
            UserId = user.Id,
            Code = code,
            Title = title,
            Description = description,
            RewardPoints = rewardPoints,
            UnlockedAtUtc = DateTime.UtcNow
        });
        user.ExperiencePoints += rewardPoints;
        user.Level = CalculateLevel(user.ExperiencePoints);
    }
}
