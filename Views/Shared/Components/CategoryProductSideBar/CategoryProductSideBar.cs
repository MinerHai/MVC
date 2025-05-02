
using App.Models.Product;
using Microsoft.AspNetCore.Mvc;

namespace App.Components{
    [ViewComponent]
    public class CategoryProductSideBar : ViewComponent{
        public class CategoryProductSideBarData{
            public List<CategoryProduct> Categories {set; get;}
            public string categorySlug{set; get;}
        }
        public IViewComponentResult Invoke(CategoryProductSideBarData data){
            return View(data);
        }
    }
}