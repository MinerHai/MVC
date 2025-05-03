using Microsoft.AspNetCore.Mvc;
using App.Models.Orders;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using App.Models;
using Microsoft.AspNetCore.Authorization;

namespace MVC.Areas_Product_Controllers
{
    [Area("ProductManage")]
    public class OrderController : Controller
    {
        private readonly IVnPayService _vnPayService;
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly CartServices _cartService;
        [TempData]
        public string StatusMessage { get; set; }

        public OrderController(AppDbContext context, UserManager<AppUser> userManager, CartServices cartService, IVnPayService vnPayService)
        {
            _context = context;
            _userManager = userManager;
            _cartService = cartService;
            _vnPayService = vnPayService;
        }

        [HttpGet]
        public IActionResult Checkout()
        {
            var cartItems = _cartService.GetCartItems();
            if (cartItems == null || cartItems.Count == 0)
            {
                TempData["Message"] = "Giỏ hàng trống.";
                return RedirectToAction("Index", "Cart");
            }

            return View(new OrderModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(OrderModel model)
        {
            var cart = _cartService.GetCartItems();
            if (cart.Count == 0)
            {
                ModelState.AddModelError("", "Giỏ hàng trống.");
                return View(model);
            }

            // Lấy thông tin User đang đăng nhập
            var userId = _userManager.GetUserId(User);
            model.UserId = userId;
            model.OrderDate = DateTime.Now;
            model.Status = "Pending";
            model.IsPaid = false;

            // Tính tổng tiền
            model.TotalAmount = cart.Sum(item => item.quantity * item.product.Price);

            // Tạo danh sách OrderItem
            model.OrderItems = cart.Select(item => new OrderItem
            {
                ProductId = item.product.ProductId,
                Quantity = item.quantity,
                UnitPrice = item.product.Price
            }).ToList();

            // Lưu vào DB
            _context.Order.Add(model);
            await _context.SaveChangesAsync();
            if (model.PaymentMethod == "Online")
            {
                // string returnUrl = Url.Action("ConfirmPayment", "Orders", new { orderId = model.OrderId }, Request.Scheme);

                // Tạo link thanh toán MoMo (cần có class MoMoService)
                // string paymentUrl = MoMoService.CreatePaymentUrl(model, returnUrl);

                // return Redirect(paymentUrl); // chuyển hướng đến MoMo
            }
            StatusMessage = "Đặt hàng thành công!";
            // Xóa giỏ hàng
            _cartService.ClearCart();

            // Điều hướng tới trang cảm ơn
            return RedirectToAction("Index", "Home", new { area = "" });
        }

        public IActionResult Success()
        {
            return View();
        }
        public async Task<IActionResult> AdminIndex(string statusFilter, bool? isPaid)
        {
            var query = _context.Order
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .AsQueryable();

            if (!string.IsNullOrEmpty(statusFilter))
            {
                query = query.Where(o => o.Status == statusFilter);
            }

            if (isPaid.HasValue)
            {
                query = query.Where(o => o.IsPaid == isPaid.Value);
            }

            var orders = await query.ToListAsync();

            ViewBag.StatusFilter = statusFilter;
            ViewBag.IsPaid = isPaid;

            return View(orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var order = await _context.Order
                                      .Include(o => o.OrderItems)
                                      .ThenInclude(oi => oi.Product)
                                      .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            // Nếu chuyển sang "Completed", trừ kho
            if (status == "Completed" && order.Status != "Completed")
            {
                foreach (var item in order.OrderItems)
                {
                    var product = item.Product;
                    if (product != null)
                    {
                        product.Quantity -= item.Quantity;

                        // Kiểm tra nếu âm thì gán về 0
                        if (product.Quantity < 0)
                            product.Quantity = 0;
                    }
                }
            }

            order.Status = status;
            await _context.SaveChangesAsync();
            return Ok();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> TogglePaid(int id)
        {
            var order = await _context.Order.FindAsync(id);
            if (order == null) return NotFound();

            order.IsPaid = !order.IsPaid;
            await _context.SaveChangesAsync();
            return Ok();
        }
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound("Không có ID được cung cấp.");
            }

            var order = await _context.Order
                .Include(o => o.User) // Lấy thông tin người dùng
                .Include(o => o.OrderItems) // Lấy các mục trong đơn hàng
                    .ThenInclude(oi => oi.Product) // Lấy thông tin sản phẩm cho từng mục
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null)
            {
                return NotFound($"Không tìm thấy đơn hàng với ID {id}.");
            }

            return View(order);
        }

        [HttpGet]
        public IActionResult PaymentCallbackVnpay()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            return Json(response);
        }


    }

}

