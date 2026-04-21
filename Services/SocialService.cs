using Microsoft.EntityFrameworkCore;
using QazaqQuest.Data;
using QazaqQuest.Models;

namespace QazaqQuest.Services;

public class SocialService
{
    private readonly AppDbContext _dbContext;

    public SocialService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void EnsureSeedData()
    {
        var today = DateTime.UtcNow.Date;
        if (_dbContext.DailyChallenges.Any(x => x.ActiveDateUtc == today))
            return;

        _dbContext.DailyChallenges.AddRange(new List<DailyChallenge>
        {
            new() { Title = "Пройди 1 квест", Description = "Заверши любой один маршрут за день.", ChallengeType = "CompleteQuests", TargetValue = 1, ExperienceReward = 60, CoinsReward = 20, ActiveDateUtc = today },
            new() { Title = "Ответь без ошибок", Description = "Пройди одну точку без лишних попыток.", ChallengeType = "PreciseAnswers", TargetValue = 1, ExperienceReward = 40, CoinsReward = 15, ActiveDateUtc = today },
            new() { Title = "Оставь отзыв", Description = "Поставь оценку и напиши отзыв к пройденному квесту.", ChallengeType = "PublishReviews", TargetValue = 1, ExperienceReward = 35, CoinsReward = 10, ActiveDateUtc = today },
            new() { Title = "Напиши другу", Description = "Отправь минимум 3 сообщения в дружеском чате.", ChallengeType = "SendMessages", TargetValue = 3, ExperienceReward = 45, CoinsReward = 15, ActiveDateUtc = today }
        });

        _dbContext.SaveChanges();
    }

    public List<DailyChallenge> GetTodayChallenges() => _dbContext.DailyChallenges
        .Where(x => x.IsActive && x.ActiveDateUtc == DateTime.UtcNow.Date)
        .OrderBy(x => x.Id)
        .ToList();

    public Dictionary<int, DailyChallengeProgress> GetDailyProgressMap(Guid userId)
    {
        return _dbContext.DailyChallengeProgresses
            .Include(x => x.DailyChallenge)
            .Where(x => x.UserId == userId && x.DailyChallenge!.ActiveDateUtc == DateTime.UtcNow.Date)
            .ToDictionary(x => x.DailyChallengeId, x => x);
    }

    public void UpdateDailyProgress(Guid userId, string challengeType, int increment = 1)
    {
        var todayChallenges = _dbContext.DailyChallenges
            .Where(x => x.IsActive && x.ActiveDateUtc == DateTime.UtcNow.Date && x.ChallengeType == challengeType)
            .ToList();

        foreach (var challenge in todayChallenges)
        {
            var progress = _dbContext.DailyChallengeProgresses
                .FirstOrDefault(x => x.UserId == userId && x.DailyChallengeId == challenge.Id);

            if (progress == null)
            {
                progress = new DailyChallengeProgress
                {
                    UserId = userId,
                    DailyChallengeId = challenge.Id,
                    CurrentValue = 0,
                    CreatedAtUtc = DateTime.UtcNow
                };
                _dbContext.DailyChallengeProgresses.Add(progress);
            }

            if (progress.IsCompleted) continue;

            progress.CurrentValue += increment;
            if (progress.CurrentValue >= challenge.TargetValue)
            {
                progress.CurrentValue = challenge.TargetValue;
                progress.IsCompleted = true;
                progress.CompletedAtUtc = DateTime.UtcNow;
            }
        }

        _dbContext.SaveChanges();
    }

    public bool ClaimDailyReward(Guid userId, int challengeId)
    {
        var progress = _dbContext.DailyChallengeProgresses
            .Include(x => x.DailyChallenge)
            .FirstOrDefault(x => x.UserId == userId && x.DailyChallengeId == challengeId);
        if (progress == null || !progress.IsCompleted || progress.IsRewardClaimed || progress.DailyChallenge == null)
            return false;

        var user = _dbContext.Users.First(x => x.Id == userId);
        user.ExperiencePoints += progress.DailyChallenge.ExperienceReward;
        user.Coins += progress.DailyChallenge.CoinsReward;
        user.Level = Math.Max(1, (user.ExperiencePoints / 200) + 1);
        progress.IsRewardClaimed = true;
        _dbContext.SaveChanges();
        return true;
    }

    public List<AppUser> GetSuggestedUsers(Guid currentUserId)
    {
        var connectedIds = _dbContext.Friendships
            .Where(x => x.RequesterId == currentUserId || x.AddresseeId == currentUserId)
            .Select(x => x.RequesterId == currentUserId ? x.AddresseeId : x.RequesterId)
            .ToHashSet();

        return _dbContext.Users
    .Where(x => x.Id != currentUserId
        && ((x.Role ?? "").ToLower() != "admin")
        && !connectedIds.Contains(x.Id))
    .OrderByDescending(x => x.Level)
    .ThenByDescending(x => x.ExperiencePoints)
    .Take(8)
    .ToList();
    }

    public List<Friendship> GetFriends(Guid currentUserId)
    {
        return _dbContext.Friendships
            .Include(x => x.Requester)
            .Include(x => x.Addressee)
            .Where(x => x.Status == "Accepted" && (x.RequesterId == currentUserId || x.AddresseeId == currentUserId))
            .OrderByDescending(x => x.AcceptedAtUtc)
            .ToList();
    }

