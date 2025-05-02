using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using App.Models;
using MVC.Models.Product;
using Microsoft.AspNetCore.Authorization;
using App.Data;
using App.Utilities;
using Microsoft.AspNetCore.Identity;
using App.Models.Product;
using System.ComponentModel.DataAnnotations;

namespace MVC.Areas_Product_Controllers
{
    [Area("ProductManage")]
    [Route("admin/product-manage/product/[action]/{id?}")]
    [Authorize(Roles = RoleName.Administrator)]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        [TempData]
        public string StatusMessage { get; set; }

        public ProductController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Product
        public async Task<IActionResult> Index([FromQuery(Name = "p")] int currentPage, int pageSize)
        {
            var products = _context.Products
                                    .Include(p => p.Author)
                                    .Include(p => p.Photos)
                                    .Include(p => p.ProductCategoryProducts)
                                    .ThenInclude(pc => pc.CategoryProducts);

            int totalProducts = await products.CountAsync();
            if (pageSize <= 0) pageSize = 10;
            int countPages = (int)Math.Ceiling((double)totalProducts / pageSize);
            if (currentPage > countPages) currentPage = countPages;
            if (currentPage < 1) currentPage = 1;

            var pagingModel = new PagingModel()
            {
                countpages = countPages,
                currentpage = currentPage,
                generateUrl = (pageNumber) => Url.Action("Index", new { p = pageNumber, pageSize = pageSize })
            };
            ViewBag.pagingModel = pagingModel;
            ViewBag.totalProducts = totalProducts;
            var productInPage = await products.Skip((currentPage - 1) * pageSize)
                                .Take(pageSize)
                                .Include(p => p.ProductCategoryProducts)
                                .ThenInclude(pc => pc.CategoryProducts).ToListAsync();
            return View(productInPage);
        }

        // GET: Product/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productModel = await _context.Products
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (productModel == null)
            {
                return NotFound();
            }

