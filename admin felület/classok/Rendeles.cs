using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace admin_felület.classok
{
    internal class Rendeles
    {
        public int Id { get; set; }
        public DateTime Datum { get; set; }
        public int Status { get; set; }
        public string OrderNumber { get; set; }
        public List<OrderItem> OrderItems { get; set; }  // Order tételek listája
    }
    internal class OrderItem
    {
        public int Id { get; set; }
        public int TermekId { get; set; }
        public string Meret { get; set; }
        public int Darabszam { get; set; }
    }
}
