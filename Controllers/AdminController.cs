using Microsoft.AspNetCore.Mvc;

namespace MVC.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Blog()
        {
            return View("Blog/Index");
        }

        public IActionResult Product()
        {
            return View("Product/Index");
        }
    }
}