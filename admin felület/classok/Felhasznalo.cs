using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace admin_felület.classok
{
    internal class Felhasznalo
    {
        public int Id { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string PhoneNumber { get; set; }
        public string LoginName { get; set; }
        public string Email { get; set; }
        public int? SzamlazasiCimId { get; set; }
        public int? Active { get; set; }
        public int PermissionLevel { get; set; }
        // Összetett típus a szállítási címre
        public SzamlazasiCim szamlazasiCim { get; set; }

        // Belső osztály a szállítási cím mezőhöz
        public class SzamlazasiCim
        {
            public int Id { get; set; }
            public string Orszag { get; set; }
            public string Varos { get; set; }
            public string Utca { get; set; }
            public int Hazszam { get; set; }
            public int Iranyitoszam { get; set; }
        }

    }
}
