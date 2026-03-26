using Microsoft.AspNetCore.Mvc;
<<<<<<< HEAD
using System.Text.RegularExpressions;

namespace QazaqQuest.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string name, string email, string password, string confirmPassword)
        {
            if (password != confirmPassword)
                return Content("Пароли не совпадают");

            var (isValid, errorMessage) = PasswordValidator.Validate(password);

            if (!isValid)
                return Content(errorMessage ?? "Пароль не соответствует требованиям");

            return Content("Регистрация успешна");
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            return Content("Вход выполнен");
        }
    }

    public static class PasswordValidator
    {
        public static (bool IsValid, string? ErrorMessage) Validate(string password)
        {
            if (password.Length < 8 || password.Length > 20)
                return (false, "Пароль должен содержать от 8 до 20 символов");

            if (password.Contains(" "))
                return (false, "Пароль не должен содержать пробелы");

            if (!Regex.IsMatch(password, "[A-Z]"))
                return (false, "Пароль должен содержать хотя бы одну заглавную букву");

            if (!Regex.IsMatch(password, "[a-z]"))
                return (false, "Пароль должен содержать хотя бы одну строчную букву");

            if (!Regex.IsMatch(password, "[0-9]"))
                return (false, "Пароль должен содержать хотя бы одну цифру");

            if (!Regex.IsMatch(password, @"[^a-zA-Z0-9]"))
                return (false, "Пароль должен содержать хотя бы один специальный символ");

            for (int i = 0; i < password.Length - 2; i++)
            {
                if (password[i] == password[i + 1] && password[i] == password[i + 2])
                    return (false, "Пароль не должен содержать три одинаковых символа подряд");
            }

            for (int i = 0; i < password.Length - 2; i++)
            {
                if (IsSequential(password[i], password[i + 1], password[i + 2]))
                    return (false, "Пароль не должен содержать последовательности из трёх символов подряд (например, abc, 123, XYZ)");
            }

            return (true, null);
        }

        private static bool IsSequential(char a, char b, char c)
        {
            if (char.IsLetter(a) && char.IsLetter(b) && char.IsLetter(c))
            {
                char upperA = char.ToUpper(a);
                char upperB = char.ToUpper(b);
                char upperC = char.ToUpper(c);
                
                if (upperB == upperA + 1 && upperC == upperA + 2)
                    return true;
                
                if (upperB == upperA - 1 && upperC == upperA - 2)
                    return true;
            }
            
            if (char.IsDigit(a) && char.IsDigit(b) && char.IsDigit(c))
            {
                if (b == a + 1 && c == a + 2)
                    return true;
                if (b == a - 1 && c == a - 2)
                    return true;
            }
            
            return false;
        }
    }
}


//Qwerty_2845
//Qwerty_123
//Abcd_2845
=======
using QazaqQuest.Services;
using System.Text.RegularExpressions;

namespace QazaqQuest.Controllers;

public class AuthController : Controller
{
    private readonly UserStoreService _userStoreService;

    public AuthController(UserStoreService userStoreService)
    {
        _userStoreService = userStoreService;
    }

    public IActionResult Login()
    {
        if (IsAuthenticated())
            return RedirectToAction("Index", "Home");

        return View();
    }

    public IActionResult Register()
    {
        if (IsAuthenticated())
            return RedirectToAction("Index", "Home");

        return View();
    }

