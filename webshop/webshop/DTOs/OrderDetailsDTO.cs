namespace webshop.DTOs
{
    public class OrderDetailsDTO
    {
        public string token { get; set; }

        public List<OrderProductDTO> product { get; set; }
    }
}
