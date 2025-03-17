using App.Data;
using App.Models;
using Microsoft.AspNetCore.Components.Forms.Mapping;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace App.Areas.Database.Controllers
{
    [Area("Database")]
    [Route("/database-manage/[action]")]
    public class DbManage : Controller
    {
        private readonly AppDbContext _dbcontext;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        [TempData]

        public string StatusMessage { get; set; }
        public DbManage(AppDbContext dbcontext, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _dbcontext = dbcontext;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        
        // GET: DbManage

        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult DeleteDb()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Migrate()
        {
            await _dbcontext.Database.MigrateAsync();
            StatusMessage = "Database migrated successfully";
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> DeleteDbAsync()
        {
            var success = await _dbcontext.Database.EnsureDeletedAsync();
            if (success)
            {
                StatusMessage = "Database deleted successfully";
            }
            else
            {
                StatusMessage = "Error while deleting database";
            }
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> SeedData()
        {
            var roles = typeof(RoleName).GetFields().ToList();
            foreach (var r in roles)
            {
                var roleName = r.GetValue(null).ToString();
                var rfound = await _roleManager.RoleExistsAsync(roleName);
                if (!rfound)
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
            if (await _userManager.FindByEmailAsync("admin") == null)
            {
                var useradmin = new AppUser()
                {
                    UserName = "admin",
                    Email = "admin@example.com",
                    EmailConfirmed = true
                };
                await _userManager.CreateAsync(useradmin, "admin");
                await _userManager.AddToRoleAsync(useradmin, RoleName.Administrator);
                StatusMessage = "Seed data added successfully";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
