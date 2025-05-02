using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using App.Models;
using App.Models.Product;
using Microsoft.AspNetCore.Authorization;
using App.Data;

namespace MVC.Areas_Product_Controllers
{
    [Area("ProductManage")]
    [Route("admin/product-manage/product-category/[action]/{id?}")]
    [Authorize(Roles = RoleName.Administrator)]

    public class CategoryProductController : Controller
    {
        private readonly AppDbContext _context;

        public CategoryProductController(AppDbContext context)
        {
            _context = context;
        }

        // GET: CategoryProduct
        public async Task<IActionResult> Index()
        {
            return View(await _context.CategoryProducts.ToListAsync());
        }

        // GET: CategoryProduct/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoryProduct = await _context.CategoryProducts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (categoryProduct == null)
            {
                return NotFound();
            }

            return View(categoryProduct);
        }

        // GET: CategoryProduct/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: CategoryProduct/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Content,Slug")] CategoryProduct categoryProduct)
        {
            if (ModelState.IsValid)
            {
                _context.Add(categoryProduct);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(categoryProduct);
        }

        // GET: CategoryProduct/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoryProduct = await _context.CategoryProducts.FindAsync(id);
            if (categoryProduct == null)
            {
                return NotFound();
            }
            return View(categoryProduct);
        }

        // POST: CategoryProduct/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content,Slug")] CategoryProduct categoryProduct)
        {
            if (id != categoryProduct.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(categoryProduct);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryProductExists(categoryProduct.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(categoryProduct);
        }

        // GET: CategoryProduct/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoryProduct = await _context.CategoryProducts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (categoryProduct == null)
            {
                return NotFound();
            }

            return View(categoryProduct);
        }

        // POST: CategoryProduct/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var categoryProduct = await _context.CategoryProducts.FindAsync(id);
            if (categoryProduct != null)
            {
                _context.CategoryProducts.Remove(categoryProduct);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryProductExists(int id)
        {
            return _context.CategoryProducts.Any(e => e.Id == id);
        }
    }
}
