using System;
using System.Collections.Generic;

namespace webshop.Models;

public partial class Termekek
{
    public int Id { get; set; }

    public string TermekNeve { get; set; } = null!;

    public string Meret { get; set; } = null!;

    public int Ar { get; set; }

    public string Kep { get; set; } = null!;

    public string Kategoria { get; set; } = null!;

    public virtual ICollection<Orderitem> Orderitems { get; set; } = new List<Orderitem>();
}
