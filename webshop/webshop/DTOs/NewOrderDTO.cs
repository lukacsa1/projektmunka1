namespace webshop.DTOs
{
    public class NewOrderDTO
    {
        public BillingDTO billing { get; set; }
        public List<OrderProductDTO> orderProducts { get; set; }
    }
}
