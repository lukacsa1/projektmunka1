using System;
using System.Collections.Generic;

namespace webshop.Models;

public partial class Orderitem
{
    public int Id { get; set; }

    public int RendelésId { get; set; }

    public int TermekId { get; set; }

    public string Meret { get; set; } = null!;

    public int Darabszam { get; set; }

    public virtual Order Rendelés { get; set; } = null!;

    public virtual Termekek Termek { get; set; } = null!;
}
