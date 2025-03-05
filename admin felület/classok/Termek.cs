using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace admin_felület.classok
{
    internal class Termek
    {
        public int Id { get; set; }
        public string TermekNeve { get; set; }
        public string Meret { get; set; }
        public int Ar { get; set; }
        public string Kep { get; set; }
        public string Kategoria { get; set; }
    }
}
