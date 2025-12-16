using Microsoft.AspNetCore.Mvc;

namespace FinMate.web.Controllers
{
    public class LegalController : Controller
    {
        public IActionResult Privacy() => View();
        public IActionResult Terms() => View();
    }
}
