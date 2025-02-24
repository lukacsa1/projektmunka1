using System;
using System.Collections.Generic;

namespace webshop.Models;

public partial class Order
{
    public int Id { get; set; }

    public int FelhasznaloId { get; set; }

    public DateTime Datum { get; set; }

    public int Status { get; set; }

    public string OrderNumber { get; set; } = null!;

    public virtual User Felhasznalo { get; set; } = null!;

    public virtual ICollection<Orderitem> Orderitems { get; set; } = new List<Orderitem>();
}
