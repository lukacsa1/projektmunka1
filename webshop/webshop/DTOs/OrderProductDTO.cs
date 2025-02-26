using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace webshop.DTOs
{
    public class OrderProductDTO
    {
        public int Id { get; set; }
        public string size { get; set; }
        public int amount { get; set; }
    }
}
