using System.Diagnostics;
using KTX.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KTX.Controllers
{
    [Authorize] // Yêu c?u ðãng nh?p
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            // L?y thông tin user
            var username = User.Identity?.Name;
            var fullName = User.Claims.FirstOrDefault(c => c.Type == "FullName")?.Value;

            ViewBag.Username = username;
            ViewBag.FullName = fullName;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
