using Microsoft.AspNetCore.Mvc;
using MVC.Models; // Assuming ErrorViewModel is here
using App.Models; // For ContactModel
using System.Diagnostics;
using System; // For DateTime
using System.Threading.Tasks; // For Task
using Microsoft.EntityFrameworkCore; // For AppDbContext if not already referenced by MVC.Models

namespace MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context; // Add AppDbContext

        public HomeController(ILogger<HomeController> logger, AppDbContext context) // Inject AppDbContext
        {
            _logger = logger;
            _context = context; // Assign injected AppDbContext
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Database()
        {
            return View();
        }

        public IActionResult RenderToast(string message)
        {
            return PartialView("_Toast", message);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // GET: /Home/Contact
        public IActionResult Contact()
        {
            return View(new ContactModel()); // Pass a new model for the form
        }

        // POST: /Home/Contact
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact([Bind("Name,Email,Desc,Phone")] ContactModel contact)
        {
            if (ModelState.IsValid)
            {
                contact.DateSent = DateTime.Now;
                _context.Add(contact);
                await _context.SaveChangesAsync();
                // Optionally: Add a TempData message for success
                // TempData["StatusMessage"] = "Your message has been sent successfully!";
                return RedirectToAction(nameof(Index)); // Or a "Thank You" page
            }
            return View(contact); // Return to the view with validation errors
        }

        public IActionResult AboutUs()
        {
            return View();
        }
    }
}
