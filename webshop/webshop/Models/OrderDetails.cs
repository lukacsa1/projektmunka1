namespace webshop.Models
{
    public class OrderDetails
    {
        public string token { get; set; }
        public int productId { get; set; }
        public int amount { get; set; }
        public string size {  get; set; }
    }
}
