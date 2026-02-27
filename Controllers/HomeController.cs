using Microsoft.AspNetCore.Mvc;

namespace QazaqQuest.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}