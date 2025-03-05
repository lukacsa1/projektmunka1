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
        public string Salt { get; set; }
        public string Hash { get; set; }
        public int? Active { get; set; }
        public DateTime RegistrationDate { get; set; }
        public int PermissionLevel { get; set; }
        public string SzamlazasiCim { get; set; }

    }
}
