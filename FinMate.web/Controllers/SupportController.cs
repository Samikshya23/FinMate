using Microsoft.AspNetCore.Mvc;

namespace FinMate.web.Controllers
{
    public class SupportController : Controller
    {
        public IActionResult Contact() => View();
    }
}
