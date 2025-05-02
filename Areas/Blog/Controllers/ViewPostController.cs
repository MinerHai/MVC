using App.Models;
using App.Models.Blog; // Thêm using cho CommentViewModel
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
namespace MVC.Areas_Blogs_Controllers
{
    [Area("Blog")]
    public class ViewPostController : Controller
    {
        private readonly ILogger<ViewPostController> _logger;
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public ViewPostController(ILogger<ViewPostController> logger, AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }

        // GET: ViewPost
        [Route("/post/{categoryslug?}")]
        public IActionResult Index(string? categoryslug, [FromQuery(Name = "p")] int currentPage, int pageSize)
        {
            var categories = GetCategories();
            ViewBag.categories = categories;
            ViewBag.categorySlug = categoryslug;

            var postCounts = _context.PostCategories
                            .Include(pc => pc.Post) // Đảm bảo tải bài viết
                            .Where(pc => pc.Post.Published)
                            .GroupBy(pc => pc.CategoryID)
                            .ToDictionary(g => g.Key, g => g.Count());

            ViewBag.PostCounts = postCounts;

            var recentPosts = _context.Posts
                .Where(p => p.Published)
                .OrderByDescending(p => p.DateUpdated)
                .ThenByDescending(p => p.DateCreated) // Nếu ngày cập nhật giống nhau, sắp xếp theo ngày tạo
                .Take(3)
                .ToList();
            ViewBag.RecentPosts = recentPosts;

            if (categoryslug != null)
            {
                if (!_context.Categories.Any(p => p.Slug == categoryslug))
                {
                    return NotFound("Không tồn tại danh mục này!!");
                }
            }
            var posts = GetPostsWithCategorySlug(categoryslug);

            // PHAN TRANG 

            int totalPosts = posts.Count();
            if (pageSize <= 0) pageSize = 10;
            int countPages = (int)Math.Ceiling((double)totalPosts / pageSize);
            if (currentPage > countPages) currentPage = countPages;
            if (currentPage < 1) currentPage = 1;

            var pagingModel = new PagingModel()
            {
                countpages = countPages,
                currentpage = currentPage,
                generateUrl = (pageNumber) => Url.Action("Index", new { p = pageNumber, pageSize = pageSize })
            };
            var postInPage = posts.Skip((currentPage - 1) * pageSize)
                                  .Take(pageSize)
                                  .ToList();
            ViewBag.pagingModel = pagingModel;
            ViewBag.totalPosts = totalPosts;
            return View(postInPage);
        }

        [Route("/post/{postslug}.html")]
        public async Task<IActionResult> DetailAsync(string? postslug)
        {
            var categories = GetCategories();
            ViewBag.categories = categories;

            var postCounts = _context.PostCategories
                            .Include(pc => pc.Post) // Đảm bảo tải bài viết
                            .Where(pc => pc.Post.Published)
                            .GroupBy(pc => pc.CategoryID)
                            .ToDictionary(g => g.Key, g => g.Count());

            ViewBag.PostCounts = postCounts;

            var recentPosts = _context.Posts
                .Where(p => p.Published)
                .OrderByDescending(p => p.DateUpdated)
                .ThenByDescending(p => p.DateCreated) // Nếu ngày cập nhật giống nhau, sắp xếp theo ngày tạo
                .Take(3)
                .ToList();
            ViewBag.RecentPosts = recentPosts;

            var post = await _context.Posts
                                     .Include(p => p.Author)
                                     .Include(p => p.PostCategories)
                                     .Include(p => p.Comments)
                                        .ThenInclude(c => c.Author) // Include Author for each comment
                                     .FirstOrDefaultAsync(p => p.Slug == postslug);

            if (post == null)
            {
                return NotFound("Không tìm thấy bài viết");
            }
            return View(post);
        }

        List<Category> GetCategories()
        {
            var categories = _context.Categories.AsEnumerable().ToList();
            return categories;
        }

        List<Post> GetPostsWithCategorySlug(string? categorySlug)
        {
            var postsQuery = _context.Posts
                                    .Include(p => p.Author)
                                    .Include(p => p.PostCategories)
                                    .ThenInclude(pc => pc.Category)
                                    .AsQueryable();

            if (!string.IsNullOrEmpty(categorySlug))
            {
                postsQuery = _context.Posts
                               .Include(p => p.Author)
                               .Include(p => p.PostCategories)
                               .ThenInclude(pc => pc.Category)
                               .Where(p => p.PostCategories.Any(pc => pc.Category.Slug == categorySlug))
                               .AsQueryable();
            }

            return postsQuery.OrderByDescending(p => p.DateUpdated).ToList();
        }


        [HttpPost]
        [Authorize] // Chỉ người dùng đã đăng nhập mới được comment
        // Sử dụng CommentViewModel và Bind attribute
        public async Task<IActionResult> AddComment([Bind("PostId, Content")] CommentViewModel commentViewModel)
        {
            // Kiểm tra ModelState hợp lệ dựa trên các DataAnnotations trong CommentViewModel
            if (!ModelState.IsValid)
            {
                // Nếu không hợp lệ, tải lại dữ liệu bài viết và hiển thị lại trang Detail với lỗi validation
                var postWithError = await _context.Posts
                                         .Include(p => p.Author)
                                         .Include(p => p.PostCategories)
                                         .Include(p => p.Comments)
                                            .ThenInclude(c => c.Author)
                                         .FirstOrDefaultAsync(p => p.PostId == commentViewModel.PostId);

                if (postWithError == null)
                {
                    return NotFound("Không tìm thấy bài viết");
                }
                // Truyền lại commentViewModel để hiển thị lỗi validation trên form
                ViewData["CommentViewModel"] = commentViewModel; // Có thể cần truyền lại ViewModel vào View
                return View("Detail", postWithError); // Trả về View Detail với model Post và lỗi
            }

            // Nếu ModelState hợp lệ, tiếp tục xử lý
            var post = await _context.Posts.Include(p => p.Comments).FirstOrDefaultAsync(p => p.PostId == commentViewModel.PostId);
            if (post == null)
            {
                return NotFound("Không tìm thấy bài viết");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge(); // Hoặc xử lý lỗi khác nếu user không tìm thấy dù đã authorize
            }

            var comment = new PostComment
            {
                PostId = commentViewModel.PostId, // Lấy từ ViewModel
                AuthorId = user.Id,
                Content = commentViewModel.Content, // Lấy từ ViewModel
                DateCreated = DateTime.Now
            };

            _context.PostComments.Add(comment);
            await _context.SaveChangesAsync();

            // TempData["StatusMessage"] = "Bình luận của bạn đã được gửi."; // Thông báo thành công (tùy chọn)

            return RedirectToAction("Detail", new { postslug = post.Slug });
        }
    } // Closing brace for ViewPostController class
} // Closing brace for namespace
