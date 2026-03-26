using System.Text.Json;
using QazaqQuest.Models;

namespace QazaqQuest.Services;

public class UserStoreService
{
    private readonly string _filePath;
    private readonly object _sync = new();
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public UserStoreService(IWebHostEnvironment environment)
    {
        var dataDirectory = Path.Combine(environment.ContentRootPath, "Data");
        Directory.CreateDirectory(dataDirectory);
        _filePath = Path.Combine(dataDirectory, "users.json");
        EnsureStorageInitialized();
    }

    public bool UserNameExists(string name) =>
        GetUsers().Any(u => u.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase));

    public bool EmailExists(string email) =>
        GetUsers().Any(u => u.Email.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase));

    public AppUser? FindByEmail(string email) =>
        GetUsers().FirstOrDefault(u => u.Email.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase));

    public AppUser? FindByName(string name) =>
        GetUsers().FirstOrDefault(u => u.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase));

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
            PasswordSalt = salt
        };

        var users = GetUsers();
        users.Add(user);
        SaveUsers(users);

        return (true, string.Empty, user);
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

    private List<AppUser> GetUsers()
    {
        lock (_sync)
        {
            if (!File.Exists(_filePath))
                return new List<AppUser>();

            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<AppUser>>(json) ?? new List<AppUser>();
        }
    }

    private void SaveUsers(List<AppUser> users)
    {
        lock (_sync)
        {
            var json = JsonSerializer.Serialize(users.OrderBy(u => u.CreatedAtUtc).ToList(), _jsonOptions);
            File.WriteAllText(_filePath, json);
        }
    }

    private void EnsureStorageInitialized()
    {
        if (File.Exists(_filePath))
            return;

        File.WriteAllText(_filePath, "[]");
    }
}
