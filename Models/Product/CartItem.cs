namespace App.Models.Product
{
    public class CartItem
    {
        public int quantity { set; get; }
        public ProductModel? product { set; get; }
    }
}