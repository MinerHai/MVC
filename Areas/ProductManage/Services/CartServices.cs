using App.Models.Product;
using Newtonsoft.Json;

public class CartServices
{
    public const string CARTKEY = "cart";
    private readonly IHttpContextAccessor? _context;
    private readonly HttpContext? _httpContext;
    public CartServices(IHttpContextAccessor? context)
    {
        _context = context;
        _httpContext = context?.HttpContext;
    }
    public List<CartItem> GetCartItems()
    {

        var session = _httpContext?.Session;
        string? jsoncart = session?.GetString(CARTKEY);
        if (jsoncart != null)
        {
            return JsonConvert.DeserializeObject<List<CartItem>>(jsoncart) ?? new List<CartItem>();
        }
        return new List<CartItem>();
    }
    public void ClearCart()
    {
        var session = _httpContext?.Session;
        session?.Remove(CARTKEY);
    }

    // Lưu Cart (Danh sách CartItem) vào session
    public void SaveCartSession(List<CartItem> ls)
    {
        var session = _httpContext?.Session;

        var settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        string jsoncart = JsonConvert.SerializeObject(ls, settings);
        session?.SetString(CARTKEY, jsoncart);
    }


}