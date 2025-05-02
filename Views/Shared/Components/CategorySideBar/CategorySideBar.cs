using App.Models;
using Microsoft.AspNetCore.Mvc;

namespace App.Components{
    [ViewComponent]
    public class CategorySideBar : ViewComponent{
        public class CategorySideBarData{
            public List<Category> Categories {set; get;}
            public string categorySlug{set; get;}
            public Dictionary<int, int> PostCounts { set; get; } = new Dictionary<int, int>();
            public List<Post> RecentPosts { set; get; } = new List<Post>(); // Thêm danh sách bài viết mới nhất
        }
        public IViewComponentResult Invoke(CategorySideBarData data){
            return View(data);
        }
    }
}