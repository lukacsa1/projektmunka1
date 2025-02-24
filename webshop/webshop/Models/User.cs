using System;
using System.Collections.Generic;

namespace webshop.Models;

public partial class User
{
    public int Id { get; set; }

    public string LoginName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public int? SzamlazasiCimId { get; set; }

    public string Salt { get; set; } = null!;

    public string Hash { get; set; } = null!;

    public int Active { get; set; }

    public DateTime RegistarionDate { get; set; }

    public int PermissionLevel { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual Szamlazasicimek? SzamlazasiCim { get; set; }
}
