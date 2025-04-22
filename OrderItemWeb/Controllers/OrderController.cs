using Microsoft.AspNetCore.Mvc;

namespace OrderItemWeb.Controllers
{
    public class OrderController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult CreateEdit()
        {
            return View();
        }
    }
}
