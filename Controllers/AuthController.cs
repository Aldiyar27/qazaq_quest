using Microsoft.AspNetCore.Mvc;
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