using Microsoft.EntityFrameworkCore;
using QazaqQuest.Data;
using QazaqQuest.Models;

namespace QazaqQuest.Services;

public class UserStoreService
{
    private readonly AppDbContext _dbContext;

    public UserStoreService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public bool UserNameExists(string name) =>
        _dbContext.Users.Any(u => u.Name.ToLower() == name.Trim().ToLower());

    public bool EmailExists(string email) =>
        _dbContext.Users.Any(u => u.Email.ToLower() == email.Trim().ToLower());

    public AppUser? FindByEmail(string email) =>
        _dbContext.Users.FirstOrDefault(u => u.Email.ToLower() == email.Trim().ToLower());

    public AppUser? FindByName(string name) =>
        _dbContext.Users.FirstOrDefault(u => u.Name.ToLower() == name.Trim().ToLower());

    public List<AppUser> GetAllUsers() =>
        _dbContext.Users.OrderBy(u => u.CreatedAtUtc).ToList();

    public (bool Success, string ErrorMessage, AppUser? User) Register(string name, string email, string password)
    {
        name = name.Trim();
        email = email.Trim().ToLowerInvariant();

        if (UserNameExists(name))
            return (false, "Пользователь с таким именем уже существует. Выбери другое имя.", null);

        if (EmailExists(email))
            return (false, "Аккаунт с таким email уже зарегистрирован.", null);

        var (hash, salt) = PasswordHasher.HashPassword(password);
        var role = email.Contains("admin", StringComparison.OrdinalIgnoreCase) ? "Admin" : "User";

        var user = new AppUser
        {
            Name = name,
            Email = email,
            Role = role,
            PasswordHash = hash,
            PasswordSalt = salt,
            CreatedAtUtc = DateTime.UtcNow
        };

        try
        {
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();
            return (true, string.Empty, user);
        }
        catch (DbUpdateException)
        {
            return (false, "Не удалось сохранить пользователя в базу данных. Проверь уникальность имени и email.", null);
        }
    }

    public (bool Success, string ErrorMessage, AppUser? User) Login(string email, string password)
    {
        var user = FindByEmail(email);
        if (user == null)
            return (false, "Пользователь с таким email не найден.", null);

        if (!PasswordHasher.VerifyPassword(password, user.PasswordHash, user.PasswordSalt))
            return (false, "Неверный пароль.", null);

        return (true, string.Empty, user);
    }
}
