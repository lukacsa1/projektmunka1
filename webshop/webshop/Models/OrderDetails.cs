namespace webshop.Models
{
    public class OrderDetails
    {
        public string token { get; set; }

        public List<OrderProduct> product { get; set; }
    }
}