    public List<Friendship> GetIncomingRequests(Guid currentUserId) => _dbContext.Friendships
        .Include(x => x.Requester)
        .Where(x => x.AddresseeId == currentUserId && x.Status == "Pending")
        .OrderByDescending(x => x.CreatedAtUtc)
        .ToList();

    public List<Friendship> GetOutgoingRequests(Guid currentUserId) => _dbContext.Friendships
        .Include(x => x.Addressee)
        .Where(x => x.RequesterId == currentUserId && x.Status == "Pending")
        .OrderByDescending(x => x.CreatedAtUtc)
        .ToList();

    public bool SendFriendRequest(Guid requesterId, Guid addresseeId)
    {
        if (requesterId == addresseeId) return false;
        var exists = _dbContext.Friendships.Any(x =>
            (x.RequesterId == requesterId && x.AddresseeId == addresseeId) ||
            (x.RequesterId == addresseeId && x.AddresseeId == requesterId));
        if (exists) return false;

        _dbContext.Friendships.Add(new Friendship
        {
            RequesterId = requesterId,
            AddresseeId = addresseeId,
            Status = "Pending",
            CreatedAtUtc = DateTime.UtcNow
        });
        _dbContext.SaveChanges();
        return true;
    }

    public bool AcceptFriendRequest(int friendshipId, Guid currentUserId)
    {
        var friendship = _dbContext.Friendships.FirstOrDefault(x => x.Id == friendshipId && x.AddresseeId == currentUserId && x.Status == "Pending");
        if (friendship == null) return false;
        friendship.Status = "Accepted";
        friendship.AcceptedAtUtc = DateTime.UtcNow;
        _dbContext.SaveChanges();
        return true;
    }

    public bool DeclineFriendRequest(int friendshipId, Guid currentUserId)
    {
        var friendship = _dbContext.Friendships.FirstOrDefault(x => x.Id == friendshipId && x.AddresseeId == currentUserId && x.Status == "Pending");
        if (friendship == null) return false;
        _dbContext.Friendships.Remove(friendship);
        _dbContext.SaveChanges();
        return true;
    }

    public bool AreFriends(Guid firstUserId, Guid secondUserId) => _dbContext.Friendships.Any(x =>
        x.Status == "Accepted" &&
        ((x.RequesterId == firstUserId && x.AddresseeId == secondUserId) ||
         (x.RequesterId == secondUserId && x.AddresseeId == firstUserId)));

    public List<ChatMessage> GetConversation(Guid currentUserId, Guid friendId)
    {
        return _dbContext.ChatMessages
            .Include(x => x.Sender)
            .Include(x => x.Recipient)
            .Where(x => (x.SenderId == currentUserId && x.RecipientId == friendId) || (x.SenderId == friendId && x.RecipientId == currentUserId))
            .OrderBy(x => x.SentAtUtc)
            .Take(150)
            .ToList();
    }

    public bool SendMessage(Guid senderId, Guid recipientId, string text)
    {
        text = text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(text) || text.Length > 1500) return false;
        if (!AreFriends(senderId, recipientId)) return false;

        _dbContext.ChatMessages.Add(new ChatMessage
        {
            SenderId = senderId,
            RecipientId = recipientId,
            MessageText = text,
            SentAtUtc = DateTime.UtcNow
        });
        _dbContext.SaveChanges();
        UpdateDailyProgress(senderId, "SendMessages", 1);
        return true;
    }

    public AppUser? GetUser(Guid userId) => _dbContext.Users.FirstOrDefault(x => x.Id == userId);

    public List<QuestReview> GetQuestReviews(int questId) => _dbContext.QuestReviews
        .Include(x => x.User)
        .Where(x => x.QuestId == questId)
        .OrderByDescending(x => x.CreatedAtUtc)
        .ToList();

    public double GetAverageRating(int questId)
    {
        var ratings = _dbContext.QuestReviews.Where(x => x.QuestId == questId).Select(x => (double)x.Rating).ToList();
        return ratings.Count == 0 ? 0 : ratings.Average();
    }

    public bool UpsertQuestReview(Guid userId, int questId, int rating, string comment)
    {
        comment = comment?.Trim() ?? string.Empty;
        if (rating < 1 || rating > 5 || string.IsNullOrWhiteSpace(comment)) return false;

        var completed = _dbContext.UserQuestProgresses.Any(x => x.UserId == userId && x.QuestId == questId && x.IsCompleted);
        if (!completed) return false;

        var review = _dbContext.QuestReviews.FirstOrDefault(x => x.UserId == userId && x.QuestId == questId);
        if (review == null)
        {
            review = new QuestReview
            {
                UserId = userId,
                QuestId = questId,
                Rating = rating,
                Comment = comment,
                CreatedAtUtc = DateTime.UtcNow
            };
            _dbContext.QuestReviews.Add(review);
            UpdateDailyProgress(userId, "PublishReviews", 1);
        }
        else
        {
            review.Rating = rating;
            review.Comment = comment;
            review.CreatedAtUtc = DateTime.UtcNow;
        }

        _dbContext.SaveChanges();
        return true;
    }

    public int GetFriendsCount(Guid userId) => _dbContext.Friendships.Count(x => x.Status == "Accepted" && (x.RequesterId == userId || x.AddresseeId == userId));
}