    [HttpPost]
    public IActionResult Register(string name, string email, string password, string confirmPassword)
    {
        name = name?.Trim() ?? string.Empty;
        email = email?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmPassword))
        {
            TempData["Error"] = "Заполни все поля регистрации.";
            return RedirectToAction(nameof(Register));
        }

        if (name.Length < 3 || name.Length > 30)
        {
            TempData["Error"] = "Имя должно содержать от 3 до 30 символов.";
            return RedirectToAction(nameof(Register));
        }

        if (!Regex.IsMatch(name, @"^[a-zA-Zа-яА-ЯәіңғүұқөһӘІҢҒҮҰҚӨҺ0-9_\- ]+$"))
        {
            TempData["Error"] = "Имя может содержать только буквы, цифры, пробел, дефис и нижнее подчёркивание.";
            return RedirectToAction(nameof(Register));
        }

        if (password != confirmPassword)
        {
            TempData["Error"] = "Пароли не совпадают";
            return RedirectToAction(nameof(Register));
        }

        var (isValid, errorMessage) = PasswordValidator.Validate(password);
        if (!isValid)
        {
            TempData["Error"] = errorMessage ?? "Пароль не соответствует требованиям";
            return RedirectToAction(nameof(Register));
        }

        var result = _userStoreService.Register(name, email, password);
        if (!result.Success || result.User == null)
        {
            TempData["Error"] = result.ErrorMessage;
            return RedirectToAction(nameof(Register));
        }

        SignIn(result.User.Name, result.User.Email, result.User.Role);
        TempData["Success"] = "Регистрация успешна. Аккаунт создан и сохранён.";
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public IActionResult Login(string email, string password)
    {
        email = email?.Trim() ?? string.Empty;
        password ??= string.Empty;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            TempData["Error"] = "Введи email и пароль.";
            return RedirectToAction(nameof(Login));
        }

        var result = _userStoreService.Login(email, password);
        if (!result.Success || result.User == null)
        {
            TempData["Error"] = result.ErrorMessage;
            return RedirectToAction(nameof(Login));
        }

        SignIn(result.User.Name, result.User.Email, result.User.Role);
        TempData["Success"] = $"Вход выполнен. Добро пожаловать, {result.User.Name}!";
        return RedirectToAction("Index", "Home");
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        TempData["Success"] = "Вы вышли из аккаунта";
        return RedirectToAction("Index", "Home");
    }

    private void SignIn(string name, string email, string role)
    {
        HttpContext.Session.SetString("UserName", name);
        HttpContext.Session.SetString("UserEmail", email);
        HttpContext.Session.SetString("UserRole", role);
    }

    private bool IsAuthenticated() =>
        !string.IsNullOrWhiteSpace(HttpContext.Session.GetString("UserEmail"));
}

public static class PasswordValidator
{
    public static (bool IsValid, string? ErrorMessage) Validate(string password)
    {
        if (password.Length < 8 || password.Length > 20)
            return (false, "Пароль должен содержать от 8 до 20 символов");

        if (password.Contains(" "))
            return (false, "Пароль не должен содержать пробелы");

        if (!Regex.IsMatch(password, "[A-Z]"))
            return (false, "Пароль должен содержать хотя бы одну заглавную букву");

        if (!Regex.IsMatch(password, "[a-z]"))
            return (false, "Пароль должен содержать хотя бы одну строчную букву");

        if (!Regex.IsMatch(password, "[0-9]"))
            return (false, "Пароль должен содержать хотя бы одну цифру");

        if (!Regex.IsMatch(password, @"[^a-zA-Z0-9]"))
            return (false, "Пароль должен содержать хотя бы один специальный символ");

        for (int i = 0; i < password.Length - 2; i++)
        {
            if (password[i] == password[i + 1] && password[i] == password[i + 2])
                return (false, "Пароль не должен содержать три одинаковых символа подряд");
        }

        for (int i = 0; i < password.Length - 2; i++)
        {
            if (IsSequential(password[i], password[i + 1], password[i + 2]))
                return (false, "Пароль не должен содержать последовательности из трёх символов подряд (например, abc, 123, XYZ)");
        }

        return (true, null);
    }

    private static bool IsSequential(char a, char b, char c)
    {
        if (char.IsLetter(a) && char.IsLetter(b) && char.IsLetter(c))
        {
            char upperA = char.ToUpper(a);
            char upperB = char.ToUpper(b);
            char upperC = char.ToUpper(c);

            if (upperB == upperA + 1 && upperC == upperA + 2)
                return true;

            if (upperB == upperA - 1 && upperC == upperA - 2)
                return true;
        }

        if (char.IsDigit(a) && char.IsDigit(b) && char.IsDigit(c))
        {
            if (b == a + 1 && c == a + 2)
                return true;
            if (b == a - 1 && c == a - 2)
                return true;
        }

        return false;
    }
}
>>>>>>> d34208a (feature 2.0)
