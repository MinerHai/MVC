using App.Data;
using App.Models;
using Bogus;
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
            StatusMessage = "Category seed data added successfully";
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
            if (await _userManager.FindByEmailAsync("admin@example.com") == null)
            {
                var useradmin = new AppUser()
                {
                    UserName = "admin",
                    Email = "admin@example.com",
                    EmailConfirmed = true
                };
                await _userManager.CreateAsync(useradmin, "admin");
                await _userManager.AddToRoleAsync(useradmin, RoleName.Administrator);

            }
            SeedPostCategory();

            StatusMessage = "Seed data added successfully";
            return RedirectToAction(nameof(Index));
        }

        private void SeedPostCategory()
        {

            _dbcontext.Categories.RemoveRange(_dbcontext.Categories.Where(c => c.Content.Contains("[fakeData]")));
            _dbcontext.Posts.RemoveRange(_dbcontext.Posts.Where(p => p.Content.Contains("[fakeData]")));

            var fakerCategory = new Faker<Category>();
            int cm = 1;
            fakerCategory.RuleFor(c => c.Title, fk => $"CM{cm++}" + fk.Lorem.Sentence(1, 2).Trim('.'));
            fakerCategory.RuleFor(c => c.Content, fk => fk.Lorem.Sentence(3, 5) + "[fakeData]");
            fakerCategory.RuleFor(c => c.Slug, fk => fk.Lorem.Slug());

            var cate1 = fakerCategory.Generate();
            var cate11 = fakerCategory.Generate();
            var cate12 = fakerCategory.Generate();
            var cate2 = fakerCategory.Generate();
            var cate21 = fakerCategory.Generate();
            var cate22 = fakerCategory.Generate();

            cate11.ParentCategory = cate1;
            cate12.ParentCategory = cate1;
            cate21.ParentCategory = cate2;
            cate22.ParentCategory = cate2;

            var categories = new Category[] { cate1, cate11, cate12, cate2, cate21, cate22 };
            _dbcontext.Categories.AddRange(categories);


            var rCateIndex = new Random();
            int bv = 1;
            var user = _userManager.GetUserAsync(this.User).Result;
            var fakerPost = new Faker<Post>();
            fakerPost.RuleFor(p => p.AuthorId, fk => user.Id);
            fakerPost.RuleFor(p => p.Content, f => f.Lorem.Paragraphs(7) + "[fakeData]");
            fakerPost.RuleFor(p => p.DateCreated, f => f.Date.Between(DateTime.Now.AddMonths(-6), DateTime.Now));
            fakerPost.RuleFor(p => p.Description, f => f.Lorem.Sentence(3));
            fakerPost.RuleFor(p => p.Published, f => true);
            fakerPost.RuleFor(p => p.Slug, f => f.Lorem.Slug());
            fakerPost.RuleFor(p => p.Title, f => $"Bài {bv++} " + f.Lorem.Sentence(3, 4).Trim('.'));

            List<Post> posts = new List<Post>();
            List<PostCategory> postCategories = new List<PostCategory>();

            for (int i = 0; i < 40; i++)
            {
                var post = fakerPost.Generate();
                post.DateUpdated = post.DateCreated;
                posts.Add(post);
                postCategories.Add(new PostCategory()
                {
                    Post = post,
                    Category = categories[rCateIndex.Next(5)]
                }
                    );
            }
            _dbcontext.AddRange(posts);
            _dbcontext.AddRange(postCategories);
            _dbcontext.SaveChanges();
        }
    }
}