            return View(productModel);
        }

        // GET: Product/Create
        public async Task<IActionResult> CreateAsync()
        {
            var categoryProducts = await _context.CategoryProducts.ToListAsync();

            ViewData["categoryProducts"] = new MultiSelectList(categoryProducts, "Id", "Title");
            return View();
        }

        // POST: Product/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Slug,Content,Published,AuthorId,Price,Discount,Quantity,Type,Color,Weight,CategoryProductId")] CreateProductModel productModel)
        {
            var categories = await _context.CategoryProducts.ToListAsync();
            ViewData["categoryProducts"] = new MultiSelectList(categories, "Id", "Title");

            if (productModel.Slug == null)
            {
                productModel.Slug = AppUtilities.GenerateSlug(productModel.Title);
            }

            if (await _context.Products.AnyAsync(p => p.Slug == productModel.Slug))
            {
                ModelState.AddModelError("Slug", "Url này đã tồn tại!!!");
            }


            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(this.User);
                productModel.AuthorId = user.Id;
                if (productModel.CategoryProductId != null)
                {
                    foreach (var Cateid in productModel.CategoryProductId)
                    {
                        _context.ProductCategoryProducts.Add(new ProductCategoryProduct()
                        {
                            CategoryId = Cateid,
                            Product = productModel
                        });
                    }
                }
                productModel.DateCreated = DateTime.Now;
                productModel.DateUpdated = DateTime.Now;
                _context.Add(productModel);
                await _context.SaveChangesAsync();
                StatusMessage = "Sản phẩm đã được tạo thành công";
                return RedirectToAction(nameof(Index));
            }
            return View(productModel);
        }

        // GET: Product/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var categories = await _context.CategoryProducts.ToListAsync();
            ViewData["categoryProducts"] = new MultiSelectList(categories, "Id", "Title");

            var product = await _context.Products.Include(p => p.ProductCategoryProducts).FirstOrDefaultAsync(p => p.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }
            var user = await _userManager.GetUserAsync(this.User);
            product.AuthorId = user.Id;

            var productEdit = new CreateProductModel()
            {
                ProductId = product.ProductId,
                Title = product.Title,
                Description = product.Description,
                Slug = product.Slug,
                Content = product.Content,
                Published = product.Published,
                AuthorId = user.Id,
                Price = product.Price,
                Discount = product.Discount,
                Quantity = product.Quantity,
                Type = product.Type,
                Color = product.Color,
                Weight = product.Weight,
                CategoryProductId = product.ProductCategoryProducts.Select(pc => pc.CategoryId).ToArray(),
                DateUpdated = DateTime.Now
            };
            return View(productEdit);
        }

        // POST: Product/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,Title,Description,Slug,Content,Published,Price,Discount,Quantity,Type,Color,Weight,CategoryProductId")] CreateProductModel productModel)
        {
            if (id != productModel.ProductId)
            {
                return NotFound();
            }

            var categories = await _context.CategoryProducts.ToListAsync();
            ViewData["categoryProducts"] = new MultiSelectList(categories, "Id", "Title");
            if (productModel.Slug == null)
            {
                productModel.Slug = AppUtilities.GenerateSlug(productModel.Title);
            }

            if (await _context.Products.AnyAsync(p => p.Slug == productModel.Slug && p.ProductId != id))
            {
                ModelState.AddModelError("Slug", "Url này đã tồn tại!!!");
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(this.User);
                productModel.AuthorId = user.Id;
                var productUpdate = await _context.Products.Include(p => p.ProductCategoryProducts).FirstOrDefaultAsync(p => p.ProductId == id);

                if (productUpdate == null)
                {
                    return NotFound();
                }
                productUpdate.Title = productModel.Title;
                productUpdate.Description = productModel.Description;
                productUpdate.Slug = productModel.Slug;
                productUpdate.Author = productModel.Author ?? user;
                productUpdate.Content = productModel.Content;
                productUpdate.Published = productModel.Published;
                productUpdate.AuthorId = user.Id;
                productUpdate.Price = productModel.Price;
                productUpdate.Discount = productModel.Discount;
                productUpdate.Quantity = productModel.Quantity;
                productUpdate.Type = productModel.Type;
                productUpdate.Color = productModel.Color;
                productUpdate.Weight = productModel.Weight;

                if (productModel.CategoryProductId == null) productModel.CategoryProductId = new int[] { };

                var oldCateID = productUpdate.ProductCategoryProducts.Select(pc => pc.CategoryId).ToArray();
                var newCateId = productModel.CategoryProductId;

                var removeCategories = from productCate in productUpdate.ProductCategoryProducts
                                       where !newCateId.Contains(productCate.CategoryId)
                                       select productCate;
                _context.ProductCategoryProducts.RemoveRange(removeCategories);

                var addCateId = from cateId in newCateId
                                where !oldCateID.Contains(cateId)
                                select cateId;
                foreach (var cateid in addCateId)
                {
                    _context.ProductCategoryProducts.Add(new ProductCategoryProduct()
                    {
                        CategoryId = cateid,
                        ProductId = id
                    });
                }
                _context.Update(productUpdate);
                await _context.SaveChangesAsync();

                StatusMessage = "Bài viết đã được cập nhật thành công";
                return RedirectToAction(nameof(Index));
            }
            return View(productModel);
        }

        // GET: Product/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productModel = await _context.Products
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (productModel == null)
            {
                return NotFound();
            }

            return View(productModel);
        }

        // POST: Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var productModel = await _context.Products.FindAsync(id);
            if (productModel != null)
            {
                _context.Products.Remove(productModel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductModelExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }

        [HttpGet]
        public IActionResult UploadPhoto(int? id)
        {
            var product = _context.Products.Where(p => p.ProductId == id)
                                            .Include(p => p.Photos)
                                            .FirstOrDefault();
            if (product == null)
            {
                return NotFound("Không tồn tại sản phẩm!!");
            }
            ViewData["product"] = product;
            return View(new UploadOneFile());
        }
        [HttpPost]
        public async Task<IActionResult> UploadPhotoAsync(int? id, [Bind("FileUpLoad")] UploadOneFile f)
        {
            var product = _context.Products.Where(p => p.ProductId == id)
                                            .Include(p => p.Photos)
                                            .FirstOrDefault();
            if (product == null)
            {
                return NotFound("Không tồn tại sản phẩm!!");
            }
            ViewData["product"] = product;
            if (f?.FileUpLoad == null || f.FileUpLoad.Length == 0)
            {
                ModelState.AddModelError("FileUpLoad", "Vui lòng chọn một file để upload.");
                return View(f); // Hiển thị lại view với thông báo lỗi
            }

            if (f != null)
            {
                var file = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + Path.GetExtension(f.FileUpLoad.FileName);
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", "Posts");
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath); // Tạo thư mục nếu chưa tồn tại
                }

                var finalFile = Path.Combine(uploadPath, file);

                using (var fileStream = new FileStream(finalFile, FileMode.Create))
                {
                    await f.FileUpLoad.CopyToAsync(fileStream);
                }
                _context.Add(new ProductPhoto()
                {
                    ProductId = product.ProductId,
                    FileName = file
                });

                await _context.SaveChangesAsync();
            }
            return RedirectToAction("UploadPhoto", new { id = product.ProductId });
        }

        [HttpPost]
        public IActionResult ListPhotos(int? id)
        {
            var product = _context.Products.Where(p => p.ProductId == id).Include(p => p.Photos).FirstOrDefault();
            if (product == null)
            {
                return Json(
                    new
                    {
                        success = 0,
                        message = "Product not found"
                    }
                );
            }
            var listphotos = product.Photos.Select(photo => new
            {
                id = photo.Id,
                path = "/contents/Products/" + photo.FileName
            });

            return Json(new
            {
                success = 1,
                photos = listphotos
            });
        }
        [HttpPost]
        public IActionResult DeletePhoto(int? id)
        {
            var photo = _context.productPhotos.Where(p => p.Id == id).FirstOrDefault();
            if (photo != null)
            {
                _context.Remove(photo);
                _context.SaveChanges();

                var fileName = "Uploads/Products/" + photo.FileName;
                System.IO.File.Delete(fileName);
            }
            return Ok();
        }
        [HttpPost]
        public async Task<IActionResult> UploadPhotoAPI(int? id, [Bind("FileUpLoad")] UploadOneFile f)
        {
            var product = _context.Products.Where(p => p.ProductId == id)
                                            .Include(p => p.Photos)
                                            .FirstOrDefault();
            if (product == null)
            {
                return NotFound("Không tồn tại sản phẩm!!");
            }

            if (f?.FileUpLoad == null || f.FileUpLoad.Length == 0)
            {
                ModelState.AddModelError("FileUpLoad", "Vui lòng chọn một file để upload.");
                return View(f);
            }

            if (f != null)
            {
                var file = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + Path.GetExtension(f.FileUpLoad.FileName);
                var finalFile = Path.Combine("Uploads", "Products", file);

                using (var fileStream = new FileStream(finalFile, FileMode.Create))
                {
                    await f.FileUpLoad.CopyToAsync(fileStream);
                }
                _context.Add(new ProductPhoto()
                {
                    ProductId = product.ProductId,
                    FileName = file
                });

                await _context.SaveChangesAsync();
            }
            return Ok();
        }
    }
}
