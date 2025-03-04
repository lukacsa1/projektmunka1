using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace webshop.Models;

public partial class User
{
    public int Id { get; set; }

    public string LastName { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string? PhoneNumber { get; set; } = null!;

    public string LoginName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public int? SzamlazasiCimId { get; set; }

    public string Salt { get; set; } = null!;

    public string Hash { get; set; } = null!;

    public int Active { get; set; }

    public DateTime RegistarionDate { get; set; }

    public int PermissionLevel { get; set; }
    [JsonIgnore]
    public virtual ICollection<Order>? Orders { get; set; } = new List<Order>();

    public virtual Szamlazasicimek? SzamlazasiCim { get; set; }
}
