using webshop.Models;

namespace webshop.DTOs
{
    public class OrderDTO
    {
        public int Id { get; set; }
        public DateTime Datum { get; set; }
        public int Status { get; set; }
        public string OrderNumber { get; set; }
        public List<OrderItemsDTO> Orderitems { get; set; }
        public RendelesSzamlazasDTO? Szamlazas { get; set; }
    }
}
