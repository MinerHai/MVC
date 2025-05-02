using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using App.Models;
using Microsoft.AspNetCore.Authorization;
using App.Data;
using App.Models.Blog;
using Microsoft.AspNetCore.Identity;
using App.Utilities;

namespace MVC.Areas_Blogs_Controllers
{

    [Area("Blog")]
    [Route("admin/blog/post/[action]/{id?}")]
    [Authorize(Roles = RoleName.Administrator + ", " + RoleName.Editor)]
    public class PostController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        [TempData]
        public string StatusMessage { get; set; }
        public PostController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Post
        public async Task<IActionResult> Index([FromQuery(Name = "p")] int currentPage, int pageSize)
        {
            var posts = _context.Posts
                        .Include(p => p.Author)
                        .OrderByDescending(p => p.DateUpdated);


            int totalPosts = await posts.CountAsync();
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
            ViewBag.pagingModel = pagingModel;
            ViewBag.totalPosts = totalPosts;
            var postInPage = await posts.Skip((currentPage - 1) * pageSize)
                                .Take(pageSize)
                                .Include(p => p.PostCategories)
                                .ThenInclude(pc => pc.Category).ToListAsync();
            return View(postInPage);
        }

        // GET: Post/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.PostId == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // GET: Post/Create
        public async Task<IActionResult> CreateAsync()
        {
            var categories = await _context.Categories.ToListAsync();

            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");
            return View();
        }

        // POST: Post/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Slug,Content,Published,CategoryId")] CreatePostModel post)
        {
            var categories = await _context.Categories.ToListAsync();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");

            if (string.IsNullOrEmpty(post.Slug))
            {
                post.Slug = AppUtilities.GenerateSlug(post.Title);
            }

            if (await _context.Posts.AnyAsync(p => p.Slug == post.Slug))
            {
                ModelState.AddModelError("Slug", "Url này đã tồn tại!!!");
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(this.User);

                // Tạo đối tượng Post từ CreatePostModel
                var newPost = new Post
                {
                    Title = post.Title,
                    Description = post.Description,
                    Slug = post.Slug,
                    Content = post.Content,
                    Published = post.Published,
                    AuthorId = user.Id,
                    DateCreated = DateTime.Now,
                    DateUpdated = DateTime.Now
                };

                _context.Add(newPost);
                await _context.SaveChangesAsync();

                // Lưu danh mục bài viết nếu có
                if (post.CategoryId != null)
                {
                    foreach (var Cateid in post.CategoryId)
                    {
                        _context.PostCategories.Add(new PostCategory()
                        {
                            CategoryID = Cateid,
                            Post = newPost
                        });
                    }
                    await _context.SaveChangesAsync();
                }

                StatusMessage = "Bài viết đã được tạo thành công";
                return RedirectToAction(nameof(Index));
            }

            return View(post);
        }

        // GET: Post/Edit/5
        public async Task<IActionResult> EditAsync(int? id)
        {
            var categories = await _context.Categories.ToListAsync();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts.Include(p => p.PostCategories).FirstOrDefaultAsync(p => p.PostId == id);
            if (post == null)
            {
                return NotFound();
            }
            var postedit = new CreatePostModel()
            {
                PostId = post.PostId,
                Title = post.Title,
                Description = post.Description,
                Slug = post.Slug,
                Content = post.Content,
                Published = post.Published,
                AuthorId = post.AuthorId,
                DateCreated = post.DateCreated,
                DateUpdated = post.DateUpdated,
                FileName = post.FileName,
                CategoryId = post.PostCategories.Select(pc => pc.CategoryID).ToArray()
            };
            return View(postedit);
        }

        // POST: Post/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PostId,Title,Description,Slug,Content,Published,CategoryId, FileName")] CreatePostModel post)
        {
            if (id != post.PostId)
            {
                return NotFound();
            }
            var categories = await _context.Categories.ToListAsync();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");
            if (post.Slug == null)
            {
                post.Slug = AppUtilities.GenerateSlug(post.Title);
            }

            if (await _context.Posts.AnyAsync(p => p.Slug == post.Slug && p.PostId != id))
            {
                ModelState.AddModelError("Slug", "Url này đã tồn tại!!!");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var postUpdate = await _context.Posts.Include(p => p.PostCategories).FirstOrDefaultAsync(p => p.PostId == id);

                    if (postUpdate == null)
                    {
                        return NotFound();
                    }
                    postUpdate.Title = post.Title;
                    postUpdate.Description = post.Description;
                    postUpdate.Slug = post.Slug;
                    postUpdate.Content = post.Content;
                    postUpdate.Published = post.Published;
                    postUpdate.DateUpdated = DateTime.Now;
                    if (post.CategoryId == null) post.CategoryId = new int[] { };

                    var oldCateID = postUpdate.PostCategories.Select(pc => pc.CategoryID).ToArray();
                    var newCateId = post.CategoryId;

                    var removeCategories = from postCate in postUpdate.PostCategories
                                           where !newCateId.Contains(postCate.CategoryID)
                                           select postCate;
                    _context.PostCategories.RemoveRange(removeCategories);

                    var addCateId = from cateId in newCateId
                                    where !oldCateID.Contains(cateId)
                                    select cateId;
                    foreach (var cateid in addCateId)
                    {
                        _context.PostCategories.Add(new PostCategory()
                        {
                            CategoryID = cateid,
                            PostID = id
                        });
                    }
                    _context.Update(postUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.PostId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                StatusMessage = "Bài viết đã được cập nhật thành công";
                return RedirectToAction(nameof(Index));
            }

            return View(post);
        }

        // GET: Post/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.PostId == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // POST: Post/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post != null)
            {
                _context.Posts.Remove(post);
            }
            else
            {
                return NotFound();
            }
            StatusMessage = "Post has been deleted" + post.Title;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.PostId == id);
        }

        [HttpGet]
        public IActionResult UploadPhoto(int? id)
        {
            var post = _context.Posts.Where(p => p.PostId == id)
                                            .FirstOrDefault();
            if (post == null)
            {
                return NotFound("Không tồn tại sản phẩm!!");
            }
            ViewData["post"] = post;
            return View(new UploadOneFile());
        }
        [HttpPost]
        public async Task<IActionResult> UploadPhotoAsync(int? id, [Bind("FileUpLoad")] UploadOneFile f)
        {
            var post = _context.Posts.Where(p => p.PostId == id).FirstOrDefault();
            if (post == null)
            {
                return NotFound("Không tồn tại sản phẩm!!");
            }
            ViewData["post"] = post;

            if (f?.FileUpLoad == null || f.FileUpLoad.Length == 0)
            {
                ModelState.AddModelError("FileUpLoad", "Vui lòng chọn một file để upload.");
                return View(f); // Hiển thị lại view với thông báo lỗi
            }

            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "Posts");
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath); // Tạo thư mục nếu chưa tồn tại
            }

            // Nếu bài viết đã có ảnh, xóa ảnh cũ
            if (!string.IsNullOrEmpty(post.FileName))
            {
                var oldFilePath = Path.Combine(uploadPath, post.FileName);
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
            }

            // Lưu ảnh mới
            var fileName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + Path.GetExtension(f.FileUpLoad.FileName);
            var newFilePath = Path.Combine(uploadPath, fileName);

            using (var fileStream = new FileStream(newFilePath, FileMode.Create))
            {
                await f.FileUpLoad.CopyToAsync(fileStream);
            }

            // Cập nhật tên file trong database
            post.FileName = fileName;
            await _context.SaveChangesAsync();

            return RedirectToAction("Edit", new { id = post.PostId });
        }

    }


}
