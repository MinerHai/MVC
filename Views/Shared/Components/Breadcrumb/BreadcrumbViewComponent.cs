using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

public class BreadcrumbViewComponent : ViewComponent
{
    // Tùy chỉnh map controller/action -> tên hiển thị
    private readonly Dictionary<string, string> ControllerNames = new()
        {
            { "Home", "Home" },
            { "ViewPost", "Blog" },
            { "ViewProduct", "Products" }
        };

    private readonly Dictionary<string, string> ActionNames = new()
        {
            { "Index", "" },
            { "Detail", "Single Blog" },
            { "Details", "Product Details" }
        };

    public IViewComponentResult Invoke()
    {
        var breadcrumbs = new List<(string Title, string Url)>();

        var controller = RouteData.Values["controller"]?.ToString();
        var action = RouteData.Values["action"]?.ToString();
        var id = RouteData.Values.ContainsKey("id") ? RouteData.Values["id"]?.ToString() : null;

        var area = RouteData.Values["area"]?.ToString();
        if (string.IsNullOrEmpty(area) && controller == "Home" && action == "Index")
        {
            return Content(string.Empty); // hoặc: return View(new List<(string, string)>()); nhưng nhẹ hơn là dùng Content("")
        }
        // Add Trang chủ
        breadcrumbs.Add(("Home", Url.Action("Index", "Home", new { area = "MainPage" })));

        if (!string.IsNullOrEmpty(controller) && controller != "Home")
        {
            string controllerDisplay = ControllerNames.ContainsKey(controller) ? ControllerNames[controller] : controller;
            breadcrumbs.Add((controllerDisplay, Url.Action("Index", controller)));
        }

        if (!string.IsNullOrEmpty(action) && action != "Index")
        {
            string actionDisplay = ActionNames.ContainsKey(action) ? ActionNames[action] : action;

            // Nếu có ID thì thêm vào URL
            string url = id != null ? Url.Action(action, controller, new { id }) : Url.Action(action, controller);
            breadcrumbs.Add((actionDisplay, url));
        }

        return View(breadcrumbs);
    }
}
