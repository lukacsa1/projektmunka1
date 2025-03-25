using System;
using System.Collections.Generic;

namespace webshop.Models;

public partial class Rendelesszamlaza
{
    public int Id { get; set; }

    public string Nev { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Telefonszam { get; set; } = null!;

    public string Orszag { get; set; } = null!;

    public string Varos { get; set; } = null!;

    public string Utca { get; set; } = null!;

    public string Hazszam { get; set; } = null!;

    public int Iranyitoszam { get; set; }
}
