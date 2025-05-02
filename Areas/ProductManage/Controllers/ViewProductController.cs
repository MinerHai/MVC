using App.Models;
using App.Models.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MVC.Areas_Product_Controllers
{
    [Area("ProductManage")]
    public class ViewProductController : Controller
    {
        private readonly ILogger<ViewProductController> _logger;
        private readonly AppDbContext _context;
        private readonly CartServices _cartService;
        public ViewProductController(ILogger<ViewProductController> logger, AppDbContext context, CartServices cartService)
        {
            _context = context;
            _logger = logger;
            _cartService = cartService;
        }

        // GET: ViewPost
        [Route("/product/{categoryslug?}")]
        public IActionResult Index(string? categoryslug, [FromQuery(Name = "p")] int currentPage, int pageSize)
        {
            var categories = GetCategories();
            ViewBag.categories = categories;
            ViewBag.categorySlug = categoryslug;
            if (categoryslug != null)
            {
                if (!_context.CategoryProducts.Any(p => p.Slug == categoryslug))
                {
                    return NotFound("Không tồn tại danh mục này!!");
                }
            }
            var products = GetProductsWithCategorySlug(categoryslug);

            // PHAN TRANG 

            int totalProducts = products.Count();
            if (pageSize <= 0) pageSize = 16;
            int countPages = (int)Math.Ceiling((double)totalProducts / pageSize);
            if (currentPage > countPages) currentPage = countPages;
            if (currentPage < 1) currentPage = 1;

            var pagingModel = new PagingModel()
            {
                countpages = countPages,
                currentpage = currentPage,
                generateUrl = (pageNumber) => Url.Action("Index", new { p = pageNumber, pageSize = pageSize })
            };
            var productInPage = products.Skip((currentPage - 1) * pageSize)
                                  .Take(pageSize)
                                  .ToList();
            ViewBag.pagingModel = pagingModel;
            ViewBag.totalPosts = totalProducts;
            return View(productInPage);
        }

        [Route("/product/{productslug}.html")]
        public async Task<IActionResult> DetailsAsync(string? productSlug)
        {
            var product = _context.Products
                                    .Include(p => p.Author)
                                    .Include(p => p.Photos)
                                    .Include(p => p.ProductCategoryProducts)
                                    .ThenInclude(pc => pc.CategoryProducts)
                                    .FirstOrDefault(p => p.Slug == productSlug);
            var relateProducts = GetProductsWithCategorySlug(product.ProductCategoryProducts.FirstOrDefault().CategoryProducts.Slug)
    .OrderBy(x => Guid.NewGuid()) // shuffle ngẫu nhiên
    .Take(4)
    .ToList();
            ViewData["relateProducts"] = relateProducts;
            return View(product);
        }

        private List<App.Models.Product.CategoryProduct> GetCategories()
        {
            var products = _context.CategoryProducts.AsEnumerable().ToList();
            return products;
        }

        List<App.Models.Product.ProductModel> GetProductsWithCategorySlug(string? categorySlug)
        {
            var productQuery = _context.Products
                                    .Include(p => p.Author)
                                    .Include(p => p.Photos)
                                    .Include(p => p.ProductCategoryProducts)
                                    .ThenInclude(pc => pc.CategoryProducts)
                                    .AsQueryable();

            if (!string.IsNullOrEmpty(categorySlug))
            {
                productQuery = _context.Products
                                .Include(p => p.Author)
                                .Include(p => p.Photos)
                                .Include(p => p.ProductCategoryProducts)
                                .ThenInclude(pc => pc.CategoryProducts)
                               .Where(p => p.ProductCategoryProducts.Any(pc => pc.CategoryProducts.Slug == categorySlug))
                               .AsQueryable();
            }

            return productQuery.ToList();
        }

        [HttpGet("addcart/{productid:int}")]
        public IActionResult AddToCart(int productid, [FromQuery] int quantity = 1)
        {
            var product = _context.Products.Include(p => p.Photos).FirstOrDefault(p => p.ProductId == productid);
            if (product == null)
                return NotFound("Không có sản phẩm");

            var cart = _cartService.GetCartItems();
            var cartitem = cart.Find(p => p.product.ProductId == productid);

            if (cartitem != null)
            {
                cartitem.quantity += quantity;
            }
            else
            {
                cart.Add(new CartItem() { quantity = quantity, product = product });
            }

            _cartService.SaveCartSession(cart);

            // ✅ Trả lại JSON hoặc chỉ show thông báo nhẹ, không redirect
            return Json(new { success = true, message = $"Đã thêm {quantity} sản phẩm vào giỏ hàng" });
        }
        // Hiện thị giỏ hàng
        [Route("/cart", Name = "cart")]
        [Authorize]
        public IActionResult Cart()
        {
            return View(_cartService.GetCartItems());
        }

        [Route("/updatecart", Name = "updatecart")]
        [HttpPost]
        public IActionResult UpdateCart([FromForm] int productid, [FromForm] int quantity)
        {
            // Cập nhật Cart thay đổi số lượng quantity ...
            var cart = _cartService.GetCartItems();
            var cartitem = cart.Find(p => p.product.ProductId == productid);
            if (cartitem != null)
            {
                // Đã tồn tại, tăng thêm 1
                cartitem.quantity = quantity;
            }
            _cartService.SaveCartSession(cart);
            // Trả về mã thành công (không có nội dung gì - chỉ để Ajax gọi)
            return Ok(new { success = true });
        }

        [Route("/removecart/{productid:int}", Name = "removecart")]
        public IActionResult RemoveCart([FromRoute] int productid)
        {
            var cart = _cartService.GetCartItems();
            var cartitem = cart.Find(p => p.product.ProductId == productid);
            if (cartitem != null)
            {
                // Đã tồn tại, tăng thêm 1
                cart.Remove(cartitem);
            }

            _cartService.SaveCartSession(cart);
            return RedirectToAction(nameof(Cart));
        }
        [HttpGet]
        [Route("productinfo/{id:int}", Name = "productinfo")]
        public IActionResult GetProductInfo(int id)
        {
            _logger.LogInformation($"GetProductInfo called with id: {id}");
            var product = _context.Products
                                    .Include(p => p.Author)
                                    .Include(p => p.Photos)
                                    .Include(p => p.ProductCategoryProducts)
                                    .ThenInclude(pc => pc.CategoryProducts)
                                    .FirstOrDefault(p => p.ProductId == id);
            if (product == null)
            {
                _logger.LogWarning($"Product with id {id} not found");
                return NotFound();
            }
            return PartialView("_ProductInfoPartial", product);
        }
        [Route("productFilter")]
        public IActionResult Filter(string? categoryslug, string? price, string? sort)
        {

            var products = GetProductsWithCategorySlug(categoryslug).AsQueryable(); ;
            // Nếu có lọc theo price
            if (!string.IsNullOrEmpty(price))
            {
                var parts = price.Split('-');
                if (parts.Length == 2 &&
                    decimal.TryParse(parts[0], out decimal min) &&
                    decimal.TryParse(parts[1], out decimal max))
                {
                    products = products.Where(p => p.Price >= min && p.Price <= max);
                }
            }
            // Nếu có lọc theo sort
            if (!string.IsNullOrEmpty(sort))
            {
                if (sort == "lastest")
                {
                    products = products.OrderByDescending(p => p.DateUpdated);
                }
                else if (sort == "oldest")
                {
                    products = products.OrderBy(p => p.Price);
                }
            }

            var result = products.ToList();
            return PartialView("_ProductGridPartial", result);
        }
        public IActionResult ShowToast(string message)
        {
            return PartialView("_Toast", message);
        }

    }
}
